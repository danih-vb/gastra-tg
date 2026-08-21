#!/usr/bin/env bash
set -euo pipefail

# ============================================================================
# GASTRA — Criação de issues e sub-issues das Sprints 1 e 2
# ============================================================================
# Este script NÃO toca nas 15 issues já existentes (criadas por
# scripts/create_issues_from_checklist.sh). Ele cria issues NOVAS de duas
# categorias:
#   1) Issues "utilitárias" novas que não existiam antes (ex.: confirmar
#      notação de diagramas, diagrama de casos de uso, MER, DER, etc.)
#   2) Sub-issues vinculadas às issues já existentes (#1, #2, #5, #10, #11,
#      #12), quebrando o trabalho genérico em tarefas concretas.
#
# Pré-requisitos:
#   - gh CLI >= 2.94.0 (suporte nativo a `gh issue create --parent`).
#     Confira com: gh --version
#     Se for mais antigo: gh extension upgrade --all  (ou reinstale o gh)
#   - Estar autenticado: gh auth status
#   - Rodar de dentro do repositório (ou usar -R owner/repo em cada chamada,
#     se preferir rodar de fora)
#
# Idempotência: este script NÃO verifica se já rodou antes. Rodar duas vezes
# cria issues duplicadas. Rode uma vez só; se precisar adicionar mais depois,
# edite o script e comente/remova o que já foi criado.
# ============================================================================

REPO_MILESTONE_M1="M1 - Levantamento de Requisitos"
REPO_MILESTONE_M2="M2 - Validação de Escopo"
REPO_MILESTONE_M3="M3 - Modelagem"

echo "== Verificando versão do gh CLI =="
gh --version

# ----------------------------------------------------------------------------
# Função utilitária: cria uma issue "pai" (sem parent) e retorna o número dela
# ----------------------------------------------------------------------------
create_parent_issue() {
  local title="$1"
  local body="$2"
  local labels="$3"
  local milestone="$4"

  local url
  url=$(gh issue create \
    --title "$title" \
    --body "$body" \
    --label "$labels" \
    --milestone "$milestone")

  echo "${url##*/}"
}

# ----------------------------------------------------------------------------
# Função utilitária: cria uma sub-issue vinculada a um parent existente
# ----------------------------------------------------------------------------
create_sub_issue() {
  local parent="$1"
  local title="$2"
  local body="$3"
  local labels="$4"

  gh issue create \
    --title "$title" \
    --body "$body" \
    --label "$labels" \
    --parent "$parent" \
    >/dev/null

  echo "  -> sub-issue criada em #$parent: $title"
}

# ============================================================================
# SPRINT 1 (foco: destravar M1 — levantamento de requisitos)
# ============================================================================
echo ""
echo "== Sprint 1 =="

# --- Nova issue utilitária: notação de diagramas ---------------------------
NOTACAO_ID=$(create_parent_issue \
  "Confirmar notação esperada para diagramas de apoio com o orientador" \
  "Definir se o orientador espera UML formal, BPMN ou fluxograma simples para os diagramas de apoio (caso de uso, atividade, MER/DER, classes). Bloqueia o início do Bloco de Modelagem (Sprint 2). Ver docs/GASTRA_STATUS.md, seção 3." \
  "tipo:documentacao,prioridade:alta" \
  "$REPO_MILESTONE_M1")
echo "Criada #$NOTACAO_ID — Confirmar notação de diagramas"

# --- Sub-issues de #1 — Coletar respostas dos questionários ----------------
create_sub_issue 1 \
  "Divulgar formulário para mais garçons (meta: sair de n=2)" \
  "Ampliar amostra do questionário de garçom além das 2 respostas atuais." \
  "tipo:documentacao,prioridade:alta"

create_sub_issue 1 \
  "Divulgar formulário para mais clientes (meta: sair de n=12)" \
  "Ampliar amostra do questionário de cliente além das 12 respostas atuais." \
  "tipo:documentacao,prioridade:alta"

create_sub_issue 1 \
  "Definir data de corte para a próxima análise das respostas" \
  "Escolher até quando aceitar novas respostas antes de rodar a próxima rodada de análise (seção 6 do GASTRA_STATUS.md)." \
  "tipo:documentacao,prioridade:media"

# --- Sub-issues de #2 — Definir critério de análise -------------------------
create_sub_issue 2 \
  "Definir gráficos por bloco temático do questionário" \
  "Escolher quais gráficos representam melhor cada bloco de perguntas (garçom e cliente)." \
  "tipo:documentacao,prioridade:media"

create_sub_issue 2 \
  "Definir cruzamento garçom x cliente nos pontos em comum" \
  "Ex.: dificuldade com flags dietéticas relatada pelos dois lados (ver seção 6 do GASTRA_STATUS.md)." \
  "tipo:documentacao,prioridade:media"

create_sub_issue 2 \
  "Escolher ferramenta de análise (planilha vs. Python/pandas)" \
  "Decidir se a análise preliminar usa planilha simples ou já entra no stack de Ciência de Dados (Python/pandas), pensando em reaproveitar para a análise de dados real do TG mais adiante." \
  "tipo:documentacao,bloco:clusterizacao,prioridade:media"

# --- Sub-issues de #5 — Formalizar RF/RNF/RN do módulo de comandas ---------
create_sub_issue 5 \
  "Revisar RF05 para refletir cardápio digital opção B" \
  "Atualizar a descrição de RF05 no GASTRA_Requisitos_RN.docx conforme decisão de 17/08/2026 (consulta via QR/tablet, sem função de pedido)." \
  "bloco:comandas,tipo:documentacao,prioridade:alta"

