"""Leitura do histórico real pela view (D10)."""

import os
from datetime import date

import pytest
from sqlalchemy import create_engine, text
from sqlalchemy.exc import OperationalError, ProgrammingError

from gastra_analitica.dados.historico_banco import ConfiguracaoBanco, ler_transacoes


def test_sem_senha_no_ambiente_o_banco_fica_desligado():
    assert ConfiguracaoBanco.do_ambiente({"GASTRA_ANALITICA_BANCO_HOST": "db"}) is None


def test_configuracao_usa_valores_padrao_do_ambiente_local():
    configuracao = ConfiguracaoBanco.do_ambiente({"GASTRA_ANALITICA_BANCO_SENHA": "x"})

    assert (configuracao.host, configuracao.porta, configuracao.nome, configuracao.usuario) == (
        "localhost", 3307, "gastra_dev", "gastra_analitica")


def test_senha_nao_aparece_na_representacao_da_engine():
    engine = ConfiguracaoBanco.do_ambiente({"GASTRA_ANALITICA_BANCO_SENHA": "segredo-123"}).criar_engine()

    assert "segredo-123" not in str(engine.url)


@pytest.fixture
def view_em_memoria():
    """SQLite com uma tabela no formato da view, para testar só a leitura e o agrupamento."""
    engine = create_engine("sqlite://")
    with engine.begin() as conexao:
        conexao.execute(text("CREATE TABLE vw_itens_por_comanda (comanda_id INT, item_cardapio_id INT, data TEXT, quantidade INT)"))
        conexao.execute(text("""
            INSERT INTO vw_itens_por_comanda VALUES
                (2, 7, '2026-09-10', 1), (1, 3, '2026-01-05', 2), (2, 4, '2026-09-10', 1), (1, 1, '2026-01-05', 1)
        """))
    return engine


def test_agrupa_os_itens_por_comanda(view_em_memoria):
    assert ler_transacoes(view_em_memoria) == [[1, 3], [4, 7]]


def test_filtra_a_janela_de_historico(view_em_memoria):
    assert ler_transacoes(view_em_memoria, desde=date(2026, 6, 1)) == [[4, 7]]


# --- Usuário somente leitura no MySQL real (roda só com a senha dele no ambiente) ---

sem_banco_real = pytest.mark.skipif(
    not os.environ.get("GASTRA_ANALITICA_BANCO_SENHA"),
    reason="Crie o usuário com infra/criar-usuario-analitico.sh e defina GASTRA_ANALITICA_BANCO_SENHA.",
)


@sem_banco_real
def test_usuario_analitico_le_a_view():
    engine = ConfiguracaoBanco.do_ambiente().criar_engine()

    assert isinstance(ler_transacoes(engine), list)


@sem_banco_real
@pytest.mark.parametrize("comando", [
    "SELECT * FROM comanda",
    "SELECT email FROM usuario",
    "INSERT INTO registro_auditoria (data_hora_utc, evento, resultado) VALUES (NOW(), 'X', 'Sucesso')",
    "DELETE FROM item_pedido",
])
def test_usuario_analitico_nao_le_tabelas_nem_grava(comando):
    engine = ConfiguracaoBanco.do_ambiente().criar_engine()

    with pytest.raises((OperationalError, ProgrammingError), match="denied"):
        with engine.begin() as conexao:
            conexao.execute(text(comando))
