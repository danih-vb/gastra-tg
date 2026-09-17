"""De onde vem o modelo de recomendação (D10): banco real ou histórico simulado."""

from gastra_analitica.recomendacao.provedor_modelo import ProvedorDeModelo

# Histórico "real" com um padrão que o simulado não tem: o item 10 sempre puxa o 11.
HISTORICO_REAL = [[10, 11]] * 40 + [[10, 11, 12]] * 20 + [[12, 13]] * 20


class Relogio:
    def __init__(self):
        self.agora = 0.0

    def __call__(self):
        return self.agora


def test_sem_banco_configurado_usa_o_simulado_e_diz_o_motivo():
    provedor = ProvedorDeModelo(lambda: None)

    atual = provedor.obter()

    assert atual.origem == "simulado"
    assert atual.motivo == "banco não configurado"
    assert atual.modelo.sugerir([1])[0].item_id == 2  # padrão plantado no simulador


def test_banco_fora_do_ar_nao_derruba_a_recomendacao():
    def ler():
        raise ConnectionError("recusado")

    atual = ProvedorDeModelo(ler).obter()

    assert atual.origem == "simulado"
    assert atual.motivo == "banco indisponível"


def test_historico_real_pequeno_demais_usa_o_simulado():
    atual = ProvedorDeModelo(lambda: [[10, 11]] * 5 + [[99]] * 100, minimo_de_comandas=50).obter()

    assert atual.origem == "simulado"
    assert "5 de 50" in atual.motivo


def test_historico_real_suficiente_treina_com_o_banco():
    atual = ProvedorDeModelo(lambda: HISTORICO_REAL, minimo_de_comandas=50).obter()

    assert atual.origem == "banco"
    assert atual.motivo is None
    assert atual.comandas_usadas == 80
    assert atual.modelo.sugerir([10])[0].item_id == 11


def test_o_modelo_fica_em_cache_e_e_recalculado_depois_do_prazo():
    leituras = []
    relogio = Relogio()

    def ler():
        leituras.append(relogio.agora)
        return HISTORICO_REAL

    provedor = ProvedorDeModelo(ler, minimo_de_comandas=50, cache_segundos=300, relogio=relogio)

    provedor.obter()
    relogio.agora = 299
    provedor.obter()
    assert len(leituras) == 1

    relogio.agora = 300
    provedor.obter()
    assert len(leituras) == 2


def test_invalidar_forca_nova_leitura():
    leituras = []
    provedor = ProvedorDeModelo(lambda: leituras.append(1) or HISTORICO_REAL, minimo_de_comandas=50)

    provedor.obter()
    provedor.invalidar()
    provedor.obter()

    assert len(leituras) == 2
