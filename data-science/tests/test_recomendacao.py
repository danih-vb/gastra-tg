"""Testes da recomendação de pratos (RF09)."""

from gastra_analitica.dados.simulador import gerar_historico
from gastra_analitica.recomendacao.regras_associacao import treinar


def test_recupera_a_combinacao_plantada_no_simulador():
    # O simulador faz a moqueca (1) puxar o arroz de coco (2) em 85% das vezes.
    modelo = treinar(gerar_historico().transacoes)

    sugestoes = modelo.sugerir([1])

    assert sugestoes, "o modelo deveria encontrar alguma regra para a moqueca"
    assert sugestoes[0].item_id == 2
    assert sugestoes[0].confianca > 0.7


def test_nao_sugere_item_que_ja_esta_na_comanda():
    modelo = treinar(gerar_historico().transacoes)

    sugeridos = [s.item_id for s in modelo.sugerir([1, 2])]

    assert 2 not in sugeridos


def test_respeita_a_lista_de_itens_disponiveis():
    modelo = treinar(gerar_historico().transacoes)

    sugeridos = [s.item_id for s in modelo.sugerir([1], itens_disponiveis=[3])]

    assert sugeridos == [3]


def test_respeita_o_limite_de_sugestoes():
    modelo = treinar(gerar_historico().transacoes)

    assert len(modelo.sugerir([1, 4], limite=1)) == 1


def test_historico_sem_combinacoes_nao_gera_regra():
    # Cada comanda com um item só: não há o que associar.
    modelo = treinar([[1], [2], [3]])

    assert len(modelo) == 0
    assert modelo.sugerir([1]) == []


def test_descarta_regras_com_lift_abaixo_de_um():
    # Itens que se evitam: quando um aparece, o outro quase nunca vem junto.
    comandas = [[1, 2]] * 5 + [[1, 3]] * 45 + [[2, 4]] * 50
    modelo = treinar(comandas, suporte_minimo=0.02, confianca_minima=0.05)

    assert all(regra.lift > 1 for regra in modelo.regras)


def test_resultado_e_o_mesmo_para_a_mesma_semente():
    primeiro = treinar(gerar_historico(semente=7).transacoes).sugerir([1])
    segundo = treinar(gerar_historico(semente=7).transacoes).sugerir([1])

    assert [s.item_id for s in primeiro] == [s.item_id for s in segundo]
