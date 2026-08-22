#!/usr/bin/env bash
# scripts/create_scope_update_issues.sh
#
# Cria as issues dos artefatos novos definidos na sessão de 21/08/2026
# (Business Model Canvas, User Stories, UX/UI, Arquitetura, Testes, Manual do Usuário)
# + uma sub-issue de #2, para a parte de governança/LGPD do tratamento do questionário.
#
# Checado manualmente contra o board atual (Sprint 1 + No Sprint, 21 issues) em 21/08/2026:
# nenhum destes 7 itens duplica issue existente. A única sobreposição parcial era com #2
# (issue-guarda-chuva de análise de dados, já com sub-issues #22/#23/#24) — por isso o último
# item deste script entra como sub-issue de #2 (--parent 2), não como issue nova solta.
#
# IMPORTANTE (mesma regra dos outros scripts em scripts/): rode uma vez só.
# Nenhum destes comandos verifica duplicidade — rodar duas vezes cria issues repetidas.
#
# Pré-requisito: gh CLI >= 2.94.0, autenticado, rodando dentro do clone do repo.
# As issues entram automaticamente no board (workflow "Auto-add to project" já configurado),
# no Status padrão (Backlog) e SEM Milestone — Milestone fica pendente até o cronograma chegar
# (decisão da dupla em 21/08/2026, ver GASTRA_STATUS.md seção 9).

set -euo pipefail

echo "Criando issue: User Stories (próxima tarefa da sprint atual)..."
gh issue create \
  --title "Redigir User Stories a partir dos RF" \
  --body "Derivar User Stories no formato \"Como [ator], quero [ação], para [benefício]\" a partir dos RF01–RF13 já formalizados em \`docs/requisitos/GASTRA_Requisitos_RN.docx\`.

Organizar por prioridade, deixando explícito no próprio artefato:
- Foco (blocos analíticos: RF06–RF12)
- Adicional, não foco (núcleo de comandas: RF01–RF04)
- Extra, condicionado a tempo (RF05, RF13)

Artefato **obrigatório** do TG II (ver Modelo_Estrutura_TGII_ADS_Atualizado.pdf). Salvar em \`docs/requisitos/\`.

Primeira tarefa da ordem combinada em 21/08/2026: User Stories → Canvas → Arquitetura → UX/UI → Testes → Manual." \
  --label "tipo:documentacao" --label "prioridade:alta"

echo "Criando issue: Business Model Canvas..."
gh issue create \
  --title "Criar Business Model Canvas do GASTRA" \
  --body "Preencher os 9 blocos do BMC aplicados ao GASTRA, conectando proposta de valor e segmentos de cliente aos achados da entrevista Cocobambu e do questionário (não descrição genérica de curso).

Salvar em \`docs/negocio/\`." \
  --label "tipo:documentacao" --label "prioridade:alta"

echo "Criando issue: Arquitetura..."
gh issue create \
  --title "Definir arquitetura do sistema (parte teórica do TG)" \
  --body "Documentar arquitetura (camadas, componentes, principais decisões e trade-offs) cobrindo ASP.NET Core (backend), Angular (frontend) e Python (data science), com justificativa técnica de cada decisão — não só descrição de stack.

Serve de base para a Prototipagem/UX-UI e para a Tabela de Cenários de Teste (issues seguintes). Relacionado a #10 (MER), #11 (DER) e #12 (Modelagem do banco de dados) — a arquitetura deve ser consistente com o que já foi/for modelado ali. Salvar em \`docs/arquitetura/\`." \
  --label "tipo:documentacao" --label "prioridade:media"

echo "Criando issue: Prototipagem/UX-UI..."
gh issue create \
  --title "Prototipagem / UX-UI — wireframes e protótipo navegável" \
  --body "Wireframes e protótipo navegável das telas principais (abertura de comanda, sugestão de alocação, ranking, dashboard de BI).

Depende da arquitetura estar definida (issue anterior). Relacionado a #27 (Diagrama de Casos de Uso) e #42 (Diagrama de Classes) — os fluxos de tela devem refletir os mesmos atores e casos de uso já mapeados ali. Salvar em \`docs/ux-ui/\`." \
  --label "tipo:documentacao" --label "prioridade:media"

echo "Criando issue: Cenários de teste..."
gh issue create \
  --title "Tabela de cenários de teste executados" \
  --body "Cenários de teste cobrindo pelo menos um caso por bloco (comandas, PL/alocação, recomendação, LGPD), com resultado executado documentado — não só planejado.

Salvar em \`docs/testes/\`." \
  --label "tipo:documentacao" --label "prioridade:baixa"

echo "Criando issue: Manual do usuário..."
gh issue create \
  --title "Manual de uso do usuário" \
  --body "Manual cobrindo os fluxos principais por ator (Garçom, Metre, Gerente). Só deve ser escrito depois que o protótipo/UX-UI estiver estável, pra não precisar reescrever.

Salvar em \`docs/manual-usuario/\`." \
  --label "tipo:documentacao" --label "prioridade:baixa"

echo "Criando sub-issue de #2: sistematização dos dados do questionário..."
echo "(evita duplicar #2/#22/#23/#24, que já são a issue-guarda-chuva de análise dos dados)"
gh issue create \
  --title "Definir tratamento LGPD dos dados brutos (raw vs. processed) antes da análise" \
  --body "Sub-issue de #2 — cobre a parte de **governança de dado** que #22/#23/#24 não cobrem (aqueles são sobre critério e ferramenta de análise; este é sobre onde cada dado pode ficar).

Exportar respostas do Jotform (waiter form 262246246733054, client form 262246590652056).

- Exportação bruta (CSV/JSON) → \`data-science/data/raw/\` (local, NUNCA no Git — já no .gitignore).
- Lista de perguntas (sem resposta) → doc versionável em \`docs/requisitos/\` (o export do Jotform já traz o texto da pergunta como cabeçalho de coluna).
- Dado do questionário Cliente (n=13), agregado/anonimizado → \`data-science/data/processed/\`, versionável.
- Dado do questionário Garçom (n=2) → **não estruturar como dataset** (amostra pequena demais, reidentifica); manter como texto narrativo em \`docs/GASTRA_STATUS.md\`, seção 6.

Ver \`docs/GASTRA_STATUS.md\`, seção 8, para o racional completo." \
  --label "tipo:documentacao" --label "prioridade:alta" \
  --parent 2

echo ""
echo "Todas as issues criadas (6 novas + 1 sub-issue de #2). Próximos passos manuais:"
echo "1. Abrir a issue 'Redigir User Stories...' no board e mover de Backlog -> A Fazer."
echo "2. Atribuir a Iteration (Sprint 1) e o Assignee dela manualmente."
echo "3. Deixar as demais em Backlog, sem Iteration nem Milestone, até o cronograma chegar."
echo "4. Conferir que a sub-issue de #2 apareceu aninhada em #2 no board (barra de progresso do pai deve virar 1 de 4)."
