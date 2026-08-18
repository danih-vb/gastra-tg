# GASTRA — Status do Projeto

TG (Trabalho de Graduação) — FATEC Araraquara
Alunos: Daniel (Danih) e Pedro | Orientador: Prof. Me. Leonardo José de Lima Ferrucci
Última atualização: 18/08/2026 (revisão: setup completo do GitHub — repositório, branches, board, workflows)

> **Como manter este arquivo vivo:** ele fica versionado no repositório (`docs/GASTRA_STATUS.md` ou
> na raiz). Toda vez que um item mudar de status, editem aqui e façam commit com uma mensagem clara
> (ex.: `docs: marca MER do módulo de comandas como concluído`). O `git log` desse arquivo já vira o
> histórico de mudanças — não precisa manter uma seção de "log" separada e duplicada.

---

## 1. Escopo do projeto

> Escopo organizado em camadas de confiança, pra não perder o controle do tamanho do TG. Cada
> camada abaixo só "sobe" de status depois de validação formal com o Prof. Ferrucci.

**🟢 Escopo original — fechado, consta no projeto de pesquisa formal (Gastra.pdf):**
1. BI (Business Intelligence)
2. Ciência de dados — clusterização e regras de associação (recomendação de pratos)
3. Programação linear — alocação/distribuição de garçons
4. Conformidade LGPD

Stack definida: Python (análise de dados), Angular (frontend), ASP.NET Core (backend).

**🟡 Expansão de escopo decidida entre a dupla (13/08/2026) — PENDENTE de validação formal com o orientador:**

5. **Módulo de comandas** (front-of-house apenas — abrir/fechar mesa, registrar pedido, fechar
   conta). Justificativa formal a documentar na introdução do TG: os 4 blocos originais são
   analíticos e dependem de dados operacionais reais; o módulo de comandas é a camada que gera esses
   dados (pedidos, itens, mesas, fechamento de conta), servindo de fundação para os outros quatro.
   **Não substitui o escopo original — é complemento operacional.**

**🟠 Nova pendência (17/08/2026) — também aguardando validação, e explicitamente FORA do escopo principal:**

6. **Cardápio digital para o cliente — opção B (somente consulta, sem pedido).** Cliente acessa via
   QR code/tablet na mesa e vê fotos, preço e flags dietéticas (vegano/vegetariano/sem glúten/sem
   lactose); o pedido continua 100% verbal, feito pelo garçom — a função do garçom não é substituída.
   Tratado como **extensão opcional do módulo de comandas**, condicionada à aprovação do item 5, não
   como bloco novo obrigatório do TG. Justificativa empírica: 83% dos clientes relataram dificuldade
   de identificar flags dietéticas no cardápio (ver seção 6).

**⚠️ Conflito a resolver com o orientador:** o Gastra.pdf delimita explicitamente o GASTRA como NÃO
incluindo "sistemas de pedidos, cardápios digitais nem a substituição do atendimento humano" — os
itens 5 e 6 acima colidem diretamente com essa delimitação original. Até validação formal, tratar
ambos como **propostas de expansão**, não como escopo fechado, e evitar redigir seções do TG que
pressuponham a aprovação. Essa regra agora também está formalizada em `CONTRIBUTING.md` (seção 2),
como controle de processo no próprio repositório.

**⚪ Hierarquia/atores (modelagem interna, não depende de validação de escopo):**
Front-of-house apenas, por decisão de 17/08/2026 — cozinha fica fora de foco por ora.
Gerente → Coordenador → Metre → Garçom. Commis de salão fica anotado como papel futuro/opcional
(avisar quando pedido está pronto), fora do MVP.

---

## 2. Decisões de design já fechadas

