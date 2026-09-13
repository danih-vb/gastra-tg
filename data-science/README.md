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

## Estrutura

```
data-science/
├── notebooks/        # Exploração e prototipagem (Jupyter)
├── src/              # Código de produção dos algoritmos
└── data/
    ├── raw/          # NUNCA versionado (dados pessoais/LGPD) — está no .gitignore
    └── processed/    # Dados tratados/anonimizados, versionáveis
```

Para abrir os notebooks: `jupyter lab` (com o ambiente ativado).

## LGPD

Nenhum dado bruto entra no Git. Dados de questionário, entrevista ou pedidos reais ficam em
`data/raw/` (ignorado) ou fora do repositório. Ver `CONTRIBUTING.md`, seção 4.
