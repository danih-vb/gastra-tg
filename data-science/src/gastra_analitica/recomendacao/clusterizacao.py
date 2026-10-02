"""Clusterização das comandas por padrão de consumo (RF09).

Cada comanda vira um vetor com 1 nos itens que ela levou e 0 nos demais — só o que foi pedido, nunca atributo do
cliente (RN05). O K-Means agrupa as comandas parecidas; cada grupo é um **perfil de consumo**.

Duas escolhas sustentam o método:

- **Vetores normalizados.** Cada vetor é dividido pelo próprio tamanho antes do K-Means. Assim a distância passa a
  medir a *proporção* do consumo que duas comandas compartilham (similaridade do cosseno), e uma mesa que pediu
  seis itens não fica longe de uma que pediu três só por ter pedido mais.
- **Quantidade de perfis escolhida pelos dados.** O K-Means precisa saber quantos grupos procurar. Testam-se de
  2 a 6, e fica a quantidade com maior **coeficiente de silhueta** — que mede, para cada comanda, o quanto ela está
  mais perto do próprio grupo do que do grupo vizinho (de -1 a 1; abaixo de 0,25, não há estrutura substancial).
"""

from __future__ import annotations

from collections.abc import Sequence
from dataclasses import dataclass

import numpy as np
from sklearn.cluster import KMeans
from sklearn.metrics import silhouette_score

SEMENTE = 42
QUANTIDADE_MINIMA_DE_PERFIS = 2
QUANTIDADE_MAXIMA_DE_PERFIS = 6

# Abaixo de 0,25 a silhueta indica que não há estrutura substancial (Kaufman e Rousseeuw, "Finding Groups in
# Data", 1990): os "perfis" seriam só um jeito de fatiar o salão, e a recomendação fica com as regras gerais.
SILHUETA_MINIMA = 0.25

# Quantos itens descrevem um perfil: os que mais se destacam nele em relação ao restaurante inteiro.
ITENS_QUE_DESCREVEM = 4


@dataclass(frozen=True)
class ItemMarcante:
    """Um item que caracteriza o perfil: aparece `destaque` vezes mais nele do que no restaurante todo."""

    item_id: int
    presenca: float  # fração das comandas do perfil que levaram o item
    destaque: float


@dataclass(frozen=True)
class Perfil:
    id: int
    comandas: int
    participacao: float
    itens_marcantes: tuple[ItemMarcante, ...]


class ModeloDePerfis:
    """Perfis encontrados e os centros de cada um, para dizer a que perfil uma comanda em andamento pertence."""

    def __init__(
        self,
        itens: Sequence[int],
        centros: np.ndarray,
        perfis: Sequence[Perfil],
        rotulos: Sequence[int],
        silhueta: float,
        silhuetas_testadas: dict[int, float],
        presenca: np.ndarray | None = None,
        destaque: np.ndarray | None = None,
    ) -> None:
        self.itens = list(itens)
        self._coluna = {item: indice for indice, item in enumerate(self.itens)}
        self._centros = centros / np.maximum(np.linalg.norm(centros, axis=1, keepdims=True), 1e-12)
        self.perfis = list(perfis)
        self.rotulos = list(rotulos)
        self.silhueta = silhueta
        self.silhuetas_testadas = dict(silhuetas_testadas)
        self._presenca = presenca
        self._destaque = destaque

    def caracteristicos(self, perfil: int) -> list[ItemMarcante]:
        """
        Itens que caracterizam o perfil, do mais ao menos frequente nele: só os que aparecem mais no perfil do que
        no restaurante inteiro (destaque acima de 1). É o que a clusterização sabe sugerir sem regra nenhuma: "mesas
        como esta costumam pedir".
        """
        if self._presenca is None or self._destaque is None:
            return []
        linha_p, linha_d = self._presenca[perfil], self._destaque[perfil]
        ordem = sorted(range(len(self.itens)), key=lambda i: (-linha_p[i], self.itens[i]))
        return [
            ItemMarcante(self.itens[i], round(float(linha_p[i]), 4), round(float(linha_d[i]), 3))
            for i in ordem
            if linha_d[i] > 1
        ]

    @property
    def tem_estrutura(self) -> bool:
        """Falso quando os dados não formam grupos: aí não faz sentido segmentar a recomendação."""
        return self.silhueta >= SILHUETA_MINIMA

    def perfil_de(self, itens_da_comanda: Sequence[int]) -> int | None:
        """
        Perfil mais parecido com uma comanda em andamento, pelo cosseno com o centro de cada perfil.
        Sem nenhum item conhecido, não há como dizer: devolve None.
        """
        vetor = np.zeros(len(self.itens))
        for item in set(itens_da_comanda):
            if item in self._coluna:
                vetor[self._coluna[item]] = 1.0
        if not vetor.any():
            return None
        return int(np.argmax(self._centros @ (vetor / np.linalg.norm(vetor))))