create_sub_issue 5 \
  "Validar RN03 com o orientador antes de fechar" \
  "RN03 (função objetivo do algoritmo de PL) segue 'Em aberto' na Matriz de Rastreabilidade — precisa de definição da dupla e validação formal." \
  "bloco:pl,tipo:documentacao,prioridade:alta"

echo ""
echo "== Sprint 1 concluída =="

# ============================================================================
# SPRINT 2 (foco: diagramas + modelagem, rumo a M2/M3)
# ============================================================================
echo ""
echo "== Sprint 2 =="

# --- Nova issue: Diagrama de Casos de Uso -----------------------------------
UC_ID=$(create_parent_issue \
  "Diagrama de Casos de Uso (UML)" \
  "Formalizar em diagrama os casos de uso já antecipados na coluna 'Caso de Uso relacionado' da Matriz de Rastreabilidade. Depende da issue #$NOTACAO_ID (notação confirmada)." \
  "tipo:modelagem,prioridade:alta" \
  "$REPO_MILESTONE_M3")
echo "Criada #$UC_ID — Diagrama de Casos de Uso"

create_sub_issue "$UC_ID" \
  "Listar casos de uso por ator a partir da Matriz de Rastreabilidade" \
  "Consolidar a lista de casos de uso (Abrir Comanda, Registrar Pedido, Calcular Alocação de Garçons, etc.) por ator (Garçom, Metre, Gerente, Cliente condicional)." \
  "tipo:modelagem,prioridade:alta"

create_sub_issue "$UC_ID" \
  "Desenhar o diagrama de casos de uso (formato editável)" \
  "Usar Mermaid ou PlantUML como texto versionável, não só imagem, salvando em docs/diagramas/." \
  "tipo:modelagem,prioridade:alta"

create_sub_issue "$UC_ID" \
  "Revisão cruzada do diagrama de casos de uso (Pedro)" \
  "Conferir se o diagrama reflete fielmente a Matriz de Rastreabilidade e se ambos conseguem explicar cada caso de uso na banca." \
  "tipo:modelagem,prioridade:media"

# --- Sub-issues de #10 (MER — issue já existente, não recriar) -------------
create_sub_issue 10 \
  "Levantar entidades e atributos em linguagem natural" \
  "Ex.: Mesa, Comanda, Pedido, Item do Pedido, Garçom, Cliente, Praça, Cardápio." \
  "tipo:modelagem,prioridade:alta"

create_sub_issue 10 \
  "Definir relacionamentos e cardinalidades" \
  "Justificar cada cardinalidade (1:N, N:N) com base nas regras de negócio já documentadas (RN01-RN05)." \
  "tipo:modelagem,prioridade:alta"

create_sub_issue 10 \
  "Revisão cruzada do MER (Pedro)" \
  "Conferir consistência com RF/RNF/RN antes de avançar para o DER formal." \
  "tipo:modelagem,prioridade:media"

# --- Sub-issues de #11 (DER — issue já existente, não recriar) -------------
create_sub_issue 11 \
  "Traduzir MER para DER formal (notação confirmada)" \
  "Só iniciar depois da issue #$NOTACAO_ID resolvida." \
  "tipo:modelagem,prioridade:alta"

create_sub_issue 11 \
  "Aplicar e documentar as formas normais (1FN/2FN/3FN)" \
  "Para cada tabela, registrar a justificativa da normalização aplicada — isso vira parágrafo defensável na banca (por que 1:N e não N:N, por que a tabela foi dividida assim)." \
  "tipo:modelagem,prioridade:alta"

create_sub_issue 11 \
  "Validar consistência entre DER e modelagem do banco relacional" \
  "Conferir se o DER bate exatamente com o que será implementado nas tabelas (issue de modelagem do banco)." \
  "tipo:modelagem,prioridade:media"

# --- Sub-issues de #12 — Modelagem do banco de dados -------------------------
create_sub_issue 12 \
  "Tabelas de comanda, pedido e item do pedido" \
  "Estrutura relacional para RF01, RF03, RF04 (abrir comanda, registrar pedido, fechar comanda)." \
  "bloco:comandas,tipo:modelagem,prioridade:alta"

create_sub_issue 12 \
  "Cardápio com flags dietéticas" \
  "Tabela de cardápio incluindo vegano/vegetariano/sem glúten/sem lactose (RF05, pendente de validação)." \
  "bloco:comandas,bloco:cardapio-digital,tipo:modelagem,prioridade:media"

create_sub_issue 12 \
  "Taxa de serviço e feedback" \
  "Estrutura para cálculo de taxa no fechamento (RF04) e coleta de feedback do cliente." \
  "bloco:comandas,tipo:modelagem,prioridade:media"

create_sub_issue 12 \
  "Sinalizar pontos de atenção LGPD na modelagem" \
  "Marcar explicitamente quais tabelas/colunas guardam dado pessoal, conforme RNF03/RN04/RN05." \
  "bloco:lgpd,tipo:modelagem,prioridade:alta"

create_sub_issue 12 \
  "Confirmar aplicação das formas normais nas tabelas finais" \
  "Checagem final de 1FN/2FN/3FN já nas tabelas implementadas, cruzando com a issue do DER." \
  "tipo:modelagem,prioridade:alta"

# --- Nova issue: Diagrama de Classes (fica para o fim da Sprint 2 / início da 3) ---
CLASSES_ID=$(create_parent_issue \
  "Diagrama de Classes (a partir do DER)" \
  "Só iniciar depois do DER fechado. Prioridade média — útil para mostrar arquitetura de código na banca, mas não bloqueia a validação de escopo com o orientador." \
  "tipo:modelagem,prioridade:baixa" \
  "$REPO_MILESTONE_M3")
echo "Criada #$CLASSES_ID — Diagrama de Classes"

echo ""
echo "== Sprint 2 concluída =="
echo ""
echo "== FIM =="
