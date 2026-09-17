"""Calibração dos pesos da RN03 por simulação de muitos turnos (issue #55).

O restaurante não forneceu histórico e o sistema ainda não tem movimento real, então os pesos w1
(equilíbrio de faturamento) e w2 = 1 − w1 (espera por praça de alto potencial) são escolhidos
simulando um salão ao longo de vários turnos. Cada turno é alocado por uma política, e ao final medem-se
duas coisas, as mesmas que motivaram a RN03:

- **desigualdade:** coeficiente de Gini do faturamento médio por turno trabalhado de cada garçom
  (0 = todos iguais). É a "desigualdade percebida entre praças" relatada na pesquisa;
- **espera máxima:** a maior sequência de turnos que algum garçom trabalhou sem pegar praça de alto
  potencial. É a recorrência pedida pelo orientador.

As premissas do salão simulado ficam todas em `CenarioSimulado`, explícitas, para irem à metodologia
do TG. A simulação é determinística: mesma semente, mesmo resultado.
"""

from __future__ import annotations

import random
import statistics
from collections.abc import Callable, Sequence
from dataclasses import dataclass, field

from gastra_analitica.alocacao.programacao_linear import GarcomDisponivel, PracaDoTurno, sugerir_alocacao


@dataclass(frozen=True)
class CenarioSimulado:
    """Premissas do salão simulado."""

    quantidade_garcons: int = 7
    # (vagas, faturamento médio da praça por turno em R$): uma praça forte, uma média e uma fraca.
    pracas: tuple[tuple[int, float], ...] = ((3, 1800.0), (3, 1200.0), (2, 600.0))
    turnos: int = 120  # 60 dias com almoço e jantar
    presenca: float = 0.85  # chance de cada garçom trabalhar num turno (folgas e faltas)
    variacao_da_praca: float = 0.25  # desvio do faturamento da praça de um turno para outro
    variacao_do_garcom: float = 0.10  # diferença individual dentro da mesma praça
    janela_do_faturamento: int = 60  # 30 dias × 2 turnos, como no backend


@dataclass(frozen=True)
class EstadoDoTurno:
    """O que uma política enxerga para decidir o turno."""

    indice: int
    presentes: list[int]
    garcons: list[GarcomDisponivel]
    pracas: list[PracaDoTurno]


Politica = Callable[[EstadoDoTurno], dict[int, int]]


@dataclass(frozen=True)
class Metricas:
    gini: float
    espera_maxima: int


@dataclass
class _Garcom:
    id: int
    faturamento_por_turno: list[tuple[int, float]] = field(default_factory=list)  # (turno, valor)
    turnos_trabalhados: int = 0
    turnos_desde_alto_potencial: int = 0
    maior_espera: int = 0


def gini(valores: Sequence[float]) -> float:
    """Coeficiente de Gini (0 = igualdade total; perto de 1 = tudo com uma pessoa só)."""
    ordenados = sorted(valores)
    n = len(ordenados)
    total = sum(ordenados)
    if n == 0 or total == 0:
        return 0.0
    acumulado = sum((indice + 1) * valor for indice, valor in enumerate(ordenados))
    return (2 * acumulado) / (n * total) - (n + 1) / n


def simular(politica: Politica, cenario: CenarioSimulado = CenarioSimulado(), semente: int = 0) -> Metricas:
    sorteio = random.Random(semente)
    garcons = {numero: _Garcom(numero) for numero in range(1, cenario.quantidade_garcons + 1)}
    capacidade = sum(vagas for vagas, _ in cenario.pracas)
    faturamento_das_pracas = {indice + 1: [media] for indice, (_, media) in enumerate(cenario.pracas)}

    for turno in range(cenario.turnos):
        presentes = _sortear_presentes(sorteio, list(garcons), cenario, capacidade)
        medias = {praca: statistics.fmean(valores) for praca, valores in faturamento_das_pracas.items()}
        alto_potencial = {praca for praca, media in medias.items() if media > statistics.fmean(medias.values())}

        estado = EstadoDoTurno(
            indice=turno,
            presentes=presentes,
            garcons=[
                GarcomDisponivel(
                    id=numero,
                    # Média por turno trabalhado, e não soma: com a soma, quem falta aparece "atrás" e ganha
                    # praça boa por isso (resultado desta calibração, ver GASTRA_Calibracao_Pesos_RN03.md).
                    faturamento_por_turno=statistics.fmean([
                        valor for quando, valor in garcons[numero].faturamento_por_turno
                        if quando > turno - cenario.janela_do_faturamento] or [0.0]),
                    turnos_desde_praca_de_alto_potencial=garcons[numero].turnos_desde_alto_potencial,
                )
                for numero in presentes
            ],
            pracas=[
                PracaDoTurno(id=indice + 1, vagas=vagas, faturamento_medio_historico=medias[indice + 1])
                for indice, (vagas, _) in enumerate(cenario.pracas)
            ],
        )

        destino = politica(estado)

        for indice, (_, media) in enumerate(cenario.pracas):
            praca = indice + 1
            atendentes = [numero for numero in presentes if destino[numero] == praca]
            faturamento = media * max(0.2, sorteio.gauss(1, cenario.variacao_da_praca))
            faturamento_das_pracas[praca].append(faturamento)

            for numero in atendentes:
                parte = faturamento / len(atendentes) * max(0.5, sorteio.gauss(1, cenario.variacao_do_garcom))
                garcom = garcons[numero]
                garcom.faturamento_por_turno.append((turno, parte))
                garcom.turnos_trabalhados += 1
                if praca in alto_potencial:
                    garcom.turnos_desde_alto_potencial = 0
                else:
                    garcom.turnos_desde_alto_potencial += 1
                    garcom.maior_espera = max(garcom.maior_espera, garcom.turnos_desde_alto_potencial)

    media_por_turno = [
        sum(valor for _, valor in garcom.faturamento_por_turno) / garcom.turnos_trabalhados
        for garcom in garcons.values() if garcom.turnos_trabalhados
    ]
    return Metricas(gini=gini(media_por_turno), espera_maxima=max(g.maior_espera for g in garcons.values()))


