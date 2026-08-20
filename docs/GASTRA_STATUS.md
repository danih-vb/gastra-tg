# GASTRA — Status do Projeto

TG (Trabalho de Graduação) — FATEC Araraquara
Alunos: Daniel (Danih) e Pedro | Orientador: Prof. Me. Leonardo José de Lima Ferrucci
Última atualização: 19/08/2026 (revisão: milestones com datas reais, Sprint 1 planejada, sub-issues criadas, entrevista Cocobambu agendada)

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
| Visibilidade do repositório | Público. Motivo: branch protection real (bloqueio de push direto) só é aplicada em repositório privado a partir do plano GitHub Pro; sem restrição de confidencialidade sobre o código em si, decidiu-se tornar público em vez de depender de disciplina sem enforcement técnico. Dados de questionário/pessoais continuam fora do Git em qualquer cenário (ver seção 6 do `CONTRIBUTING.md`) | 18/08/2026 |
| Mecanismo de proteção de branch | Rulesets (não branch protection clássica) — clássica ficou obsoleta na migração; `main` e `dev` protegidas, PR obrigatório + 1 aprovação | 18/08/2026 |
| Convenção de sub-issues | Issues do checklist geral (seção 3 deste arquivo) funcionam como issue guarda-chuva; quebradas em sub-issues nativas do GitHub conforme o trabalho fica concreto — documentado em `CONTRIBUTING.md`, seção 4 | 18/08/2026 |
| Licenciamento do repositório | MIT como padrão sugerido (não é exigência ABNT/FATEC) — nota de justificativa e ressalvas movida do `LICENSE` para `CONTRIBUTING.md` (seção 8), pra não interferir na detecção automática de licença do GitHub | 18/08/2026 |
| Branch padrão do repositório | Alterada de `main` para `dev` — reduz risco de abrir PR sem querer direcionado à `main`, já que a maioria dos PRs do dia a dia vai para `dev` | 19/08/2026 |
| Duração das Iterations (sprints) | Ajustada de 2 semanas (configuração inicial) para 1 semana — ritmo mais rápido faz sentido na fase de documentação atual; pode ser revisitada quando entrar a fase de implementação de código | 19/08/2026 |
| Auto-delete de branches após merge | Ativado (Settings > General > Pull Requests). Branches protegidas (`main`, `dev`) são poupadas pela regra "Restrict deletions" do Ruleset — ver nota de cautela na seção 4 | 19/08/2026 |
| Processo de release | Usa **tags Git** (não branches vivas) para marcar checkpoints — `release/*` é descartável após a tag criada; se apagada antes do merge para `dev`, usar PR `main → dev` em vez de tentar restaurar a branch | 19/08/2026 |

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
- [x] **Roteiro de entrevista com garçom do Cocobambu** — salvo em
      `docs/requisitos/entrevistas/GASTRA_Roteiro_Entrevista_Cocobambu.docx`. Entrevista agendada
      para 20/08/2026.
- [x] **Setup do repositório GitHub**: repositório criado, Pedro como colaborador, estrutura de
      pastas definida (`backend/`, `frontend/`, `data-science/`, `docs/`, `scripts/`, `.github/`)
- [x] **main e dev protegidas** via Ruleset (PR obrigatório + 1 aprovação; migrado de branch
      protection clássica, que não era aplicada em repositório privado no plano Free)
- [x] Repositório tornado público (decisão registrada na seção 2)
- [x] Labels criadas (`bloco:*`, `tipo:*`, `prioridade:*`), incluindo `tipo:infraestrutura` (nova)
- [x] **Setup do GitHub Projects**: board "GASTRA - TG" criado com 5 colunas (Backlog, A Fazer,
      Em Andamento, Em Revisão, Concluído)
- [x] Workflows automáticos configurados: Item added to project → Backlog · Pull request linked to
      issue → Em Revisão · Pull request merged → Concluído · Item closed → Concluído
- [x] Workflow "Auto-add to project" corrigido (removido filtro indevido `label:bug`) e ativado —
      issues novas entram no board sozinhas
