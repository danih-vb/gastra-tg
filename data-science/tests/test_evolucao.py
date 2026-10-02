"""Testes da evolução do Gini turno a turno (issue #221)."""

from gastra_analitica.alocacao.calibracao import politica_programacao_linear, politica_rodizio
from gastra_analitica.alocacao.evolucao import (
    EstadoInicial,
    GarcomInicial,
    PracaInicial,
    curva_media,
    evoluir,
    gini_da_janela,
)

# Um salão pequeno e desigual: quem vende mais (fator alto) também faturou mais na janela.
INICIO = EstadoInicial(
    garcons=tuple(
        GarcomInicial(
            id=numero,
            fator_de_venda=fator,
            historico=tuple((-turno, 100 * fator * (2 if numero > 3 else 1)) for turno in range(1, 41)),
            turnos_desde_alto_potencial=0,
        )
        for numero, fator in enumerate([0.6, 0.8, 1.0, 1.2, 1.4], start=1)
    ),
    pracas=(PracaInicial(1, 2, 15, 1), PracaInicial(2, 2, 9, 1), PracaInicial(3, 1, 4, 1)),
    ticket_medio=150,
)


def test_a_curva_comeca_no_gini_do_banco_e_tem_um_ponto_por_turno():
    curva = evoluir(politica_rodizio, INICIO, turnos=10, semente=1)

    historicos = {g.id: list(g.historico) for g in INICIO.garcons}
    assert len(curva) == 11
    assert curva[0] == gini_da_janela(historicos, 0)


def test_mesma_semente_mesma_curva():
    assert evoluir(politica_rodizio, INICIO, 20, semente=3) == evoluir(politica_rodizio, INICIO, 20, semente=3)


def test_a_rn03_reduz_a_desigualdade_mais_que_o_rodizio():
    sementes = range(5)

    rn03 = curva_media(politica_programacao_linear(0.6), INICIO, sementes, turnos=60)
    rodizio = curva_media(politica_rodizio, INICIO, sementes, turnos=60)

    assert rn03[-1] < rn03[0] / 2  # cai pelo menos pela metade
    assert rn03[-1] < rodizio[-1]
