"""Calibração dos pesos da RN03 por simulação (issue #55)."""

import pytest

from gastra_analitica.alocacao.calibracao import (
    CenarioSimulado,
    EstadoDoTurno,
    ResultadoDaPolitica,
    escolher_peso,
    gini,
    politica_fixa,
    politica_programacao_linear,
    politica_rodizio,
    simular,
)
from gastra_analitica.alocacao.programacao_linear import PracaDoTurno

# Cenário curto para os testes: o completo (120 turnos, 20 sementes) roda no script.
CURTO = CenarioSimulado(turnos=60)


def media(metricas, atributo):
    return sum(getattr(m, atributo) for m in metricas) / len(metricas)


@pytest.mark.parametrize(("valores", "esperado"), [
    ([10, 10, 10], 0.0),
    ([0, 0, 9], 2 / 3),
    ([1, 3], 0.25),
    ([], 0.0),
])
def test_gini(valores, esperado):
    assert gini(valores) == pytest.approx(esperado)


def test_simulacao_e_deterministica():
    assert simular(politica_rodizio, CURTO, semente=3) == simular(politica_rodizio, CURTO, semente=3)


def test_politicas_de_referencia_ocupam_todas_as_pracas_sem_estourar_vagas():
    pracas = [PracaDoTurno(1, 3, 1800), PracaDoTurno(2, 3, 1200), PracaDoTurno(3, 2, 600)]
    estado = EstadoDoTurno(indice=4, presentes=[1, 2, 3, 4, 5, 6, 7], garcons=[], pracas=pracas)

    for politica in (politica_fixa, politica_rodizio):
        destino = politica(estado)
        assert sorted(destino) == estado.presentes
        for praca in pracas:
            ocupacao = sum(1 for p in destino.values() if p == praca.id)
            assert 1 <= ocupacao <= praca.vagas


def test_rn03_com_peso_calibrado_distribui_melhor_que_deixar_sem_regra():
    sementes = (0, 1)
    sem_regra = [simular(politica_fixa, CURTO, s) for s in sementes]
    rn03 = [simular(politica_programacao_linear(0.8), CURTO, s) for s in sementes]

    assert media(rn03, "gini") < media(sem_regra, "gini") / 2
    assert media(rn03, "espera_maxima") < media(sem_regra, "espera_maxima")


def test_peso_todo_na_espera_reduz_a_espera_maxima():
    sementes = (0, 1)
    so_espera = [simular(politica_programacao_linear(0.0), CURTO, s) for s in sementes]
    so_faturamento = [simular(politica_programacao_linear(1.0), CURTO, s) for s in sementes]

    assert media(so_espera, "espera_maxima") < media(so_faturamento, "espera_maxima")


def test_escolhe_o_peso_que_equilibra_as_duas_metricas():
    resultados = [
        ResultadoDaPolitica("w1 = 0.0", 0.0, gini_medio=0.050, espera_maxima_media=4),
        ResultadoDaPolitica("w1 = 0.5", 0.5, gini_medio=0.030, espera_maxima_media=6),
        ResultadoDaPolitica("w1 = 1.0", 1.0, gini_medio=0.010, espera_maxima_media=12),
        ResultadoDaPolitica("fixa (sem regra)", None, gini_medio=0.001, espera_maxima_media=1),
    ]

    # Normalizados: w1=0 → (1 + 0)/2; w1=0,5 → (0,5 + 0,25)/2; w1=1 → (0 + 1)/2. A referência não concorre.
    assert escolher_peso(resultados).peso_desequilibrio == 0.5