def _matriz(transacoes: Sequence[Sequence[int]]) -> tuple[list[int], np.ndarray]:
    itens = sorted({item for comanda in transacoes for item in comanda})
    coluna = {item: indice for indice, item in enumerate(itens)}
    matriz = np.zeros((len(transacoes), len(itens)))
    for linha, comanda in enumerate(transacoes):
        for item in set(comanda):
            matriz[linha, coluna[item]] = 1.0
    return itens, matriz / np.maximum(np.linalg.norm(matriz, axis=1, keepdims=True), 1e-12)


def _descrever(
    rotulos: np.ndarray, binaria: np.ndarray, itens: list[int]
) -> tuple[list[Perfil], np.ndarray, np.ndarray]:
    total = len(rotulos)
    presenca_geral = binaria.mean(axis=0)
    perfis, presencas, destaques = [], [], []
    for grupo in range(int(rotulos.max()) + 1):
        membros = binaria[rotulos == grupo]
        presenca = membros.mean(axis=0)
        destaque = presenca / np.maximum(presenca_geral, 1e-12)
        presencas.append(presenca)
        destaques.append(destaque)
        ordem = np.argsort(-destaque)[:ITENS_QUE_DESCREVEM]
        perfis.append(
            Perfil(
                id=grupo,
                comandas=len(membros),
                participacao=round(len(membros) / total, 4),
                itens_marcantes=tuple(
                    ItemMarcante(itens[i], round(float(presenca[i]), 4), round(float(destaque[i]), 3)) for i in ordem
                ),
            )
        )
    return perfis, np.array(presencas), np.array(destaques)


def agrupar(
    transacoes: Sequence[Sequence[int]],
    minimo: int = QUANTIDADE_MINIMA_DE_PERFIS,
    maximo: int = QUANTIDADE_MAXIMA_DE_PERFIS,
    semente: int = SEMENTE,
) -> ModeloDePerfis | None:
    """
    Agrupa as comandas em perfis de consumo. Devolve None quando não há comandas suficientes para testar nem a
    menor quantidade de perfis. Mesma semente, mesmo resultado.
    """
    comandas = [comanda for comanda in transacoes if comanda]
    if len(comandas) <= maximo:
        return None

    itens, matriz = _matriz(comandas)
    binaria = (matriz > 0).astype(float)

    # Não adianta pedir mais grupos do que comandas diferentes: o K-Means repetiria centros.
    distintas = len({tuple(linha) for linha in binaria})
    melhor: tuple[float, int, KMeans] | None = None
    silhuetas: dict[int, float] = {}
    for k in range(minimo, min(maximo, distintas) + 1):
        kmeans = KMeans(n_clusters=k, n_init=10, random_state=semente).fit(matriz)
        if len(set(kmeans.labels_)) < 2:
            continue
        silhueta = float(silhouette_score(matriz, kmeans.labels_, metric="cosine"))
        silhuetas[k] = round(silhueta, 4)
        if melhor is None or silhueta > melhor[0]:
            melhor = (silhueta, k, kmeans)

    if melhor is None:
        return None

    silhueta, _, kmeans = melhor
    rotulos = kmeans.labels_
    perfis, presenca, destaque = _descrever(rotulos, binaria, itens)
    return ModeloDePerfis(
        itens, kmeans.cluster_centers_, perfis, rotulos, round(silhueta, 4), silhuetas, presenca, destaque
    )