def _sortear_presentes(sorteio: random.Random, todos: list[int], cenario: CenarioSimulado, capacidade: int) -> list[int]:
    presentes = [numero for numero in todos if sorteio.random() < cenario.presenca]
    faltando = [numero for numero in todos if numero not in presentes]
    sorteio.shuffle(faltando)
    # Toda praça precisa de alguém, e ninguém pode ficar sem vaga.
    while len(presentes) < len(cenario.pracas):
        presentes.append(faltando.pop())
    sorteio.shuffle(presentes)
    return sorted(presentes[:capacidade])


# --- Políticas ---

def politica_programacao_linear(peso_desequilibrio: float) -> Politica:
    """A RN03 como implementada: programação linear com w1 = peso_desequilibrio e w2 = 1 − w1."""
    peso_espera = round(1 - peso_desequilibrio, 10)

    def decidir(estado: EstadoDoTurno) -> dict[int, int]:
        resultado = sugerir_alocacao(estado.garcons, estado.pracas, peso_desequilibrio, peso_espera)
        return {d.garcom_id: d.praca_id for d in resultado.designacoes}

    return decidir


def politica_fixa(estado: EstadoDoTurno) -> dict[int, int]:
    """Sem regra: cada garçom tende a ficar sempre na mesma praça (os de número menor na praça forte)."""
    return _preencher_em_ordem(estado.presentes, estado.pracas)


def politica_rodizio(estado: EstadoDoTurno) -> dict[int, int]:
    """Rodízio simples: a ordem dos garçons gira um passo a cada turno."""
    deslocamento = estado.indice % len(estado.presentes)
    return _preencher_em_ordem(estado.presentes[deslocamento:] + estado.presentes[:deslocamento], estado.pracas)


def _preencher_em_ordem(ordem: list[int], pracas: list[PracaDoTurno]) -> dict[int, int]:
    """Um garçom por praça primeiro (nenhuma fica vazia), depois completa as vagas na ordem das praças."""
    destino: dict[int, int] = {}
    restantes = list(ordem)
    ocupacao = {praca.id: 0 for praca in pracas}
    for praca in pracas:
        if restantes:
            destino[restantes.pop(0)] = praca.id
            ocupacao[praca.id] += 1
    for praca in pracas:
        while restantes and ocupacao[praca.id] < praca.vagas:
            destino[restantes.pop(0)] = praca.id
            ocupacao[praca.id] += 1
    return destino


# --- Calibração ---

@dataclass(frozen=True)
class ResultadoDaPolitica:
    nome: str
    peso_desequilibrio: float | None
    gini_medio: float
    espera_maxima_media: float


def avaliar(nome: str, politica: Politica, sementes: Sequence[int], cenario: CenarioSimulado,
            peso_desequilibrio: float | None = None) -> ResultadoDaPolitica:
    metricas = [simular(politica, cenario, semente) for semente in sementes]
    return ResultadoDaPolitica(
        nome=nome,
        peso_desequilibrio=peso_desequilibrio,
        gini_medio=statistics.fmean(m.gini for m in metricas),
        espera_maxima_media=statistics.fmean(m.espera_maxima for m in metricas),
    )


def calibrar(
    pesos: Sequence[float] = tuple(round(passo / 10, 1) for passo in range(11)),
    sementes: Sequence[int] = tuple(range(20)),
    cenario: CenarioSimulado = CenarioSimulado(),
) -> list[ResultadoDaPolitica]:
    """Avalia cada peso e as políticas de referência (fixa e rodízio) nas mesmas sementes."""
    resultados = [avaliar(f"w1 = {peso:.1f}", politica_programacao_linear(peso), sementes, cenario, peso) for peso in pesos]
    resultados.append(avaliar("fixa (sem regra)", politica_fixa, sementes, cenario))
    resultados.append(avaliar("rodízio simples", politica_rodizio, sementes, cenario))
    return resultados


def escolher_peso(resultados: Sequence[ResultadoDaPolitica]) -> ResultadoDaPolitica:
    """
    Critério: as duas métricas têm o mesmo peso. Cada uma é normalizada entre o melhor e o pior valor dos
    pesos testados, e escolhe-se o peso com a menor média das duas. Em empate, o mais próximo de 0,5.
    """
    candidatos = [r for r in resultados if r.peso_desequilibrio is not None]

    def normalizar(valor: float, valores: list[float]) -> float:
        menor, maior = min(valores), max(valores)
        return 0.0 if maior == menor else (valor - menor) / (maior - menor)

    ginis = [r.gini_medio for r in candidatos]
    esperas = [r.espera_maxima_media for r in candidatos]

    return min(
        candidatos,
        key=lambda r: (
            round((normalizar(r.gini_medio, ginis) + normalizar(r.espera_maxima_media, esperas)) / 2, 6),
            abs(r.peso_desequilibrio - 0.5),
        ),
    )
