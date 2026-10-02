"""Testes da clusterização por perfil de consumo e da recomendação segmentada (RF09)."""

from collections import Counter

from sklearn.metrics import adjusted_rand_score

from gastra_analitica.dados.simulador import gerar_historico
from gastra_analitica.recomendacao.clusterizacao import agrupar
from gastra_analitica.recomendacao.regras_associacao import treinar
from gastra_analitica.recomendacao.segmentada import taxa_de_acerto, treinar_segmentado

# Ids do simulador usados nos exemplos.
MOQUECA, PUDIM, SALADA, PORCAO_INFANTIL, SUCO, RISOTO, AGUA = 1, 4, 5, 6, 7, 11, 13


def test_escolhe_pela_silhueta_a_quantidade_de_perfis_plantada():
    # O simulador planta três perfis; ninguém diz isso ao algoritmo.
    modelo = agrupar(gerar_historico(1000).transacoes)

    assert len(modelo.perfis) == 3
    assert modelo.silhuetas_testadas[3] == max(modelo.silhuetas_testadas.values())
    assert modelo.tem_estrutura


def test_redescobre_os_perfis_plantados():
    historico = gerar_historico(1000)

    modelo = agrupar(historico.transacoes)

    # Índice de Rand ajustado: 1 é concordância perfeita com o gabarito, 0 é o que o acaso daria.
    assert adjusted_rand_score(historico.perfis, modelo.rotulos) > 0.75
    # Cada perfil encontrado é, em mais de 85%, um único perfil plantado.
    for perfil in modelo.perfis:
        plantados = Counter(p for p, r in zip(historico.perfis, modelo.rotulos) if r == perfil.id)
        assert plantados.most_common(1)[0][1] / perfil.comandas > 0.85


def test_descreve_cada_perfil_pelos_itens_que_se_destacam_nele():
    modelo = agrupar(gerar_historico(1000).transacoes)

    familia = modelo.perfil_de([PORCAO_INFANTIL])
    marcantes = [item.item_id for item in modelo.perfis[familia].itens_marcantes]

    assert PORCAO_INFANTIL in marcantes
    assert all(item.destaque > 1 for item in modelo.perfis[familia].itens_marcantes)


def test_associa_uma_comanda_em_andamento_ao_perfil_mais_parecido():
    modelo = agrupar(gerar_historico(1000).transacoes)

    assert modelo.perfil_de([SALADA, AGUA]) == modelo.perfil_de([RISOTO])
    assert modelo.perfil_de([SALADA, AGUA]) != modelo.perfil_de([MOQUECA])
    assert modelo.perfil_de([]) is None
    assert modelo.perfil_de([999]) is None  # item que o histórico nunca viu


def test_mesma_semente_mesmos_perfis():
    primeiro = agrupar(gerar_historico(600, semente=7).transacoes)
    segundo = agrupar(gerar_historico(600, semente=7).transacoes)

    assert primeiro.rotulos == segundo.rotulos


def test_historico_pequeno_demais_nao_gera_perfil():
    assert agrupar([[1, 2], [3], [1]]) is None


def test_segmentada_sugere_o_que_mesas_do_mesmo_perfil_pedem():
    modelo = treinar_segmentado(gerar_historico(1000).transacoes)

    recomendacao = modelo.recomendar([SALADA, SUCO])

    # Salada com suco tem cara de almoço executivo: sai o risoto, e não a porção infantil que o suco puxaria no
    # restaurante inteiro.
    sugeridos = [s.item_id for s in recomendacao.sugestoes]
    assert recomendacao.perfil == modelo.perfis.perfil_de([RISOTO])
    assert RISOTO in sugeridos
    assert PORCAO_INFANTIL not in sugeridos


def test_segmentada_continua_respeitando_itens_disponiveis_e_o_que_ja_foi_pedido():
    modelo = treinar_segmentado(gerar_historico(1000).transacoes)

    sugeridos = [s.item_id for s in modelo.recomendar([SALADA, SUCO], itens_disponiveis=[AGUA, SALADA]).sugestoes]

    assert sugeridos == [AGUA]


def test_segmentada_acerta_mais_que_as_regras_gerais():
    # Item escondido: 70% das comandas treinam, e em cada comanda restante esconde-se um item por vez.
    comandas = gerar_historico(2000, semente=7).transacoes
    corte = int(len(comandas) * 0.7)

    geral = taxa_de_acerto(treinar, comandas[:corte], comandas[corte:])
    segmentada = taxa_de_acerto(treinar_segmentado, comandas[:corte], comandas[corte:])

    assert segmentada > geral


def test_sem_estrutura_a_recomendacao_fica_so_com_as_regras_gerais():
    # Poucas comandas, todas diferentes entre si: não há perfil para segmentar.
    comandas = [[1, 2], [3, 4], [5, 6], [7, 8], [1, 3], [2, 4], [5, 7], [6, 8]] * 3
    modelo = treinar_segmentado(comandas)

    assert modelo.por_perfil == {}
    assert modelo.recomendar([1]).perfil is None
