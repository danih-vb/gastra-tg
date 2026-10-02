"""Testes dos endpoints que o backend em C# consome."""

import pytest
from fastapi.testclient import TestClient

from gastra_analitica.api.main import app, provedor_de_modelo
from gastra_analitica.recomendacao.provedor_modelo import ModeloComOrigem
from gastra_analitica.recomendacao.segmentada import treinar_segmentado


@pytest.fixture
def cliente(monkeypatch):
    # Os testes da API nunca dependem de um banco configurado na máquina de quem roda.
    monkeypatch.delenv("GASTRA_ANALITICA_BANCO_SENHA", raising=False)
    provedor_de_modelo.invalidar()
    return TestClient(app)


def test_recomendacao_usa_o_historico_simulado_quando_nenhum_e_informado(cliente):
    resposta = cliente.post("/recomendacao/combinacoes", json={"itens": [1]})

    assert resposta.status_code == 200
    corpo = resposta.json()
    assert corpo["origem_do_historico"] == "simulado"
    assert corpo["sugestoes"], "deveria sugerir algo para a moqueca"
    assert corpo["sugestoes"][0]["lift"] > 1


def test_recomendacao_informa_o_motivo_de_usar_o_simulado(cliente):
    corpo = cliente.post("/recomendacao/combinacoes", json={"itens": [1]}).json()

    assert corpo["motivo_do_simulado"] == "banco não configurado"


def test_recomendacao_usa_o_historico_do_banco_quando_disponivel(cliente, monkeypatch):
    # Com todo pedido igual, o lift seria 1 e a regra descartada: o histórico precisa ter contraste.
    modelo_real = treinar_segmentado([[10, 11]] * 40 + [[12, 13]] * 40)
    monkeypatch.setattr(provedor_de_modelo, "obter", lambda: ModeloComOrigem(modelo_real, "banco", comandas_usadas=60))

    corpo = cliente.post("/recomendacao/combinacoes", json={"itens": [10]}).json()

    assert corpo["origem_do_historico"] == "banco"
    assert corpo["motivo_do_simulado"] is None
    assert corpo["sugestoes"][0]["item_id"] == 11


def test_recomendacao_aceita_historico_informado_pelo_backend(cliente):
    # O item 2 só aparece junto do 1: a regra 1 -> 2 tem lift bem acima de 1.
    historico = [[1, 2], [1, 2], [1, 2], [3, 4], [3, 4]]

    resposta = cliente.post(
        "/recomendacao/combinacoes",
        json={"itens": [1], "historico": historico, "limite": 1},
    )

    corpo = resposta.json()
    assert corpo["origem_do_historico"] == "informado"
    assert corpo["sugestoes"][0]["item_id"] == 2


def test_recomendacao_filtra_pelos_itens_disponiveis(cliente):
    resposta = cliente.post(
        "/recomendacao/combinacoes",
        json={"itens": [1], "itens_disponiveis": [3]},
    )

    assert [s["item_id"] for s in resposta.json()["sugestoes"]] == [3]


def test_alocacao_devolve_uma_praca_para_cada_garcom(cliente):
    resposta = cliente.post(
        "/alocacao/sugestao",
        json={
            "garcons": [
                {"id": 1, "faturamento_por_turno": 9000, "turnos_desde_praca_de_alto_potencial": 0},
                {"id": 2, "faturamento_por_turno": 3000, "turnos_desde_praca_de_alto_potencial": 4},
            ],
            "pracas": [
                {"id": 1, "vagas": 1, "faturamento_medio_historico": 1800},
                {"id": 2, "vagas": 1, "faturamento_medio_historico": 600},
            ],
        },
    )

    assert resposta.status_code == 200
    designacoes = resposta.json()["designacoes"]
    assert len(designacoes) == 2
    # Quem faturou menos e está esperando fica com a praça de maior potencial (RN03).
    assert next(d["praca_id"] for d in designacoes if d["garcom_id"] == 2) == 1


def test_alocacao_sem_vagas_suficientes_retorna_422(cliente):
    resposta = cliente.post(
        "/alocacao/sugestao",
        json={
            "garcons": [
                {"id": 1, "faturamento_por_turno": 1000, "turnos_desde_praca_de_alto_potencial": 0},
                {"id": 2, "faturamento_por_turno": 1000, "turnos_desde_praca_de_alto_potencial": 0},
            ],
            "pracas": [{"id": 1, "vagas": 1, "faturamento_medio_historico": 1000}],
        },
    )

    assert resposta.status_code == 422
    assert "comportam" in resposta.json()["detail"]


def test_alocacao_com_pesos_que_nao_somam_um_retorna_422(cliente):
    resposta = cliente.post(
        "/alocacao/sugestao",
        json={
            "garcons": [{"id": 1, "faturamento_por_turno": 1000, "turnos_desde_praca_de_alto_potencial": 0}],
            "pracas": [{"id": 1, "vagas": 1, "faturamento_medio_historico": 1000}],
            "peso_desequilibrio": 0.9,
            "peso_espera": 0.4,
        },
    )

    assert resposta.status_code == 422


def test_recomendacao_diz_qual_perfil_de_consumo_usou(cliente):
    # Salada (5) e água com gás (13) são do perfil "almoço executivo" do simulador.
    corpo = cliente.post("/recomendacao/combinacoes", json={"itens": [5, 13]}).json()

    assert corpo["perfil"] is not None
    assert 6 not in [s["item_id"] for s in corpo["sugestoes"]]  # porção infantil é de outro perfil


def test_perfis_de_consumo_descrevem_os_grupos_encontrados(cliente):
    corpo = cliente.get("/clusterizacao/perfis").json()

    assert corpo["origem_do_historico"] == "simulado"
    assert corpo["segmenta_a_recomendacao"] is True
    assert len(corpo["perfis"]) == 3
    assert abs(sum(p["participacao"] for p in corpo["perfis"]) - 1) < 0.01
    assert corpo["silhueta"] == max(corpo["silhuetas_testadas"].values())
    assert all(item["destaque"] > 1 for p in corpo["perfis"] for item in p["itens_marcantes"])
