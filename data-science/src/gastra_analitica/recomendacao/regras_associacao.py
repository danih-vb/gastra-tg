"""Recomendação de pratos por regras de associação (RF09).

Entram apenas **padrões de consumo observáveis** — o que foi pedido junto em comandas anteriores.
Nenhum atributo pessoal do cliente é usado, nem pode ser (RN05).

Vocabulário:

- **suporte** de um conjunto de itens: em que fração das comandas ele aparece;
- **confiança** da regra A → B: entre as comandas que têm A, em quantas também aparece B;
- **lift**: quantas vezes A e B aparecem juntos além do esperado se fossem independentes.
  Lift 1 significa "nenhuma relação"; abaixo de 1, os itens se evitam.
"""

from __future__ import annotations

from collections.abc import Iterable, Sequence
from dataclasses import dataclass

import pandas as pd
from mlxtend.frequent_patterns import apriori, association_rules
from mlxtend.preprocessing import TransactionEncoder

SUPORTE_MINIMO_PADRAO = 0.05
CONFIANCA_MINIMA_PADRAO = 0.30

# Lift 1 significa "aparecem juntos só por acaso". Abaixo disso, os itens se evitam — sugerir seria
# pior do que não sugerir nada.
LIFT_MINIMO_PADRAO = 1.0


@dataclass(frozen=True)
class Regra:
    """Regra "quem pediu os itens de `antecedente` costuma pedir `consequente`"."""

    antecedente: frozenset[int]
    consequente: int
    suporte: float
    confianca: float
    lift: float


@dataclass(frozen=True)
class Sugestao:
    item_id: int
    confianca: float
    lift: float


class ModeloRecomendacao:
    """Conjunto de regras já apuradas, pronto para sugerir."""

    def __init__(self, regras: Sequence[Regra]):
        self.regras = list(regras)

    def __len__(self) -> int:
        return len(self.regras)

    def sugerir(
        self,
        itens_da_comanda: Iterable[int],
        limite: int = 3,
        itens_disponiveis: Iterable[int] | None = None,
    ) -> list[Sugestao]:
        """
        Sugere itens para uma comanda em andamento.

        Uma regra vale quando todo o seu antecedente já está na comanda e o consequente ainda não.
        Entre as regras que valem, ganha a de maior confiança; o lift desempata.
        """
        pedidos = set(itens_da_comanda)
        disponiveis = set(itens_disponiveis) if itens_disponiveis is not None else None

        candidatas: dict[int, Sugestao] = {}
        for regra in self.regras:
            if not regra.antecedente.issubset(pedidos) or regra.consequente in pedidos:
                continue
            if disponiveis is not None and regra.consequente not in disponiveis:
                continue

            melhor = candidatas.get(regra.consequente)
            if melhor is None or (regra.confianca, regra.lift) > (melhor.confianca, melhor.lift):
                candidatas[regra.consequente] = Sugestao(regra.consequente, regra.confianca, regra.lift)

        ordenadas = sorted(candidatas.values(), key=lambda s: (-s.confianca, -s.lift, s.item_id))
        return ordenadas[:limite]


def treinar(
    comandas: Sequence[Sequence[int]],
    suporte_minimo: float = SUPORTE_MINIMO_PADRAO,
    confianca_minima: float = CONFIANCA_MINIMA_PADRAO,
    lift_minimo: float = LIFT_MINIMO_PADRAO,
) -> ModeloRecomendacao:
    """
    Apura as regras a partir do histórico de comandas.

    Cada comanda é a lista de ids de itens do cardápio que ela levou. Comandas com menos de dois
    itens não dizem nada sobre combinação e são descartadas.
    """
    transacoes = [sorted(set(comanda)) for comanda in comandas if len(set(comanda)) > 1]
    if not transacoes:
        return ModeloRecomendacao([])

    codificador = TransactionEncoder()
    matriz = codificador.fit(transacoes).transform(transacoes)
    tabela = pd.DataFrame(matriz, columns=codificador.columns_)

    frequentes = apriori(tabela, min_support=suporte_minimo, use_colnames=True)
    if frequentes.empty:
        return ModeloRecomendacao([])

    encontradas = association_rules(frequentes, metric="confidence", min_threshold=confianca_minima)

    regras = [
        Regra(
            antecedente=frozenset(int(i) for i in linha.antecedents),
            consequente=int(next(iter(linha.consequents))),
            suporte=float(linha.support),
            confianca=float(linha.confidence),
            lift=float(linha.lift),
        )
        # Um consequente por regra: a sugestão é sempre "peça também este item".
        for linha in encontradas.itertuples()
        if len(linha.consequents) == 1 and float(linha.lift) > lift_minimo
    ]

    return ModeloRecomendacao(regras)
