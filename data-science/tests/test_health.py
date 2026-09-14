from fastapi.testclient import TestClient

from gastra_analitica.api.main import app

client = TestClient(app)


def test_health_responde_healthy():
    resposta = client.get("/health")

    assert resposta.status_code == 200
    assert resposta.json() == {"status": "Healthy"}
