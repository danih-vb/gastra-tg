"""Evolução da desigualdade turno a turno, partindo do histórico do banco (issue #221).

A calibração (`calibracao.py`) parte de um salão sintético e mede o resultado no fim de 120 turnos. Aqui o ponto de
partida é o **estado de um banco de verdade** — no TG, o semeado —, e o que se mede é a **curva**: o Gini do
faturamento médio por turno de cada garçom nos últimos 30 dias, recalculado a cada turno, enquanto uma política
aloca os garçons. É o que a RN03 promete: não só escolher bem um turno, mas corrigir a desigualdade com o tempo.

Do banco vêm só coisas observáveis pelas views:

- o faturamento por turno de cada garçom nos últimos 30 dias (a janela da RN03) e há quantos turnos ele não pega
  praça de alto potencial;
- o **fator de venda** de cada garçom, estimado como o ticket médio das comandas dele dividido pelo ticket médio do
  restaurante — quem sugere entrada, sobremesa e a segunda rodada tem ticket maior;
- o movimento de cada praça (média e desvio de comandas por turno) e o ticket médio do restaurante.

A cada turno simulado: sorteia-se quem está presente (85%, como na calibração), a política distribui, cada praça
recebe o seu movimento, as comandas da praça são divididas entre os garçons dela, e cada comanda vale o ticket médio
vezes o fator do garçom, com variação.
"""

from __future__ import annotations

import random
import statistics
from collections.abc import Sequence
from dataclasses import dataclass

from gastra_analitica.alocacao.calibracao import EstadoDoTurno, Politica, gini
from gastra_analitica.alocacao.programacao_linear import GarcomDisponivel, PracaDoTurno

JANELA_EM_TURNOS = 60  # 30 dias com almoço e jantar, a mesma janela da RN03
PRESENCA = 0.85
VARIACAO_DA_COMANDA = 0.30


@dataclass(frozen=True)
class GarcomInicial:
    id: int
    fator_de_venda: float
    # Faturamento de cada turno trabalhado na janela, do mais antigo ao mais recente, com o índice do turno
    # (negativo: antes do início da simulação).
    historico: tuple[tuple[int, float], ...]
    turnos_desde_alto_potencial: int


@dataclass(frozen=True)
class PracaInicial:
    id: int
    vagas: int
    comandas_por_turno: float
    desvio_de_comandas: float


@dataclass(frozen=True)
class EstadoInicial:
    garcons: tuple[GarcomInicial, ...]
    pracas: tuple[PracaInicial, ...]
    ticket_medio: float


def _media_na_janela(historico: list[tuple[int, float]], turno: int) -> float:
    valores = [valor for quando, valor in historico if quando > turno - JANELA_EM_TURNOS]
    return statistics.fmean(valores) if valores else 0.0


def gini_da_janela(historicos: dict[int, list[tuple[int, float]]], turno: int) -> float:
    """Desigualdade percebida no turno: Gini do faturamento médio por turno de cada garçom nos últimos 30 dias."""
    medias = [_media_na_janela(h, turno) for h in historicos.values()]
    return gini([m for m in medias if m > 0])


def evoluir(politica: Politica, inicio: EstadoInicial, turnos: int = 120, semente: int = 0) -> list[float]:
    """
    Aplica a política por `turnos` turnos a partir do estado do banco. Devolve o Gini antes do primeiro turno e
    depois de cada um (`turnos + 1` valores). Mesma semente, mesma curva.
    """
    sorteio = random.Random(semente)
    fatores = {g.id: g.fator_de_venda for g in inicio.garcons}
    historicos = {g.id: list(g.historico) for g in inicio.garcons}
    espera = {g.id: g.turnos_desde_alto_potencial for g in inicio.garcons}
    potencial = {p.id: p.comandas_por_turno * inicio.ticket_medio for p in inicio.pracas}
    alto = {praca for praca, valor in potencial.items() if valor > statistics.fmean(potencial.values())}
    capacidade = sum(p.vagas for p in inicio.pracas)
    todos = sorted(fatores)

    curva = [gini_da_janela(historicos, 0)]
    for turno in range(turnos):
        presentes = [g for g in todos if sorteio.random() < PRESENCA]
        faltando = [g for g in todos if g not in presentes]
        sorteio.shuffle(faltando)
        while len(presentes) < len(inicio.pracas):  # toda praça precisa de alguém
            presentes.append(faltando.pop())
        presentes = sorted(presentes[:capacidade])

        estado = EstadoDoTurno(
            indice=turno,
            presentes=presentes,
            garcons=[GarcomDisponivel(g, _media_na_janela(historicos[g], turno), espera[g]) for g in presentes],
            pracas=[PracaDoTurno(p.id, p.vagas, potencial[p.id]) for p in inicio.pracas],
        )
        destino = politica(estado)

        for praca in inicio.pracas:
            equipe = [g for g in presentes if destino[g] == praca.id]
            if not equipe:
                continue
            comandas = max(1, round(sorteio.gauss(praca.comandas_por_turno, praca.desvio_de_comandas)))
            ganho = {g: 0.0 for g in equipe}
            for indice in range(comandas):
                garcom = equipe[indice % len(equipe)]
                ganho[garcom] += inicio.ticket_medio * fatores[garcom] * max(0.3, sorteio.gauss(1, VARIACAO_DA_COMANDA))
            for garcom, valor in ganho.items():
                historicos[garcom].append((turno, valor))
                espera[garcom] = 0 if praca.id in alto else espera[garcom] + 1

        curva.append(gini_da_janela(historicos, turno + 1))
    return curva


def curva_media(politica: Politica, inicio: EstadoInicial, sementes: Sequence[int], turnos: int = 120) -> list[float]:
    """Média, turno a turno, das curvas de várias sementes."""
    curvas = [evoluir(politica, inicio, turnos, semente) for semente in sementes]
    return [statistics.fmean(valores) for valores in zip(*curvas)]