| Decisão | Resolução | Data |
|---|---|---|
| Captação da composição da mesa (casal, família, etc.) | Híbrido: sistema sugere com base em regras simples (nº de lugares, horário, itens pedidos), garçom confirma/ajusta | 13/08/2026 |
| Uso de atributos sensíveis (gênero, classe social, condição de saúde) na segmentação | **Excluídos deliberadamente.** Substituídos por sinais observáveis e não sensíveis (tamanho do grupo, horário, itens pedidos) e por informação que o cliente comunica voluntariamente (não por inferência visual do garçom) | 13/08/2026 |
| Regras de inferência de estrutura da mesa | Solo (1 lugar) / Casal (2 lugares) / Grupo pequeno (3–4, sem item infantil) / Família (3+, com item infantil) / Grupo grande (5+) — qualificadores como "jovem"/"executivo" ficam a critério do garçom, não do algoritmo | 13/08/2026 |
| Ferramenta de levantamento de requisitos | Jotform (conectado e configurado) | 13/08/2026 |
| Modelo de branches | GitFlow simplificado (`main`, `dev`, `feature/*`) — sem `release/*` completo de livro, adaptado pra dupla | 18/08/2026 |
| Visibilidade do repositório | Público. Motivo: branch protection real (bloqueio de push direto) só é aplicada em repositório privado a partir do plano GitHub Pro; sem restrição de confidencialidade sobre o código em si, decidiu-se tornar público em vez de depender de disciplina sem enforcement técnico. Dados de questionário/pessoais continuam fora do Git em qualquer cenário (ver seção 4 do `CONTRIBUTING.md`) | 18/08/2026 |
| Mecanismo de proteção de branch | Rulesets (não branch protection clássica) — clássica ficou obsoleta na migração; `main` e `dev` protegidas, PR obrigatório + 1 aprovação | 18/08/2026 |
| Convenção de sub-issues | Issues do checklist geral (seção 3 deste arquivo) funcionam como issue guarda-chuva; quebradas em sub-issues nativas do GitHub conforme o trabalho fica concreto — documentado em `CONTRIBUTING.md`, seção 4 | 18/08/2026 |
| Licenciamento do repositório | MIT como padrão sugerido (não é exigência ABNT/FATEC) — nota de justificativa e ressalvas movida do `LICENSE` para `CONTRIBUTING.md` (seção 8), pra não interferir na detecção automática de licença do GitHub | 18/08/2026 |

**Pendente de decisão:** ver checklist "A fazer" (seção 3) para itens em aberto — qualificador
livre vs. lista fechada, notação de diagramas, função objetivo do PL, etc.

---

## 3. Checklist

### ✅ Concluído
- [x] Definição do escopo original (BI + clusterização/regras de associação + PL + LGPD)
- [x] Questionário de levantamento de requisitos — Garçom (24 perguntas, Jotform, revisado e ajustado)
- [x] Questionário de levantamento de requisitos — Cliente (18 perguntas, Jotform, finalizado)
- [x] Decisão de escopo: módulo de comandas como complemento operacional
- [x] Decisão de design: segmentação sem dado sensível (justificativa documentada, pronta pra banca)
- [x] Regras de inferência de estrutura de mesa (primeira versão)
- [x] Hierarquia organizacional e matriz de acesso (RBAC) — front-of-house: Gerente, Coordenador,
      Metre, Garçom (commis de salão anotado como papel futuro/opcional)
- [x] Definição de atores de caso de uso (Garçom, Metre, Gerente; Cliente condicional à validação
      do item 6 de escopo)
- [x] Rascunho inicial de RF/RNF/RN para o módulo de comandas (núcleo — RF05 ainda precisa ser
      revisado pra refletir a decisão de cardápio digital opção B)
- [x] Matriz de rastreabilidade de requisitos (RF/RNF/RN → origem → status de validação)
- [x] **Setup do repositório GitHub**: repositório criado, Pedro como colaborador, estrutura de
      pastas definida (`backend/`, `frontend/`, `data-science/`, `docs/`, `scripts/`, `.github/`)
- [x] **main e dev protegidas** via Ruleset (PR obrigatório + 1 aprovação; migrado de branch
      protection clássica, que não era aplicada em repositório privado no plano Free)
- [x] Repositório tornado público (decisão registrada na seção 2)
- [x] Labels criadas (`bloco:*`, `tipo:*`, `prioridade:*`)
- [x] **Setup do GitHub Projects**: board "GASTRA - TG" criado com 5 colunas (Backlog, A Fazer,
      Em Andamento, Em Revisão, Concluído)
- [x] Workflows automáticos configurados: Item added to project → Backlog · Pull request linked to
      issue → Em Revisão · Pull request merged → Concluído · Item closed → Concluído
- [x] Workflow "Auto-add to project" corrigido (removido filtro indevido `label:bug`) e ativado —
      issues novas entram no board sozinhas