- [x] Campo Iteration configurado via view em Table (duração ajustada para 1 semana — ver seção 2)
- [x] Issues do checklist deste arquivo criadas (via `scripts/create_issues_from_checklist.sh`) e
      adicionadas ao board (#1 a #15)
- [x] Link do Project adicionado ao `README.md`
- [x] `CONTRIBUTING.md` criado e expandido: modelo de branches, convenção de commits, regra de
      escopo pendente de validação, convenção de sub-issues, uso do quadro (Status manual vs.
      automático), segurança/LGPD, estrutura de pastas, licenciamento
- [x] `LICENSE` (MIT) definido, com nota de justificativa movida para `CONTRIBUTING.md`
- [x] `docs/CHECKLIST_REVISAO_PR.md` criado (roteiro para quem revisa um PR) e referenciado no
      `CONTRIBUTING.md`, seção 4
- [x] **PR `feature/docs-licenca` → `dev` mergeado** (LICENSE, CONTRIBUTING.md, GASTRA_STATUS.md,
      CHECKLIST_REVISAO_PR.md)
- [x] **Milestones criados** (M1 a M10) com datas reais preenchidas — datas originais reajustadas
      em 19/08/2026 por serem otimistas demais (ver seção 5)
- [x] `gh` CLI atualizado para versão ≥ 2.94.0 (suporte nativo a `--parent` para sub-issues)
- [x] `scripts/create_sprint_issues.sh` criado e executado: sub-issues adicionadas em #1, #2, #5,
      #10, #11, #12; novas issues-pai criadas ("Confirmar notação de diagramas", "Diagrama de Casos
      de Uso", "Diagrama de Classes")
- [x] **Planejamento da Sprint 1** — issues atribuídas à Iteration e movidas para "A Fazer" (ver
      seção 5)
- [x] Fluxo de release praticado de ponta a ponta (release → main + tag → dev) — ver seção 4

### 🔄 Em andamento
- [ ] Divulgação dos questionários para garçons e clientes / coleta de respostas
      (parcial em 17/08/2026: 2 respostas de garçom, 12 de cliente — amostra ainda pequena, análise
      abaixo é preliminar, não conclusiva)
- [ ] Entrevista com garçom do Cocobambu — **agendada para 20/08/2026**

### ⏳ A fazer

**Levantamento de requisitos**
- [ ] Coletar respostas dos dois questionários
- [ ] Definir critério de análise das respostas (gráficos por bloco, cruzamento garçom × cliente nos
      pontos em comum — ex.: dificuldade com flags dietéticas no cardápio)
- [ ] Realizar entrevista e sistematizar achados (bloqueia formalização de RF/RNF/RN e definição de
      RN03 — ver sub-issues de #5 e #6)
- [ ] Formalizar RF/RNF/RN do módulo de comandas em tabela rastreável (RF → ator → fonte/justificativa)
- [ ] Definir função objetivo exata do algoritmo de programação linear (RN03 — minimizar tempo de
      espera? equilibrar faturamento entre garçons? maximizar gorjeta total?)
- [ ] Decidir qualificador livre vs. lista fechada pro garçom (impacta Bloco de análise de dados)

**Validação com o orientador (fila)**
- [ ] Validar módulo de comandas como expansão de escopo (item 5, seção 1)
- [ ] Validar cardápio digital opção B — consulta pelo cliente, sem pedido (item 6, seção 1)
- [ ] Confirmar notação esperada pros diagramas de apoio (UML / BPMN / fluxograma simples) — issue
      aberta no board, prioridade alta por destravar toda a Sprint 2

**Documentação (TG)**
- [ ] Parágrafo de justificativa da expansão de escopo (introdução) — módulo de comandas
- [ ] Parágrafo de justificativa da extensão opcional — cardápio digital (condicionado à validação)
- [ ] Revisão ABNT do que já foi escrito até aqui

**Modelagem**
- [ ] Diagrama de Casos de Uso (UML) — depende da notação confirmada com o orientador
- [ ] Bloco 4 — MER do módulo de comandas: entidades e relacionamentos em linguagem natural
- [ ] Bloco 4 — DER formal (a partir do MER, não pular direto pro diagrama), incluindo justificativa
      de 1FN/2FN/3FN por tabela
- [ ] Bloco 3 — Modelagem do banco de dados: comanda, cardápio (com flags dietéticas), taxa de
      serviço, feedback — com pontos de atenção LGPD sinalizados
- [ ] Confirmar consistência entre MER/DER e modelagem do banco
- [ ] Diagrama de Classes (a partir do DER) — prioridade baixa, útil mas não bloqueante

**Diagramas de apoio**
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
- [ ] Confirmar exclusão das labels padrão irrelevantes (`duplicate`, `invalid`, `wontfix`,
      `help wanted`, `good first issue`, `documentation`) — triagem sugerida em 19/08/2026, execução
      a confirmar

---

## 4. Setup do GitHub (roteiro) — ✅ concluído em 18/08/2026

### Repositório
- Nome: `gastra-tg`, **público** (ver decisão na seção 2)
- Branch principal: `main` (protegida via Ruleset — PR obrigatório + 1 aprovação, sem push direto)
- Branch de desenvolvimento: `dev` (protegida via Ruleset, mesmas regras — **branch padrão do
  repositório desde 19/08/2026**, ver seção 2)
- Branches de feature: `feature/comandas`, `feature/cardapio-digital`, `feature/clusterizacao`,
  `feature/programacao-linear`, `feature/lgpd`, `feature/mer-der`, `feature/docs-licenca`, uma por
  bloco/módulo ou tarefa de documentação
- Pasta `docs/` guarda este arquivo, o MER/DER, e demais documentos de apoio do TG

> **Nota técnica:** a proteção de branch inicialmente configurada como "branch protection rule"
> clássica não era aplicada de fato (repositório privado no plano GitHub Free não aplica a regra,
> só a exibe como configurada — aviso "Not enforced" na tela de Settings). Resolvido migrando para
> **Rulesets** após tornar o repositório público (branch protection em repositório público é
> gratuita em qualquer plano). Ver decisão completa na seção 2.

> **Nota de cautela — auto-delete e branches protegidas:** com "Automatically delete head branches"
> ativo, a documentação do GitHub garante que branches protegidas (como `main`) são poupadas da
> exclusão automática — mas há relatos de usuários de comportamento inconsistente especificamente
> com Rulesets (diferente da branch protection clássica). Prática recomendada: sempre conferir
> manualmente que `main` continua existindo após qualquer PR onde ela é a branch de origem (`compare`),
> como no fluxo `main → dev` do processo de release abaixo.

### Labels
`bloco:bi` · `bloco:clusterizacao` · `bloco:pl` · `bloco:lgpd` · `bloco:comandas` ·
`bloco:cardapio-digital` · `tipo:documentacao` · `tipo:modelagem` · `tipo:codigo` ·
`tipo:infraestrutura` · `prioridade:alta` · `prioridade:media` · `prioridade:baixa` · `bug` ·
`enhancement` · `question` — mantidas por cobrirem categoria útil não coberta pelas labels
customizadas. Labels padrão do GitHub sem uso claro no contexto de dupla (`duplicate`, `invalid`,
`wontfix`, `help wanted`, `good first issue`, `documentation`) sugeridas para remoção — ver checklist
"A fazer", seção Infraestrutura.

### GitHub Projects — board "GASTRA - TG"
Link: https://github.com/users/danih-vb/projects/3

Colunas (Status): **Backlog** → **A Fazer** → **Em Andamento** → **Em Revisão** → **Concluído**.

Transições manuais vs. automáticas (detalhado em `CONTRIBUTING.md`, seção 5):
- Backlog → A Fazer → Em Andamento: manual
- Em Andamento → Em Revisão: automático (workflow "Pull request linked to issue")
- Em Revisão → Concluído: automático (workflows "Pull request merged" e "Item closed")
- Item novo → Backlog: automático (workflow "Item added to project", alimentado por "Auto-add to
  project" sem filtro de label)

Campo **Iteration** configurado, duração ajustada para 1 semana (ver seção 2) — atribuição de sprint
a cada item é manual, sem workflow automático para isso.

### Issues do checklist
Criadas via `scripts/create_issues_from_checklist.sh` (issues #1-#15) e `scripts/create_sprint_issues.sh`
(sub-issues + issues novas de diagrama/notação). Issues guarda-chuva (genéricas) são quebradas em
sub-issues nativas do GitHub conforme o trabalho fica concreto — ver `CONTRIBUTING.md`, seção 4.

### Processo de release (praticado em 19/08/2026)
1. `release/<versao>` nasce de `dev`, recebe só ajustes finais.
2. PR `release/<versao> → main`, revisão do Pedro, merge.
3. Criar tag anotada na `main` (`git tag -a v<versao> -m "..."` + `git push origin v<versao>`) —
   é a tag, não a branch, que preserva o checkpoint permanentemente.
4. PR `release/<versao> → dev` para trazer qualquer ajuste feito na release de volta à integração.
   **Se a branch de release já tiver sido apagada** (auto-delete ativo) antes desse passo, usar
   `main → dev` no lugar — o conteúdo já está acessível via `main`, então funciona igual.
5. Deletar `release/<versao>` (manual ou automático) — seguro fazer, o conteúdo já está preservado
   pela tag e pelos merges.

Primeira tag criada: `v0.1-setup-inicial` (infraestrutura + documentação base, sem código ainda).

---

## 5. Milestones e Sprints

Milestones (marcos temáticos com prazo) e Iterations/Sprints (blocos semanais de trabalho) são
campos independentes no GitHub Projects — ver `CONTRIBUTING.md`, seção 5, para a diferença entre os
dois conceitos.

### Milestones (datas reajustadas em 19/08/2026)

As datas originais eram otimistas demais (M1 venceria em apenas 2 dias após a criação). Reajustadas
mantendo o espaçamento relativo entre elas, com atenção especial a M2, que depende da agenda do
orientador:

| Milestone | Data |
|---|---|
| M1 - Levantamento de Requisitos | 28/08/2026 |
| M2 - Validação de Escopo | 05/09/2026 ⚠️ depende de agenda do orientador, tratar como meta, não garantia |
| M3 - Modelagem | 18/09/2026 |
| M4 - Módulo de Comandas (MVP) | 05/10/2026 |
| M5 - BI | 19/10/2026 |
| M6 - Ciência de Dados | 26/10/2026 |
| M7 - Programação Linear | 09/11/2026 |
| M8 - LGPD | 16/11/2026 |
| M9 - Redação Final (ABNT) | 23/11/2026 |
| M10 - Defesa | 07/12/2026 |

### Sprint 1 (19–26/08/2026)

Foco: destravar M1. A entrevista com o garçom do Cocobambu, agendada para 20/08/2026, destrava as
issues #5 e #6 (dependentes dos achados sistematizados).

Issues planejadas para esta sprint: #1 (+ 3 sub-issues), #2 (+ 3 sub-issues), #3, #4, #5 (+ 2
sub-issues), #6, e a issue "Confirmar notação esperada para diagramas de apoio com o orientador".
Atribuições (Danih/Pedro) combinadas no planejamento — ver comentários nas issues correspondentes.

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

**Próximo passo desta seção:** reanalisar quando a amostra crescer (a entrevista de 20/08 deve
somar achados qualitativos); se os achados se confirmarem, viram parágrafos formais na seção de
resultados do TG (com o `[TG]` de cada item indicando pra que bloco cada achado serve).

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
- **19/08/2026** — PR `feature/docs-licenca` aprovado pelo Pedro e mergeado em `dev`. Primeira
  contribuição em par completa, do fluxo de branch até o merge revisado.
- **19/08/2026** — Milestones M1-M10 criados no GitHub com datas iniciais; datas reajustadas no
  mesmo dia por serem consideradas otimistas demais frente ao trabalho real restante (ver seção 5).
- **19/08/2026** — Branch padrão do repositório alterada de `main` para `dev`, reduzindo risco de
  abrir PR sem querer contra a branch errada.
- **19/08/2026** — Duração do campo Iteration ajustada de 2 semanas para 1 semana — sprints
  semanais fazem mais sentido na fase atual de documentação (mais rápida que a fase de
  implementação que vem a seguir).
- **19/08/2026** — Praticado o fluxo completo de release (`release/0.1-setup-inicial` → `main` +
  tag `v0.1-setup-inicial` → `dev`), incluindo o cenário de branch de release apagada pelo
  auto-delete antes do merge para `dev` — resolvido usando PR `main → dev` no lugar.
- **19/08/2026** — `scripts/create_sprint_issues.sh` criado e executado: sub-issues adicionadas a
  #1, #2, #5, #10, #11, #12 (sem duplicar as issues-mãe já existentes); novas issues-pai criadas
  para notação de diagramas e diagramas de casos de uso/classes.
- **19/08/2026** — Roteiro de entrevista com garçom do Cocobambu adicionado ao repositório em
  `docs/requisitos/entrevistas/`. Entrevista agendada para 20/08/2026.
- **19/08/2026** — Sprint 1 planejada: issues atribuídas à Iteration "Sprint 1" e movidas de
  Backlog para "A Fazer", com atribuição de responsável (Danih/Pedro) combinada entre a dupla.
