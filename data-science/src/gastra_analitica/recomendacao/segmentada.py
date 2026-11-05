"""Recomendação segmentada por perfil de consumo: clusterização + regras de associação (RF09).

As regras de associação sozinhas respondem "o que costuma vir junto no restaurante inteiro". Com os perfis da
clusterização, a comanda em andamento é associada ao perfil mais parecido e recebe as sugestões de três fontes:

1. as **regras do perfil** — numa mesa com cara de família, o que costuma vir junto *nas famílias*;
2. os **itens característicos do perfil** que a mesa ainda não pediu — "mesas como esta costumam pedir";
3. as **regras gerais**, para completar.

As duas primeiras entram **juntas, ordenadas pela confiança**: as duas estimam a chance de a mesa pedir o item, e uma
regra fraca não pode passar na frente de um item que quase toda mesa do perfil pede. Os itens característicos costumam
pesar mais: dentro de um perfil, os itens aparecem juntos por serem do perfil, e não por um puxar o outro, e aí as
regras do perfil saem com lift perto de 1 e são descartadas.

Quando a clusterização não encontra estrutura, ou o perfil tem poucas comandas para gerar regras confiáveis, a
recomendação fica exatamente como era: só as regras gerais.
"""

from __future__ import annotations

from collections.abc import Callable, Iterable, Sequence
from dataclasses import dataclass

from gastra_analitica.recomendacao.clusterizacao import ModeloDePerfis, agrupar
from gastra_analitica.recomendacao.regras_associacao import ModeloRecomendacao, Sugestao, treinar

# Mesmo piso do provedor de modelo: menos que isso e as regras de um perfil saem por acaso.
MINIMO_DE_COMANDAS_POR_PERFIL = 50


@dataclass(frozen=True)
class Recomendacao:
    sugestoes: list[Sugestao]
    perfil: int | None  # perfil usado, ou None quando só as regras gerais valeram


class ModeloSegmentado:
    def __init__(
        self,
        geral: ModeloRecomendacao,
        perfis: ModeloDePerfis | None,
        por_perfil: dict[int, ModeloRecomendacao],
    ) -> None:
        self.geral = geral
        self.perfis = perfis
        self.por_perfil = por_perfil

    def __len__(self) -> int:
        return len(self.geral)

    def recomendar(
        self,
        itens_da_comanda: Iterable[int],
        limite: int = 3,
        itens_disponiveis: Iterable[int] | None = None,
    ) -> Recomendacao:
        itens = list(itens_da_comanda)
        disponiveis = list(itens_disponiveis) if itens_disponiveis is not None else None

        perfil = self.perfis.perfil_de(itens) if self.perfis is not None else None
        do_perfil = self.por_perfil.get(perfil) if perfil is not None else None
        if do_perfil is None:
            return Recomendacao(self.geral.sugerir(itens, limite, disponiveis), None)

        # Primeiro o que vem do perfil — as regras dele ("nas mesas como esta, quem pediu A pede B") e os itens que o
        # caracterizam ("mesas como esta costumam pedir") —, juntos e do mais para o menos provável. As duas fontes
        # medem a mesma coisa, a chance de a mesa pedir o item, e por isso disputam a mesma fila: com uma na frente
        # da outra, uma regra fraca passaria um item que quase toda mesa do perfil pede. As regras gerais só completam.
        pedidos = set(itens)
        permitidos = set(disponiveis) if disponiveis is not None else None

        do_proprio_perfil: dict[int, Sugestao] = {}
        candidatas = do_perfil.sugerir(itens, len(self.perfis.itens), disponiveis) + [
            Sugestao(marcante.item_id, marcante.presenca, marcante.destaque)
            for marcante in self.perfis.caracteristicos(perfil)
            if marcante.item_id not in pedidos and (permitidos is None or marcante.item_id in permitidos)
        ]
        for candidata in candidatas:
            melhor = do_proprio_perfil.get(candidata.item_id)
            if melhor is None or (candidata.confianca, candidata.lift) > (melhor.confianca, melhor.lift):
                do_proprio_perfil[candidata.item_id] = candidata

        sugestoes = sorted(do_proprio_perfil.values(), key=lambda s: (-s.confianca, -s.lift, s.item_id))[:limite]
        ja = {s.item_id for s in sugestoes}
        for sugestao in self.geral.sugerir(itens, limite + len(ja), disponiveis):
            if len(sugestoes) >= limite:
                break
            if sugestao.item_id not in ja:
                sugestoes.append(sugestao)
                ja.add(sugestao.item_id)
        return Recomendacao(sugestoes, perfil)

    def sugerir(
        self,
        itens_da_comanda: Iterable[int],
        limite: int = 3,
        itens_disponiveis: Iterable[int] | None = None,
    ) -> list[Sugestao]:
        """Mesmo contrato do modelo de regras gerais, para quem só quer a lista."""
        return self.recomendar(itens_da_comanda, limite, itens_disponiveis).sugestoes


def treinar_segmentado(comandas: Sequence[Sequence[int]]) -> ModeloSegmentado:
    geral = treinar(comandas)
    perfis = agrupar(comandas)
    if perfis is None or not perfis.tem_estrutura:
        return ModeloSegmentado(geral, perfis, {})

    uteis = [comanda for comanda in comandas if comanda]
    por_perfil: dict[int, ModeloRecomendacao] = {}
    for perfil in perfis.perfis:
        membros = [comanda for comanda, rotulo in zip(uteis, perfis.rotulos) if rotulo == perfil.id]
        if len(membros) >= MINIMO_DE_COMANDAS_POR_PERFIL:
            por_perfil[perfil.id] = treinar(membros)
    return ModeloSegmentado(geral, perfis, por_perfil)


def taxa_de_acerto(
    modelo_para: Callable[[Sequence[Sequence[int]]], ModeloRecomendacao | ModeloSegmentado],
    treino: Sequence[Sequence[int]],
    teste: Sequence[Sequence[int]],
    limite: int = 3,
) -> float:
    """
    Avaliação por item escondido: em cada comanda de teste com dois itens ou mais, esconde-se um item por vez e
    confere-se se ele volta entre as `limite` sugestões feitas a partir dos demais. É a pergunta prática do garçom:
    "a sugestão acertou o que a mesa ia pedir?".
    """
    modelo = modelo_para(treino)
    tentativas = acertos = 0
    for comanda in teste:
        itens = sorted(set(comanda))
        if len(itens) < 2:
            continue
        for escondido in itens:
            restantes = [item for item in itens if item != escondido]
            sugeridos = {s.item_id for s in modelo.sugerir(restantes, limite)}
            tentativas += 1
            acertos += escondido in sugeridos
    return acertos / tentativas if tentativas else 0.0
