#!/usr/bin/env bash
# Cria as labels padrão do GASTRA no repositório remoto atual.
# Requer: gh CLI instalado e autenticado (gh auth login), rodado dentro do repo já
# conectado ao remoto (git remote add origin ... já feito).
set -e

declare -A LABELS=(
  ["bloco:bi"]="1f77b4"
  ["bloco:clusterizacao"]="2ca02c"
  ["bloco:pl"]="9467bd"
  ["bloco:lgpd"]="d62728"
  ["bloco:comandas"]="ff7f0e"
  ["bloco:cardapio-digital"]="e377c2"
  ["tipo:documentacao"]="8c8c8c"
  ["tipo:modelagem"]="17becf"
  ["tipo:codigo"]="393939"
  ["prioridade:alta"]="b60205"
  ["prioridade:media"]="fbca04"
  ["prioridade:baixa"]="0e8a16"
)

for name in "${!LABELS[@]}"; do
  color="${LABELS[$name]}"
  echo "Criando label: $name"
  gh label create "$name" --color "$color" --force
done

echo "Pronto. Labels criadas/atualizadas."
