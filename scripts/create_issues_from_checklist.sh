#!/usr/bin/env bash
# Cria issues em lote no repositório remoto a partir de uma lista de tarefas.
# Requer: gh CLI instalado e autenticado (gh auth login).
#
# Edite o array TASKS abaixo — cada linha é "Título da issue|label".
# Os itens de exemplo vieram do checklist "A fazer" de docs/GASTRA_STATUS.md;
# ajustem a lista conforme o que realmente querem já no board.
set -e

TASKS=(
  "Coletar respostas dos dois questionários|tipo:documentacao"
  "Definir criterio de analise das respostas (graficos por bloco)|tipo:documentacao"
  "Roteiro de entrevista com garcom do Cocobambu|tipo:documentacao"
  "Realizar entrevista e sistematizar achados|tipo:documentacao"
  "Formalizar RF/RNF/RN do modulo de comandas em tabela rastreavel|bloco:comandas"
  "Definir funcao objetivo do algoritmo de programacao linear (RN03)|bloco:pl"
  "Decidir qualificador livre vs lista fechada para o garcom|bloco:comandas"
  "Validar modulo de comandas como expansao de escopo com o orientador|tipo:documentacao"
  "Validar cardapio digital opcao B com o orientador|tipo:documentacao"
  "Bloco 4 - MER do modulo de comandas (linguagem natural)|tipo:modelagem"
  "Bloco 4 - DER formal a partir do MER|tipo:modelagem"
  "Bloco 3 - Modelagem do banco de dados (comanda, cardapio, taxa, feedback)|tipo:modelagem"
  "Fluxo do garcom (abertura -> pedido -> fechamento)|tipo:modelagem"
  "Fluxo de recomendacao de pratos|bloco:clusterizacao"
  "Fluxo de alocacao de garcons|bloco:pl"
)

for entry in "${TASKS[@]}"; do
  title="${entry%%|*}"
  label="${entry##*|}"
  echo "Criando issue: $title [$label]"
  gh issue create --title "$title" --label "$label" \
    --body "Item vindo do checklist de docs/GASTRA_STATUS.md. Ver contexto completo la."
done

echo "Pronto. Issues criadas — confira se entraram no Project (coluna Backlog)."
