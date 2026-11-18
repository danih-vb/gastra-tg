"""Alocação de garçons às praças por programação linear (RF06, RN03).

É um **problema de designação**: cada garçom vai para exatamente uma praça, e cada praça recebe no
máximo o número de garçons que comporta. Minimiza-se o custo total das designações escolhidas.

O custo de colocar o garçom *i* na praça *j* combina os dois critérios da RN03, ambos normalizados
em [0, 1] antes de somar — senão reais e turnos, que têm grandezas diferentes, distorceriam o peso:

    c(i,j) = w1 · desequilíbrio(i,j) + w2 · espera(i,j),  com w1 + w2 = 1

- **desequilíbrio:** faturamento médio por turno do garçom (últimos 30 dias) × potencial da praça, ambos
  normalizados. É média, e não soma: com a soma, quem faltou mais parece ter faturado menos e recebe praça
  boa por isso (calibração #55).
  Juntar "quem mais faturou" com "a praça que mais fatura" fica caro, então a solução de menor custo
  entrega a praça boa a quem está para trás.
- **espera:** (1 − espera do garçom) × potencial da praça. Quem está há mais turnos sem pegar praça
  de alto potencial custa menos nela.

Os dois são **produtos**, e não somas, de propósito: somar faturamento do garçom com o da praça dá o
mesmo total em qualquer distribuição — o solver empata e a regra não decide nada.

A matriz de um problema de designação é totalmente unimodular, então a relaxação contínua já sai
inteira: não é preciso solver de programação inteira (nota dos KPIs).
"""

from __future__ import annotations

from collections.abc import Sequence
from dataclasses import dataclass

import cbcbox
import pulp

# Calibrados por simulação na issue #55 (docs/analises/GASTRA_Calibracao_Pesos_RN03.md).
PESO_DESEQUILIBRIO_PADRAO = 0.6
PESO_ESPERA_PADRAO = 0.4


@dataclass(frozen=True)
class GarcomDisponivel:
    id: int
    faturamento_por_turno: float
    turnos_desde_praca_de_alto_potencial: int


@dataclass(frozen=True)
class PracaDoTurno:
    id: int
    vagas: int
    faturamento_medio_historico: float


@dataclass(frozen=True)
class Designacao:
    garcom_id: int
    praca_id: int
    custo: float


@dataclass(frozen=True)
class ResultadoAlocacao:
    designacoes: list[Designacao]
    custo_total: float
    peso_desequilibrio: float
    peso_espera: float


class AlocacaoInviavelError(ValueError):
    """Não existe distribuição possível com os garçons e as vagas informados."""


def _normalizar_por_chave(valores: dict[int, float]) -> dict[int, float]:
    """Leva os valores para [0, 1]. Se são todos iguais, viram 0: esse critério não desempata nada."""
    if not valores:
        return {}

    menor, maior = min(valores.values()), max(valores.values())
    if maior == menor:
        return {chave: 0.0 for chave in valores}

    return {chave: (valor - menor) / (maior - menor) for chave, valor in valores.items()}


def montar_matriz_de_custo(
    garcons: Sequence[GarcomDisponivel],
    pracas: Sequence[PracaDoTurno],
    peso_desequilibrio: float = PESO_DESEQUILIBRIO_PADRAO,
    peso_espera: float = PESO_ESPERA_PADRAO,
) -> dict[tuple[int, int], float]:
    """Custo de cada par (garçom, praça), já com os dois critérios normalizados e ponderados."""
    if abs(peso_desequilibrio + peso_espera - 1) > 1e-9:
        raise ValueError("Os pesos w1 e w2 precisam somar 1.")

    faturamento_do_garcom = _normalizar_por_chave({g.id: g.faturamento_por_turno for g in garcons})
    espera_do_garcom = _normalizar_por_chave({g.id: g.turnos_desde_praca_de_alto_potencial for g in garcons})
    potencial_da_praca = _normalizar_por_chave({p.id: p.faturamento_medio_historico for p in pracas})

    return {
        (garcom.id, praca.id):
            peso_desequilibrio * faturamento_do_garcom[garcom.id] * potencial_da_praca[praca.id]
            + peso_espera * (1 - espera_do_garcom[garcom.id]) * potencial_da_praca[praca.id]
        for garcom in garcons
        for praca in pracas
    }


def sugerir_alocacao(
    garcons: Sequence[GarcomDisponivel],
    pracas: Sequence[PracaDoTurno],
    peso_desequilibrio: float = PESO_DESEQUILIBRIO_PADRAO,
    peso_espera: float = PESO_ESPERA_PADRAO,
) -> ResultadoAlocacao:
    """Resolve a designação e devolve o par garçom → praça de menor custo total."""
    if not garcons or not pracas:
        raise AlocacaoInviavelError("Informe pelo menos um garçom e uma praça.")

    vagas = sum(praca.vagas for praca in pracas)
    if vagas < len(garcons):
        raise AlocacaoInviavelError(
            f"As praças comportam {vagas} garçons, mas há {len(garcons)} no turno.")

    custo = montar_matriz_de_custo(garcons, pracas, peso_desequilibrio, peso_espera)

    problema = pulp.LpProblem("alocacao_de_garcons", pulp.LpMinimize)

    # add_variable é a forma indicada a partir do PuLP 3.3 (LpVariable direto sai no PuLP 4.0).
    escolha = {
        (garcom.id, praca.id): problema.add_variable(f"x_{garcom.id}_{praca.id}", lowBound=0, upBound=1)
        for garcom in garcons
        for praca in pracas
    }

    problema += pulp.lpSum(custo[chave] * variavel for chave, variavel in escolha.items())

    for garcom in garcons:
        problema += pulp.lpSum(escolha[(garcom.id, praca.id)] for praca in pracas) == 1

    for praca in pracas:
        atendentes = pulp.lpSum(escolha[(garcom.id, praca.id)] for garcom in garcons)
        problema += atendentes <= praca.vagas

        # Com gente suficiente, nenhuma praça fica sem atendimento.
        if len(garcons) >= len(pracas):
            problema += atendentes >= 1

    # O solver embutido do PuLP está obsoleto; o CBC vem no pacote cbcbox.
    solver = pulp.COIN_CMD(path=cbcbox.cbc_bin_path(), msg=0)
    problema.solve(solver)

    if pulp.LpStatus[problema.status] != "Optimal":
        raise AlocacaoInviavelError(f"Não foi possível resolver a alocação: {pulp.LpStatus[problema.status]}.")

    designacoes = [
        Designacao(garcom_id=chave[0], praca_id=chave[1], custo=round(custo[chave], 4))
        for chave, variavel in escolha.items()
        if variavel.value() is not None and variavel.value() > 0.5
    ]
    designacoes.sort(key=lambda d: (d.praca_id, d.garcom_id))

    return ResultadoAlocacao(
        designacoes=designacoes,
        custo_total=round(sum(d.custo for d in designacoes), 4),
        peso_desequilibrio=peso_desequilibrio,
        peso_espera=peso_espera,
    )
