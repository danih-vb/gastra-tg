"""Gerador de dados simulados para desenvolver e validar os algoritmos.

O restaurante colaborador não fornece base histórica, e o sistema ainda não acumulou movimento
próprio. Este módulo cria um histórico plausível e **determinístico** (mesma semente, mesmos dados),
para que os testes e a calibração dos pesos possam ser repetidos.

Nada aqui vai para produção: é insumo de teste e de experimentação nos notebooks.
"""

from __future__ import annotations

import random
from dataclasses import dataclass, field

# Itens do cardápio usados na simulação, com a chance de cada um aparecer numa comanda qualquer.
ITENS = {
    1: ("Moqueca", 0.30),
    2: ("Arroz de coco", 0.25),
    3: ("Caipirinha", 0.35),
    4: ("Pudim", 0.20),
    5: ("Salada", 0.15),
    6: ("Porção infantil", 0.10),
    7: ("Suco natural", 0.25),
    8: ("Café", 0.30),
}

# Combinações que o salão observa na prática: se o primeiro item entra, o segundo tende a entrar junto.
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


@dataclass
class HistoricoSimulado:
    comandas: list[ComandaSimulada] = field(default_factory=list)

    @property
    def transacoes(self) -> list[list[int]]:
        """Formato esperado pelo treinamento das regras de associação."""
        return [comanda.itens for comanda in self.comandas]


def gerar_historico(quantidade: int = 400, semente: int = 42) -> HistoricoSimulado:
    """Gera comandas com as combinações acima embutidas, mais ruído aleatório."""
    sorteio = random.Random(semente)
    historico = HistoricoSimulado()

    for _ in range(quantidade):
        itens = {item for item, (_, chance) in ITENS.items() if sorteio.random() < chance}

        for gatilho, acompanhamento, chance in COMBINACOES:
            if gatilho in itens and sorteio.random() < chance:
                itens.add(acompanhamento)

        if not itens:
            continue

        pessoas = sorteio.choices([1, 2, 3, 4, 5, 6], weights=[10, 35, 20, 20, 10, 5])[0]
        valor = round(sum(20 + sorteio.random() * 60 for _ in itens), 2)
        historico.comandas.append(ComandaSimulada(sorted(itens), pessoas, valor))

    return historico


@dataclass
class GarcomSimulado:
    id: int
    faturamento_acumulado: float
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
            faturamento_acumulado=round(sorteio.uniform(3_000, 12_000), 2),
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
