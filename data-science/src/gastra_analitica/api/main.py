from fastapi import FastAPI

app = FastAPI(
    title="GASTRA — Camada Analítica",
    description="Recomendação de pratos e alocação de garçons, consumida pelo backend em C#.",
    version="0.1.0",
)


@app.get("/health", tags=["Saúde"])
def health() -> dict[str, str]:
    """Indica que o serviço está no ar."""
    return {"status": "Healthy"}
