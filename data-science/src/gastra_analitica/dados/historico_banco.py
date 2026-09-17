"""Leitura do histórico real de pedidos (decisão D10 da arquitetura).

O Python só lê **views**, com um usuário MySQL que tem apenas `SELECT` nelas
(`infra/criar-usuario-analitico.sh`). As views não têm dado pessoal: só ids de comanda e de item.
Quem grava no banco é sempre o backend em C#.
"""

from __future__ import annotations

import os
from collections import OrderedDict
from dataclasses import dataclass
from datetime import date

from sqlalchemy import create_engine, text
from sqlalchemy.engine import URL, Engine

# Nomes das variáveis de ambiente. A senha nunca fica em arquivo versionado.
VARIAVEIS = {
    "host": "GASTRA_ANALITICA_BANCO_HOST",
    "porta": "GASTRA_ANALITICA_BANCO_PORTA",
    "nome": "GASTRA_ANALITICA_BANCO_NOME",
    "usuario": "GASTRA_ANALITICA_BANCO_USUARIO",
    "senha": "GASTRA_ANALITICA_BANCO_SENHA",
}


@dataclass(frozen=True)
class ConfiguracaoBanco:
    host: str
    porta: int
    nome: str
    usuario: str
    senha: str

    @classmethod
    def do_ambiente(cls, ambiente: dict[str, str] | None = None) -> ConfiguracaoBanco | None:
        """Lê a configuração das variáveis de ambiente. Sem senha, o banco fica desligado (volta ao simulado)."""
        ambiente = os.environ if ambiente is None else ambiente
        senha = ambiente.get(VARIAVEIS["senha"])
        if not senha:
            return None

        return cls(
            host=ambiente.get(VARIAVEIS["host"], "localhost"),
            porta=int(ambiente.get(VARIAVEIS["porta"], "3307")),
            nome=ambiente.get(VARIAVEIS["nome"], "gastra_dev"),
            usuario=ambiente.get(VARIAVEIS["usuario"], "gastra_analitica"),
            senha=senha,
        )

    def criar_engine(self) -> Engine:
        url = URL.create(
            "mysql+pymysql",
            username=self.usuario,
            password=self.senha,
            host=self.host,
            port=self.porta,
            database=self.nome,
        )
        # Tempo de conexão curto: se o banco não responde, o serviço volta ao simulado em vez de travar.
        return create_engine(url, pool_pre_ping=True, connect_args={"connect_timeout": 3})


CONSULTA_TRANSACOES = """
    SELECT comanda_id, item_cardapio_id
    FROM vw_itens_por_comanda
    {filtro}
    ORDER BY comanda_id, item_cardapio_id
"""


def ler_transacoes(engine: Engine, desde: date | None = None) -> list[list[int]]:
    """
    Uma lista de ids de itens por comanda fechada, no formato que o treinamento espera.

    `desde` limita a janela de histórico (ex.: últimos 180 dias), para o modelo acompanhar mudanças
    de cardápio e de sazonalidade.
    """
    filtro = "WHERE data >= :desde" if desde else ""
    parametros = {"desde": desde} if desde else {}

    comandas: OrderedDict[int, list[int]] = OrderedDict()
    with engine.connect() as conexao:
        for comanda_id, item_id in conexao.execute(text(CONSULTA_TRANSACOES.format(filtro=filtro)), parametros):
            comandas.setdefault(int(comanda_id), []).append(int(item_id))

    return list(comandas.values())
