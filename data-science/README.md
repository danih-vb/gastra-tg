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

## Algoritmos

### Recomendação de pratos (RF09)

`src/gastra_analitica/recomendacao/regras_associacao.py` — regras de associação com o mlxtend.

- **Suporte:** em que fração das comandas um conjunto de itens aparece.
- **Confiança** da regra A → B: entre as comandas que têm A, em quantas B também aparece.
- **Lift:** quantas vezes A e B aparecem juntos além do esperado se fossem independentes. Regras com
  lift **menor ou igual a 1** são descartadas: significam itens que se evitam, e sugerir seria pior
  do que não sugerir nada.
- Entram apenas **padrões de consumo observáveis** (itens já pedidos). Nenhum atributo pessoal do
  cliente é usado, nem pode ser (RN05).

### Alocação de garçons (RF06, RN03)

`src/gastra_analitica/alocacao/programacao_linear.py` — problema de designação resolvido com PuLP + CBC.

Custo de colocar o garçom *i* na praça *j*, com os fatores normalizados em [0, 1]:

```
c(i,j) = w1 · (faturamento do garçom × potencial da praça)
       + w2 · ((1 − espera do garçom) × potencial da praça)
```

- **São produtos, e não somas, de propósito:** somar "faturamento do garçom + faturamento da praça"
  dá o mesmo total em qualquer distribuição, e o solver empataria — a regra não decidiria nada. Com o
  produto, juntar quem mais faturou com a praça que mais fatura fica caro, e a solução ótima entrega
  a praça boa a quem está para trás.
- **Restrições:** cada garçom em exatamente uma praça; cada praça dentro das suas vagas; nenhuma
  praça sem atendimento quando há garçons suficientes.
- A matriz de um problema de designação é totalmente unimodular, então a relaxação contínua já sai
  inteira: não é preciso solver de programação inteira.
- Pesos `w1` e `w2` (padrão 0,6 e 0,4) ainda serão calibrados com dado simulado.

### Dados simulados

`src/gastra_analitica/dados/simulador.py` gera comandas e turnos **determinísticos** (mesma semente,
mesmos dados). O restaurante colaborador não forneceu base histórica e o sistema ainda não acumulou
movimento próprio, então é com esses dados que os algoritmos são testados e os pesos serão calibrados.

## Endpoints

| Método | Rota | Para quê |
|---|---|---|
| `GET` | `/health` | Saúde do serviço |
| `POST` | `/recomendacao/combinacoes` | Sugere itens a partir dos já pedidos na comanda |
| `POST` | `/alocacao/sugestao` | Distribui os garçons do turno entre as praças |

Quem chama é sempre o backend em C#; o serviço **calcula e devolve**, nunca grava (decisão D2 da
arquitetura). Enquanto o banco não tem movimento real, a recomendação usa o histórico simulado, e a
resposta diz isso no campo `origem_do_historico`.

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
