"""Gerador de dados simulados para desenvolver e validar os algoritmos.

O restaurante colaborador não fornece base histórica, e o sistema ainda não acumulou movimento
próprio. Este módulo cria um histórico plausível e **determinístico** (mesma semente, mesmos dados),
para que os testes e a calibração dos pesos possam ser repetidos.

Nada aqui vai para produção: é insumo de teste e de experimentação nos notebooks.
"""

from __future__ import annotations

import random
from dataclasses import dataclass, field

# Itens do cardápio usados na simulação. Os ids 1 a 8 são os de sempre (testes e notebooks dependem deles); os
# demais completam o cardápio do semeador, para que os perfis abaixo tenham itens próprios.
ITENS = {
    1: "Moqueca",
    2: "Arroz de coco",
    3: "Caipirinha",
    4: "Pudim",
    5: "Salada",
    6: "Porção infantil",
    7: "Suco natural",
    8: "Café",
    9: "Bolinho de bacalhau",
    10: "Bobó de camarão",
    11: "Risoto de cogumelos",
    12: "Petit gâteau",
    13: "Água com gás",
    14: "Sorvete de tapioca",
}

# Perfis de consumo plantados: cada comanda nasce de um perfil, que dá a chance de cada item entrar. É o gabarito
# da clusterização (RF09): o algoritmo recebe só os itens pedidos e precisa redescobrir estes três grupos.
# Item fora da lista do perfil entra com CHANCE_FORA_DO_PERFIL.
PERFIS = {
    "executivo": {5: 0.55, 11: 0.50, 7: 0.45, 13: 0.40, 8: 0.45},
    "frutos_do_mar": {1: 0.60, 10: 0.35, 9: 0.50, 3: 0.45, 12: 0.35},
    "familia": {6: 0.75, 7: 0.40, 2: 0.30, 4: 0.45, 14: 0.45},
}
CHANCE_FORA_DO_PERFIL = 0.03

# Quem senta à mesa muda o perfil mais provável: no almoço a dois, o executivo; no jantar, os frutos do mar; com
# três ou mais pessoas, a família. Período e pessoas escolhem o perfil, mas NÃO entram na clusterização (RN05):
# servem só para interpretar os grupos que ela encontra.
PESOS_DOS_PERFIS = {
    ("almoco", "ate_dois"): {"executivo": 65, "frutos_do_mar": 25, "familia": 10},
    ("almoco", "tres_ou_mais"): {"executivo": 15, "frutos_do_mar": 25, "familia": 60},
    ("jantar", "ate_dois"): {"executivo": 15, "frutos_do_mar": 75, "familia": 10},
    ("jantar", "tres_ou_mais"): {"executivo": 5, "frutos_do_mar": 45, "familia": 50},
}

# Combinações que o salão observa na prática: se o primeiro item entra, o segundo tende a entrar junto. Valem em
# qualquer perfil, e são o gabarito das regras de associação.
COMBINACOES = [
    (1, 2, 0.85),  # moqueca puxa arroz de coco
    (1, 3, 0.60),  # moqueca puxa caipirinha
    (4, 8, 0.70),  # sobremesa puxa café
    (6, 7, 0.65),  # porção infantil puxa suco natural
]


@dataclass
class ComandaSimulada:
    itens: list[int]
    quantidade_pessoas: int
    valor_total: float
    periodo: str = "almoco"
    perfil: str = ""


@dataclass
class HistoricoSimulado:
    comandas: list[ComandaSimulada] = field(default_factory=list)

    @property
    def transacoes(self) -> list[list[int]]:
        """Formato esperado pelo treinamento das regras de associação e pela clusterização."""
        return [comanda.itens for comanda in self.comandas]

    @property
    def perfis(self) -> list[str]:
        """O perfil que gerou cada comanda: o gabarito para medir a clusterização."""
        return [comanda.perfil for comanda in self.comandas]


def sortear_perfil(sorteio: random.Random, periodo: str, pessoas: int) -> str:
    pesos = PESOS_DOS_PERFIS[(periodo, "ate_dois" if pessoas <= 2 else "tres_ou_mais")]
    return sorteio.choices(list(pesos), weights=list(pesos.values()))[0]


def gerar_historico(quantidade: int = 400, semente: int = 42) -> HistoricoSimulado:
    """Gera comandas a partir dos perfis, com as combinações acima embutidas e ruído aleatório."""
    sorteio = random.Random(semente)
    historico = HistoricoSimulado()

    for _ in range(quantidade):
        periodo = sorteio.choice(["almoco", "jantar"])
        pessoas = sorteio.choices([1, 2, 3, 4, 5, 6], weights=[10, 35, 20, 20, 10, 5])[0]
        perfil = sortear_perfil(sorteio, periodo, pessoas)
        chances = PERFIS[perfil]

        itens = {item for item in ITENS if sorteio.random() < chances.get(item, CHANCE_FORA_DO_PERFIL)}

        for gatilho, acompanhamento, chance in COMBINACOES:
            if gatilho in itens and sorteio.random() < chance:
                itens.add(acompanhamento)

        if not itens:
            continue

        valor = round(sum(20 + sorteio.random() * 60 for _ in itens), 2)
        historico.comandas.append(ComandaSimulada(sorted(itens), pessoas, valor, periodo, perfil))

    return historico


@dataclass
class GarcomSimulado:
    id: int
    faturamento_por_turno: float
    turnos_desde_praca_de_alto_potencial: int


@dataclass
class PracaSimulada:
    id: int
    vagas: int
    faturamento_medio_historico: float


def gerar_quadro_do_turno(
    quantidade_garcons: int = 6,
    semente: int = 42,
) -> tuple[list[GarcomSimulado], list[PracaSimulada]]:
    """Monta um turno de exemplo: garçons com históricos diferentes e três praças."""
    sorteio = random.Random(semente)

    garcons = [
        GarcomSimulado(
            id=numero,
            faturamento_por_turno=round(sorteio.uniform(3_000, 12_000), 2),
            turnos_desde_praca_de_alto_potencial=sorteio.randint(0, 10),
        )
        for numero in range(1, quantidade_garcons + 1)
    ]

    pracas = [
        PracaSimulada(id=1, vagas=2, faturamento_medio_historico=1_800.0),  # alto potencial
        PracaSimulada(id=2, vagas=2, faturamento_medio_historico=1_200.0),
        PracaSimulada(id=3, vagas=2, faturamento_medio_historico=700.0),
    ]

    return garcons, pracas