- [x] Campo Iteration (sprints de 2 semanas) configurado via view em Table
- [x] Issues do checklist deste arquivo criadas (via `scripts/create_issues_from_checklist.sh`) e
      adicionadas manualmente ao board (criadas antes do "Auto-add to project" estar corrigido)
- [x] Link do Project adicionado ao `README.md`
- [x] `CONTRIBUTING.md` criado e expandido: modelo de branches, convenção de commits, regra de
      escopo pendente de validação, convenção de sub-issues, uso do quadro (Status manual vs.
      automático), segurança/LGPD, estrutura de pastas, licenciamento
- [x] `LICENSE` (MIT) definido, com nota de justificativa movida para `CONTRIBUTING.md`

### 🔄 Em andamento
- [ ] Divulgação dos questionários para garçons e clientes / coleta de respostas
      (parcial em 17/08/2026: 2 respostas de garçom, 12 de cliente — amostra ainda pequena, análise
      abaixo é preliminar, não conclusiva)
- [ ] PR de `feature/docs-licenca` → `dev` (LICENSE + CONTRIBUTING.md + este arquivo) — aguardando
      revisão do Pedro

### ⏳ A fazer

**Levantamento de requisitos**
- [ ] Coletar respostas dos dois questionários
- [ ] Definir critério de análise das respostas (gráficos por bloco, cruzamento garçom × cliente nos
      pontos em comum — ex.: dificuldade com flags dietéticas no cardápio)
- [ ] Roteiro de entrevista com garçom do Cocobambu
- [ ] Realizar entrevista e sistematizar achados
- [ ] Formalizar RF/RNF/RN do módulo de comandas em tabela rastreável (RF → ator → fonte/justificativa)
- [ ] Definir função objetivo exata do algoritmo de programação linear (RN03 — minimizar tempo de
      espera? equilibrar faturamento entre garçons? maximizar gorjeta total?)
- [ ] Decidir qualificador livre vs. lista fechada pro garçom (impacta Bloco de análise de dados)

**Validação com o orientador (fila)**
- [ ] Validar módulo de comandas como expansão de escopo (item 5, seção 1)
- [ ] Validar cardápio digital opção B — consulta pelo cliente, sem pedido (item 6, seção 1)
- [ ] Confirmar notação esperada pros diagramas de apoio (UML / BPMN / fluxograma simples)

**Documentação (TG)**
- [ ] Parágrafo de justificativa da expansão de escopo (introdução) — módulo de comandas
- [ ] Parágrafo de justificativa da extensão opcional — cardápio digital (condicionado à validação)
- [ ] Revisão ABNT do que já foi escrito até aqui
- [ ] Acrescentar ao README, seção de Privacidade/LGPD, menção explícita de que o repositório é
      público mas nenhum dado pessoal de cliente/garçom é versionado (reforço agora que o repo
      deixou de ser privado)

**Modelagem**
- [ ] Bloco 4 — MER do módulo de comandas: entidades e relacionamentos em linguagem natural
- [ ] Bloco 4 — DER formal (a partir do MER, não pular direto pro diagrama)
- [ ] Bloco 3 — Modelagem do banco de dados: comanda, cardápio (com flags dietéticas), taxa de
      serviço, feedback — com pontos de atenção LGPD sinalizados
- [ ] Confirmar consistência entre MER/DER e modelagem do banco

**Diagramas de apoio**
- [ ] Confirmar notação esperada pela banca/orientador
- [ ] Fluxo do garçom (abertura de mesa → pedido → fechamento)
- [ ] Fluxo de recomendação de pratos (clusterização/regras de associação)
- [ ] Fluxo de alocação de garçons (programação linear)

**Back-end / Front-end**
- [ ] Confirmar stack (linguagem, framework)
- [ ] Cardápio digital (imagem, preço, descrição, tags: vegano/vegetariano/sem glúten/sem lactose)
- [ ] Interface de comanda (tablet do garçom)
- [ ] Implementação dos algoritmos de clusterização/recomendação
- [ ] Implementação da alocação por programação linear
- [ ] Regras de gamificação (score do garçom) — só detalhar depois de ver o resultado do questionário
      sobre ranking (pergunta veio propositalmente neutra, pode não ser bem aceita)

**Análise de dados**
- [ ] Definir dados simulados/amostra a usar nas primeiras análises
- [ ] Documentar limitações e suposições dos dados usados

