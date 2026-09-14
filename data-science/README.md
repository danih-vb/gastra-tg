# Camada Analítica — GASTRA

Python da camada analítica: BI, clusterização e regras de associação (RF09),
programação linear para alocação de garçons (RF06, RN03).

## Pré-requisitos

- Python 3.13

## Montando o ambiente

Todos os comandos abaixo são executados dentro de `data-science/`.

1. Crie o ambiente virtual (uma vez por máquina — a pasta `.venv/` não é versionada):
   ```bash
   python -m venv .venv
   ```
2. Ative o ambiente:
   - PowerShell (Windows): `.venv\Scripts\Activate.ps1`
   - Git Bash / Linux / macOS: `source .venv/Scripts/activate` (Windows) ou `source .venv/bin/activate`

   O terminal passa a mostrar `(.venv)`. Para sair: `deactivate`.
3. Instale as dependências nas versões fixadas:
   ```bash
   pip install -r requirements.txt
   ```

## Solver de Programação Linear

O PuLP 3.3 marca o solver embutido (`PULP_CBC_CMD`) como obsoleto — ele será removido no
PuLP 4.0. Por isso o `requirements.txt` instala `pulp[cbc]`, que traz o solver CBC no pacote
`cbcbox`. O executável não fica no PATH, então o caminho precisa ser passado explicitamente:

```python
import cbcbox
import pulp

solver = pulp.COIN_CMD(path=cbcbox.cbc_bin_path(), msg=0)
problema.solve(solver)
```

Não use `pulp.PULP_CBC_CMD()` nem `prob.solve()` sem solver: funcionam hoje, mas quebram na
atualização para o PuLP 4.0.

## Responsabilidade

A camada analítica **calcula e devolve** resultados: recomendação de pratos (clusterização e
regras de associação) e alocação de garçons (programação linear). **A gravação no banco é
responsabilidade do backend em C#**, que consome este serviço por HTTP.

## Estrutura

```
data-science/
├── pyproject.toml              # Configuração do pacote e do pytest
├── requirements.txt
├── src/
│   └── gastra_analitica/
│       ├── api/                # FastAPI: recebe as requisições e chama os algoritmos
│       ├── recomendacao/       # Clusterização e regras de associação (RF09)
│       └── alocacao/           # Programação linear — alocação de garçons (RF06, RN03)
├── tests/                      # pytest
├── notebooks/                  # Exploração e prototipagem (Jupyter)
└── data/
    ├── raw/                    # NUNCA versionado (dados pessoais/LGPD) — está no .gitignore
    └── processed/              # Dados tratados/anonimizados, versionáveis
```

**Regra:** os módulos de algoritmos (`recomendacao/`, `alocacao/`) **não dependem da API**.
São funções Python puras, usadas tanto pelo FastAPI quanto pelos notebooks, sem duplicar
código. A regra é verificada por `tests/test_arquitetura.py`.

## Comandos

Todos executados dentro de `data-science/`, com o ambiente ativado.

| Comando | O que faz |
|---|---|
| `uvicorn gastra_analitica.api.main:app --app-dir src --reload` | Sobe o serviço em `http://localhost:8000` (o `--reload` reinicia ao salvar) |
| `pytest` | Roda os testes |
| `jupyter lab` | Abre os notebooks |

Com o serviço rodando:
- `http://localhost:8000/health` responde `{"status": "Healthy"}`
- `http://localhost:8000/docs` abre a documentação interativa da API (gerada pelo FastAPI)

## LGPD

Nenhum dado bruto entra no Git. Dados de questionário, entrevista ou pedidos reais ficam em
`data/raw/` (ignorado) ou fora do repositório. Ver `CONTRIBUTING.md`, seção 4.
