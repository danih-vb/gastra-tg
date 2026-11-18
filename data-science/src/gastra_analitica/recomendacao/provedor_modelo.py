"""De onde vem o modelo de recomendação: histórico real do banco ou histórico simulado.

Regras:
- com banco configurado, acessível e com movimento suficiente, o modelo é treinado com o histórico real;
- em qualquer outro caso, usa o histórico simulado e informa o motivo;
- o modelo fica em cache por alguns minutos: treinar a cada chamada seria lento, e o histórico muda pouco
  entre duas consultas do garçom;
- vencido o prazo, a resposta sai na hora com o modelo anterior, e o novo é treinado em segundo plano. Treinar
  dentro da requisição chegou a passar dos 2 s que o backend espera, e o garçom via "sugestões indisponíveis"
  (achado dos testes operacionais).
"""

from __future__ import annotations

import logging
import threading
import time
from collections.abc import Callable, Sequence
from dataclasses import dataclass

from gastra_analitica.dados.simulador import gerar_historico
from gastra_analitica.recomendacao.segmentada import ModeloSegmentado, treinar_segmentado

logger = logging.getLogger(__name__)

# Abaixo disso, as regras de associação não têm base estatística: poucas comandas geram regras por acaso.
MINIMO_DE_COMANDAS = 50
CACHE_SEGUNDOS = 300


@dataclass(frozen=True)
class ModeloComOrigem:
    modelo: ModeloSegmentado
    origem: str  # "banco" ou "simulado"
    motivo: str | None = None  # por que caiu no simulado
    comandas_usadas: int = 0


class ProvedorDeModelo:
    """
    `ler_historico` devolve as transações do banco ou `None` quando o banco não está configurado.
    O relógio e o executor em segundo plano são injetáveis para testar o cache sem esperar nem abrir thread.
    """

    def __init__(
        self,
        ler_historico: Callable[[], Sequence[Sequence[int]] | None],
        minimo_de_comandas: int = MINIMO_DE_COMANDAS,
        cache_segundos: float = CACHE_SEGUNDOS,
        relogio: Callable[[], float] = time.monotonic,
        em_segundo_plano: Callable[[Callable[[], None]], None] | None = None,
    ) -> None:
        self._ler_historico = ler_historico
        self._minimo = minimo_de_comandas
        self._cache_segundos = cache_segundos
        self._relogio = relogio
        self._em_segundo_plano = em_segundo_plano or (lambda tarefa: threading.Thread(target=tarefa, daemon=True).start())
        self._atual: ModeloComOrigem | None = None
        self._calculado_em = 0.0
        self._simulado: CacheDoModeloSimulado = CacheDoModeloSimulado()
        self._trava = threading.Lock()
        self._atualizando = False

    def obter(self) -> ModeloComOrigem:
        if self._atual is None:
            # Só sem modelo nenhum a requisição espera: na subida do serviço, aquecer() já cuida disso.
            with self._trava:
                if self._atual is None:
                    self._atual, self._calculado_em = self._calcular(), self._relogio()
        elif self._relogio() - self._calculado_em >= self._cache_segundos:
            self._atualizar_em_segundo_plano()
        return self._atual

    def aquecer(self) -> None:
        """Treina o primeiro modelo antes da primeira comanda pedir sugestão."""
        self._em_segundo_plano(self.obter)

    def invalidar(self) -> None:
        self._atual = None

    def _atualizar_em_segundo_plano(self) -> None:
        with self._trava:
            if self._atualizando:
                return
            self._atualizando = True

        def atualizar() -> None:
            try:
                novo = self._calcular()
                self._atual, self._calculado_em = novo, self._relogio()
            finally:
                self._atualizando = False

        self._em_segundo_plano(atualizar)

    def _calcular(self) -> ModeloComOrigem:
        try:
            historico = self._ler_historico()
        except Exception as erro:  # banco fora do ar não pode derrubar a recomendação (D3)
            logger.warning("Histórico do banco indisponível, usando o simulado: %s", type(erro).__name__)
            return self._simulado.obter("banco indisponível")

        if historico is None:
            return self._simulado.obter("banco não configurado")

        comandas_uteis = [comanda for comanda in historico if len(set(comanda)) > 1]
        if len(comandas_uteis) < self._minimo:
            return self._simulado.obter(
                f"histórico real insuficiente ({len(comandas_uteis)} de {self._minimo} comandas com 2 ou mais itens)"
            )

        return ModeloComOrigem(treinar_segmentado(comandas_uteis), "banco", comandas_usadas=len(comandas_uteis))


class CacheDoModeloSimulado:
    """O modelo simulado é sempre o mesmo (semente fixa): treina uma vez só."""

    def __init__(self) -> None:
        self._modelo: ModeloSegmentado | None = None
        self._comandas = 0

    def obter(self, motivo: str) -> ModeloComOrigem:
        if self._modelo is None:
            transacoes = gerar_historico().transacoes
            self._modelo = treinar_segmentado(transacoes)
            self._comandas = len(transacoes)
        return ModeloComOrigem(self._modelo, "simulado", motivo, self._comandas)
