"""Testes da alocação de garçons por programação linear (RF06, RN03)."""

import pytest

from gastra_analitica.alocacao.programacao_linear import (
    AlocacaoInviavelError,
    GarcomDisponivel,
    PracaDoTurno,
    montar_matriz_de_custo,
    sugerir_alocacao,
)

PRACA_BOA = PracaDoTurno(id=1, vagas=1, faturamento_medio_historico=2000)
PRACA_FRACA = PracaDoTurno(id=2, vagas=1, faturamento_medio_historico=500)


def test_praca_de_alto_potencial_vai_para_quem_faturou_menos():
    quem_faturou_muito = GarcomDisponivel(id=1, faturamento_acumulado=10_000, turnos_desde_praca_de_alto_potencial=0)
    quem_faturou_pouco = GarcomDisponivel(id=2, faturamento_acumulado=2_000, turnos_desde_praca_de_alto_potencial=0)

    resultado = sugerir_alocacao([quem_faturou_muito, quem_faturou_pouco], [PRACA_BOA, PRACA_FRACA])

    destino = {d.garcom_id: d.praca_id for d in resultado.designacoes}
    assert destino == {2: 1, 1: 2}


def test_quem_espera_ha_mais_turnos_ganha_a_praca_boa_no_empate_de_faturamento():
    esperando = GarcomDisponivel(id=1, faturamento_acumulado=5_000, turnos_desde_praca_de_alto_potencial=8)
    recente = GarcomDisponivel(id=2, faturamento_acumulado=5_000, turnos_desde_praca_de_alto_potencial=0)

    resultado = sugerir_alocacao([esperando, recente], [PRACA_BOA, PRACA_FRACA])

    destino = {d.garcom_id: d.praca_id for d in resultado.designacoes}
    assert destino[1] == PRACA_BOA.id


def test_cada_garcom_recebe_exatamente_uma_praca_e_as_vagas_sao_respeitadas():
    garcons = [GarcomDisponivel(i, 1_000 * i, i) for i in range(1, 6)]
    pracas = [PracaDoTurno(1, 2, 1_800), PracaDoTurno(2, 2, 1_000), PracaDoTurno(3, 2, 600)]

    resultado = sugerir_alocacao(garcons, pracas)

    assert len(resultado.designacoes) == len(garcons)
    assert len({d.garcom_id for d in resultado.designacoes}) == len(garcons)
    for praca in pracas:
        alocados = [d for d in resultado.designacoes if d.praca_id == praca.id]
        assert 1 <= len(alocados) <= praca.vagas


def test_sem_vagas_suficientes_a_alocacao_e_inviavel():
    garcons = [GarcomDisponivel(i, 1_000, 0) for i in range(1, 4)]

    with pytest.raises(AlocacaoInviavelError):
        sugerir_alocacao(garcons, [PRACA_BOA, PRACA_FRACA])


def test_pesos_precisam_somar_um():
    garcons = [GarcomDisponivel(1, 1_000, 0), GarcomDisponivel(2, 2_000, 0)]

    with pytest.raises(ValueError):
        montar_matriz_de_custo(garcons, [PRACA_BOA, PRACA_FRACA], peso_desequilibrio=0.9, peso_espera=0.4)


def test_custos_ficam_entre_zero_e_um():
    garcons = [GarcomDisponivel(i, 3_000 * i, i * 2) for i in range(1, 5)]
    pracas = [PRACA_BOA, PRACA_FRACA, PracaDoTurno(3, 2, 1_200)]

    custos = montar_matriz_de_custo(garcons, pracas).values()

    assert all(0 <= custo <= 1 for custo in custos)


def test_so_o_peso_da_espera_inverte_a_escolha():
    # Mesmo par de garçons: com peso todo no faturamento, ganha quem faturou menos;
    # com peso todo na espera, ganha quem está esperando há mais tempo.
    faturou_menos_sem_espera = GarcomDisponivel(id=1, faturamento_acumulado=2_000, turnos_desde_praca_de_alto_potencial=0)
    faturou_mais_esperando = GarcomDisponivel(id=2, faturamento_acumulado=9_000, turnos_desde_praca_de_alto_potencial=9)
    garcons = [faturou_menos_sem_espera, faturou_mais_esperando]

    por_faturamento = sugerir_alocacao(garcons, [PRACA_BOA, PRACA_FRACA], 1.0, 0.0)
    por_espera = sugerir_alocacao(garcons, [PRACA_BOA, PRACA_FRACA], 0.0, 1.0)

    assert {d.garcom_id: d.praca_id for d in por_faturamento.designacoes}[1] == PRACA_BOA.id
    assert {d.garcom_id: d.praca_id for d in por_espera.designacoes}[2] == PRACA_BOA.id