**Infraestrutura**
- [ ] Milestones com datas reais preenchidas
- [ ] Planejamento do primeiro sprint (atribuir Iteration + mover itens de Backlog para A Fazer)

---

## 4. Setup do GitHub (roteiro) — ✅ concluído em 18/08/2026

### Repositório
- Nome: `gastra-tg`, **público** (ver decisão na seção 2)
- Branch principal: `main` (protegida via Ruleset — PR obrigatório + 1 aprovação, sem push direto)
- Branch de desenvolvimento: `dev` (protegida via Ruleset, mesmas regras)
- Branches de feature: `feature/comandas`, `feature/cardapio-digital`, `feature/clusterizacao`,
  `feature/programacao-linear`, `feature/lgpd`, `feature/mer-der`, `feature/docs-licenca`, uma por
  bloco/módulo ou tarefa de documentação
- Pasta `docs/` guarda este arquivo, o MER/DER, e demais documentos de apoio do TG

> **Nota técnica:** a proteção de branch inicialmente configurada como "branch protection rule"
> clássica não era aplicada de fato (repositório privado no plano GitHub Free não aplica a regra,
> só a exibe como configurada — aviso "Not enforced" na tela de Settings). Resolvido migrando para
> **Rulesets** após tornar o repositório público (branch protection em repositório público é
> gratuita em qualquer plano). Ver decisão completa na seção 2.

### Labels
`bloco:bi` · `bloco:clusterizacao` · `bloco:pl` · `bloco:lgpd` · `bloco:comandas` ·
`tipo:documentacao` · `tipo:modelagem` · `tipo:codigo` · `prioridade:alta` · `prioridade:media` ·
`prioridade:baixa` — todas criadas.

### GitHub Projects — board "GASTRA - TG"
Link: https://github.com/users/danih-vb/projects/3

Colunas (Status): **Backlog** → **A Fazer** → **Em Andamento** → **Em Revisão** → **Concluído**.

