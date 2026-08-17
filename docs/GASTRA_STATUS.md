# GASTRA — Status do Projeto

TG (Trabalho de Graduação) — FATEC Araraquara
Alunos: Daniel (Danih) e Pedro | Orientador: Prof. Me. Leonardo José de Lima Ferrucci
Última atualização: 17/08/2026 (revisão: hierarquia front-of-house, atores, cardápio digital opção B)

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
pressuponham a aprovação.

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

### 🔄 Em andamento
- [ ] Divulgação dos questionários para garçons e clientes / coleta de respostas
      (parcial em 17/08/2026: 2 respostas de garçom, 12 de cliente — amostra ainda pequena, análise
      abaixo é preliminar, não conclusiva)

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
- [ ] Setup do repositório GitHub (ver seção 4)
- [ ] Setup do GitHub Projects (ver seção 4)

---

## 4. Setup do GitHub (roteiro)

### Repositório
- Nome sugerido: `gastra` ou `gastra-tg`
- Branch principal: `main` (protegida — sem push direto, só via PR)
- Branch de desenvolvimento: `dev`
- Branches de feature: `feature/comandas`, `feature/cardapio-digital`, `feature/clusterizacao`,
  `feature/programacao-linear`, `feature/lgpd`, um por bloco/módulo
- Pasta `docs/` para guardar este arquivo, o MER/DER, e demais documentos de apoio do TG

### Labels sugeridas
`bloco:bi` · `bloco:clusterizacao` · `bloco:pl` · `bloco:lgpd` · `bloco:comandas` ·
`tipo:documentacao` · `tipo:modelagem` · `tipo:codigo` · `prioridade:alta` · `prioridade:media` ·
`prioridade:baixa`

### GitHub Projects — colunas do quadro
1. **Backlog** — tudo que está na seção "A fazer" acima, sem prazo definido ainda
2. **A Fazer (Sprint atual)** — o que vocês dois decidiram atacar agora
3. **Em Andamento**
4. **Em Revisão** — pronto, esperando o outro revisar (documentação principalmente, já que é tese
   de dupla — cada um deveria revisar o que o outro escreve antes de considerar concluído)
5. **Concluído**

### Como popular o Projects rapidamente
Cada item da seção 3 (Checklist) deste arquivo vira uma Issue no GitHub, com a label do bloco
correspondente, e entra automaticamente na coluna "Backlog" do Projects. Ao criar as issues, usem o
texto exato dos itens daqui — assim este arquivo e o board não divergem com o tempo.

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
