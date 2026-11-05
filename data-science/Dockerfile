# Serviço analítico do GASTRA (issue #135). Construído a partir de data-science/:
#   docker build -t gastra-analitica data-science
# Só as dependências do serviço (requirements-servico.txt): notebooks, gráficos e testes ficam de fora da imagem.

FROM python:3.13-slim

ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1 PIP_NO_CACHE_DIR=1
WORKDIR /app

COPY requirements-servico.txt .
RUN pip install -r requirements-servico.txt

COPY src/ src/

# Usuário sem privilégio de root.
RUN useradd --create-home --uid 10001 analitica
USER analitica

EXPOSE 8000

HEALTHCHECK --interval=15s --timeout=5s --start-period=20s --retries=5 \
  CMD python -c "import urllib.request; urllib.request.urlopen('http://localhost:8000/health', timeout=3)"

CMD ["python", "-m", "uvicorn", "gastra_analitica.api.main:app", "--app-dir", "src", "--host", "0.0.0.0", "--port", "8000"]