Transições manuais vs. automáticas (detalhado em `CONTRIBUTING.md`, seção 5):
- Backlog → A Fazer → Em Andamento: manual
- Em Andamento → Em Revisão: automático (workflow "Pull request linked to issue")
- Em Revisão → Concluído: automático (workflows "Pull request merged" e "Item closed")
- Item novo → Backlog: automático (workflow "Item added to project", alimentado por "Auto-add to
  project" sem filtro de label)

Campo **Iteration** configurado (sprints de 2 semanas) — atribuição de sprint a cada item é manual,
sem workflow automático para isso.

### Issues do checklist
Criadas via `scripts/create_issues_from_checklist.sh` a partir da seção 3 deste arquivo, com a
label de bloco correspondente, e adicionadas ao board. Issues guarda-chuva (genéricas) devem ser
quebradas em sub-issues nativas do GitHub conforme o trabalho fica concreto — ver `CONTRIBUTING.md`,
seção 4.

---

## 6. Resultados preliminares (análise das respostas — 17/08/2026)

> ⚠️ **Amostra muito pequena** (2 garçons, 12 clientes). Tratar tudo abaixo como sinal preliminar /
> hipótese reforçada, não como resultado estatisticamente válido. Reanalisar quando a amostra crescer
> e atualizar esta seção (ou mover pra um relatório separado quando o volume justificar).

### Garçom (n=2)
- As 2 respostas já usam sistema/comanda eletrônica — nenhuma no papel puro (amostra pequena demais
  pra generalizar, mas é o primeiro dado a favor da dor que justifica o módulo de comandas).
- 100% percebem diferença de faturamento entre praças; nota média 4,0/5 pra "rotação baseada em dados
  seria mais justa". `[TG]` Achado mais forte até agora pro Bloco de Programação Linear.
- Clareza das informações do prato: 4,0/5. Uso de sugestão automática do sistema: 3,0/5 (neutro, não
  entusiasmado — acompanhar se essa média se mantém com mais respostas).
- 100% disseram que um ranking de desempenho os motivaria. Contraria a hipótese inicial de que
  gamificação poderia gerar desconforto — **não descartar a hipótese ainda**, n=2 é cedo demais pra
  conclusão.

### Cliente (n=12)
- Maior incômodo: demora pro pedido chegar (50%) e dificuldade de decidir o que pedir (33%).
- 83% já tiveram dificuldade de identificar flags dietéticas no cardápio. `[TG]` Cruza com a nota 4,0
  do garçom sobre clareza do prato — mesma dor confirmada dos dois lados (garçom e cliente).
- LGPD: 83% aceitam histórico de pedidos pra recomendação, mas metade desses exige poder apagar
  quando quiser. `[TG]` Valida diretamente a decisão de design de minimização/direito ao
  esquecimento (Art. 6º) já registrada na seção 2 deste arquivo.
- NPS médio 9,5/10 pra "experiência ágil e personalizada aumentaria recomendação". "Atendimento mais
  rápido" foi o motivo mais citado pra voltar (8 menções), à frente de recomendação personalizada
  (6 menções).
- Respostas abertas: "demora" aparece repetidamente como ponto negativo e como algo a melhorar —
  candidato a citação (parafraseada) na introdução do TG.

**Próximo passo desta seção:** reanalisar quando a amostra crescer; se os achados se confirmarem,
viram parágrafos formais na seção de resultados do TG (com o `[TG]` de cada item indicando pra que
bloco cada achado serve).

---

## 7. Log de decisões (histórico narrativo, além do `git log`)

*Esta seção é só para decisões importantes que merecem uma frase de contexto — o dia a dia de "o que
mudou" já fica no `git log` deste arquivo.*

- **13/08/2026** — Escopo expandido para incluir módulo de comandas. Decidido em conversa de
  orientação, motivado pela necessidade de uma fonte real de dados operacionais para os 4 blocos
  analíticos originais.
- **13/08/2026** — Descartada a captação de atributos sensíveis (gênero, classe social, condição de
  saúde) para segmentação de clientes, por risco de viés/discriminação e por LGPD tratar dado de
  saúde como sensível (Art. 5º, II). Substituída por sinais observáveis e comunicação voluntária do
  cliente.
- **13/08/2026** — Questionários de levantamento de requisitos criados e refinados no Jotform.
- **17/08/2026** — Primeira análise preliminar das respostas coletadas (2 garçons, 12 clientes).
  Achados registrados na seção 6, com destaque para amostra pequena e caráter não conclusivo.
- **17/08/2026** — Definida hierarquia organizacional e matriz de acesso (RBAC), restrita a
  front-of-house por decisão da dupla (cozinha fica fora de foco por ora). Commis anotado como papel
  de salão, futuro/opcional (avisar pedido pronto), fora do MVP.
- **17/08/2026** — Definidos atores de caso de uso do núcleo (Garçom, Metre, Gerente). Ator "Cliente"
  fica condicional à validação do item 6 de escopo (cardápio digital).
- **17/08/2026** — Decidido, entre a dupla, que o cardápio digital para o cliente seguirá a opção B
  (somente consulta via QR code/tablet, sem função de pedido) *se* aprovado pelo orientador — e que,
  mesmo aprovado, entra como extensão opcional do módulo de comandas, não como escopo principal do TG.
  Motivo: preservar o papel do garçom como quem tira o pedido, resolvendo ao mesmo tempo a dor
  empírica de 83% dos clientes com flags dietéticas pouco claras.
- **18/08/2026** — Setup completo do GitHub: repositório criado, labels, board "GASTRA - TG" com 5
  colunas e 4 workflows automáticos, campo Iteration, issues do checklist criadas e no board.
- **18/08/2026** — Repositório tornado público. Motivo: branch protection real (bloqueio de push
  direto na `main`/`dev`) não é aplicada em repositório privado no plano GitHub Free — só no Pro ou
  superior. Sem restrição de confidencialidade sobre o código, optou-se por público em vez de
  depender de disciplina sem enforcement técnico, ou de aguardar aprovação do GitHub Student Pack.
- **18/08/2026** — Migração de branch protection clássica para Rulesets em `main` e `dev` (regra
  antiga era exibida como configurada mas não era de fato aplicada; Rulesets, com o repositório
  público, aplica a regra de verdade — PR obrigatório + 1 aprovação).
- **18/08/2026** — `CONTRIBUTING.md` expandido: regra explícita proibindo tratar itens "Pendente de
  validação" como escopo fechado em código/texto commitado; convenção de sub-issues nativas do
  GitHub para quebrar issues guarda-chuva do checklist; tabela de transições manuais vs. automáticas
  do quadro de tarefas; seção de licenciamento (nota que antes estava dentro do `LICENSE`).