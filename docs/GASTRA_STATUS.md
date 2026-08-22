# GASTRA — Status do Projeto

TG (Trabalho de Graduação) — FATEC Araraquara
Alunos: Daniel (Danih) e Pedro | Orientador: Prof. Me. Leonardo José de Lima Ferrucci
Última atualização: 21/08/2026

> **Como manter este arquivo vivo:** ele fica versionado no repositório (`docs/GASTRA_STATUS.md`).
> Toda vez que um item mudar de status, editem aqui e façam commit com uma mensagem clara.

---

## 1. Escopo do projeto (revisado em 21/08/2026 — validado com o orientador)

**Foco e entrega central do TG (inalterado):**
1. BI (Business Intelligence)
2. Ciência de dados — clusterização e regras de associação (recomendação de pratos)
3. Programação linear — alocação/distribuição de garçons
4. Conformidade LGPD

**O que mudou nesta validação:**

| Item | Status até 17/08 | Status a partir de 21/08 |
|---|---|---|
| Núcleo do módulo de comandas (abrir/pedir/fechar) | 🟡 Pendente de validação | 🟢 **Validado como adicional** — necessário para alimentar os 4 blocos, mas **não é o foco** do TG |
| Cardápio digital (consulta, QR/tablet) | 🟠 Pendente, extensão opcional | 🟠 **Validado como extra opcional** — só se sobrar tempo |
| Cliente acompanhar a comanda em tempo real | (não existia) | 🟠 **Validado como extra opcional** — achado da entrevista Cocobambu, só se sobrar tempo |
| Lista de pendências do garçom (itens não entregues) | (não existia) | 🟠 **Validado como extra opcional** — achado da entrevista, simples de implementar |
| Integração com a cozinha (acesso a pedidos, confirmar preparo) | Nunca cogitado | ⚫ **Fora do escopo do TG** — feature futura, pós-defesa |

**Regra prática:** esforço de desenvolvimento e de documentação prioriza sempre o núcleo (comandas) +
os 4 blocos analíticos. Os itens 🟠 só começam depois que houver um MVP apresentável do núcleo. O
item ⚫ não entra em nenhuma issue deste ciclo do TG — se surgir vontade de adiantar algo da cozinha,
registrar como ideia futura, não como tarefa.

---

## 2. Decisões de design já fechadas

| Decisão | Resolução | Data |
|---|---|---|
| Captação da composição da mesa | Híbrido: sistema sugere por regras, garçom confirma/ajusta — confirmado por entrevista (sem item infantil pedido, sistema não tem como inferir sozinho) | 13/08/2026, confirmado 21/08/2026 |
| Uso de atributos sensíveis na segmentação | Excluídos deliberadamente; sinais observáveis + comunicação voluntária | 13/08/2026 |
| Regras de inferência de estrutura da mesa | Solo/Casal/Grupo pequeno/Família/Grupo grande | 13/08/2026 |
| Ferramenta de levantamento de requisitos | Jotform | 13/08/2026 |
| **Critério de rotação de garçons (RN03)** | **Proposta:** equilibrar faturamento acumulado por garçom, ponderado pelo faturamento histórico da praça — ver `GASTRA_Requisitos_RN.docx`, seção 3.1 | 21/08/2026 — decisão interna proposta, pendente de validação final |
| **KPIs de BI e ranking (RF10/RF11)** | Faturamento por garçom/praça, ticket médio, tempo médio de atendimento, índice composto (não só venda) — ver seção 3.2 do Requisitos_RN.docx | 21/08/2026 — decisão interna proposta |
| **Critério de recomendação de pratos (RF09)** | Regras de associação com limiares de suporte/confiança a calibrar; clusterização por padrão de consumo, nº de clusters a definir experimentalmente | 21/08/2026 — decisão interna proposta, calibração numérica pendente de dado |

**Pendente de decisão:**
- Qualificador livre vs. lista fechada de opções pro garçom (impacta o Bloco 6 — análise de dados)
- Notação dos diagramas de apoio (UML / BPMN / fluxograma simples)
- Valores numéricos de RNF01/RNF02 (X segundos, N toques)
- Pesos exatos do critério de rotação (RN03) — estrutura definida, calibração pendente de dado real/simulado

---

## 3. Checklist

### ✅ Concluído
- [x] Definição do escopo original (BI + clusterização/regras de associação + PL + LGPD)
- [x] Questionário de levantamento de requisitos — Garçom (24 perguntas, Jotform)
- [x] Questionário de levantamento de requisitos — Cliente (18 perguntas, Jotform)
- [x] Decisão de escopo: módulo de comandas como complemento operacional
- [x] Decisão de design: segmentação sem dado sensível
- [x] Regras de inferência de estrutura de mesa
- [x] Coleta de respostas encerrada (17/08/2026) — 2 garçons, 13 clientes
- [x] Roteiro de entrevista com garçom do Cocobambu criado e aplicado
- [x] Entrevista transcrita
- [x] **Validação de escopo com o orientador (21/08/2026)** — comandas como adicional, extras
      condicionados a tempo, cozinha fora do TG
- [x] RF/RNF/RN atualizados pós-entrevista (RF13 novo, RN03 com proposta de resolução)
- [x] Matriz de Rastreabilidade atualizada com os novos status
- [x] Achados do questionário cruzados com a entrevista (seção 6) — sem citação literal, agregado

### 🔄 Em andamento
- [ ] Exportar respostas brutas do Jotform via download nativo (CSV) e salvar em
      `data-science/data/raw/` — API não devolve o conteúdo de submissões em texto, só via UI, então
      esse passo é manual (ver seção 8)
- [ ] Definir ferramenta de análise (Python/pandas recomendado — ver seção 8) e montar o
      dado processado do cliente (n=13) em `data-science/data/processed/`
- [ ] Decidir se o Cocobambu é citado nominalmente no TG ou tratado como "restaurante colaborador"

### ⏳ A fazer

**Levantamento de requisitos**
- [ ] Redigir seção de resultados do TG com achados do questionário + entrevista, limitações de
      amostra explícitas

**Artefatos novos definidos em 21/08/2026**
- [ ] **Business Model Canvas** — `docs/negocio/`
- [ ] **User Stories** (obrigatório) — `docs/requisitos/`, formato "Como [ator], quero [ação], para
      [benefício]", derivadas dos RF já existentes
- [ ] **Prototipagem / UX-UI** — wireframes e protótipo navegável — `docs/ux-ui/`
- [ ] **Definição de arquitetura** (parte teórica do TG) — `docs/arquitetura/`
- [ ] **Tabela de cenários de teste executados** — `docs/testes/`
- [ ] **Manual de uso do usuário** — `docs/manual-usuario/`

**Documentação (TG)**
- [ ] Parágrafo de justificativa da expansão de escopo (comandas como adicional, não foco)
- [ ] Revisão ABNT do que já foi escrito até aqui

**Modelagem**
- [ ] MER do núcleo do módulo de comandas
- [ ] DER formal (a partir do MER)
- [ ] Modelagem do banco de dados (comanda, cardápio, taxa de serviço) com pontos LGPD sinalizados

**Diagramas de apoio**
- [ ] Confirmar notação esperada pela banca/orientador
- [ ] Fluxo do garçom, fluxo de recomendação, fluxo de alocação (PL)

**Back-end / Front-end**
- [ ] Implementação do núcleo de comandas
- [ ] Implementação dos algoritmos (clusterização/recomendação, PL com critério RN03)
- [ ] Extras (🟠), só depois do MVP do núcleo estar apresentável

**Infraestrutura**
- [x] Setup do repositório GitHub
- [x] Setup do GitHub Projects
- [ ] Atualizar issues/milestones assim que o cronograma oficial chegar (ver seção 9)

---

## 6. Resultados preliminares (questionário — encerrado em 17/08/2026)

**Atualizado em 21/08/2026 — cruzamento garçom × cliente + entrevista Cocobambu.**

> Amostras pequenas (garçom n=2, cliente n=13). Tratar como indicativo, não como estatisticamente
> representativo — isso deve ficar explícito na seção de limitações do TG. Nenhuma citação literal
> de resposta individual aparece abaixo, só padrão agregado (regra da seção 8).

### 6.1 Restrição alimentar / flags dietéticas no cardápio (RF05, RF09)
- **Cliente:** 77% relatam dificuldade em identificar se um prato é vegano/vegetariano/sem
  glúten/sem lactose (46% "às vezes", 31% "frequentemente"); só 23% nunca tiveram problema.
- **Garçom:** percepção dividida sobre a clareza da informação hoje (metade neutro, metade "muito
  claro") — ou seja, o garçom tende a achar que comunica bem, mas o cliente sente mais atrito do
  que isso sugere.
- **Entrevista Cocobambu:** confirma e explica o padrão acima — não é falha de comunicação do
  garçom, é falta de opção real no cardápio (poucos pratos vegano/vegetariano/sem glúten
  disponíveis, então a resposta do garçom é limitada por design do menu, não por informação).
- **Leitura para o TG:** os três achados se complementam em vez de se contradizerem — dá pra
  argumentar que RF05 (cardápio digital com flags) resolve o sintoma (dificuldade de identificar),
  mas não a causa raiz (pouca opção no menu), que está fora do escopo do GASTRA.

### 6.2 Rotação de praças por dados de faturamento (RN03)
- **Garçom:** 100% concordam (nota 4/5) que rotação baseada em dados de faturamento seria mais
  justa que escala fixa — consenso total, apesar de n pequeno.
- **Cliente:** tema não se aplica (fora do escopo da experiência do cliente).
- **Entrevista Cocobambu:** dá o "porquê" por trás do número — hoje a distribuição é hierárquica
  (chefe de fila escolhe primeiro), e o entrevistado deu exemplo concreto de praças com potencial
  de faturamento estruturalmente diferente (casal vs. família).
- **Leitura para o TG:** esse é o achado mais forte do conjunto todo — sustenta diretamente a
  proposta de RN03 (seção 3.1 do `GASTRA_Requisitos_RN.docx`).

### 6.3 Sugestão automática de combinações de pratos (RF09)
- **Cliente:** interesse alto, média 4,31/5.
- **Garçom:** interesse dividido (metade baixo, metade alto) — sinal de que a ferramenta precisa
  ser apoio opcional, não substituição do julgamento do garçom (reforça RF07 — decisão humana
  final mantida).
- **Entrevista Cocobambu:** hoje a sugestão do garçom é baseada em "se o cliente já frequenta a
  casa" e em pratos de divulgação para clientes novos — é um critério pobre para clientes
  recorrentes com preferência conhecida, que é exatamente o que a clusterização/regras de
  associação (RF09) mira resolver.

### 6.4 Taxa de serviço (10%)
- **Cliente:** 54% sempre reparam no valor, mas resistência ativa é baixa (62% nunca pediram pra
  retirar).
- **Garçom:** confirma que o pedido de retirada acontece, mas é raro/às vezes — motivo citado é
  falta de costume do cliente em pagar e demora no atendimento.
- **Entrevista Cocobambu:** acrescenta um ângulo novo não capturado pelo questionário — risco de
  cliente ir embora sem pagar e disputas de conta (ex.: item caro que o cliente nega ter pedido).
  Esse achado motivou RF13 (comanda em tempo real) e RN02 (itens pendentes visíveis).

### 6.5 Ranking / gamificação (RF11)
- **Garçom:** 100% dizem que um ranking os motivaria.
- **Entrevista Cocobambu:** qualifica esse número — já existe algo informal (gerente consulta um
  painel às vezes) e o entrevistado nunca trabalhou com meta/ranking formal antes; também sugeriu
  espontaneamente que o critério não deveria ser só venda.
- **Leitura para o TG:** o número "100% motivaria" sozinho seria frágil na banca (n=2, resposta
  socialmente esperada); com a entrevista qualificando o contexto, fica mais defensável.

### 6.6 Personalização e recomendação — impacto no cliente
- **Cliente:** média de 9,46/10 para "quanto uma experiência mais ágil e personalizada faria você
  recomendar o restaurante" — o achado de maior magnitude de todo o questionário.
- **Leitura para o TG:** é o número mais forte para abrir a justificativa de negócio (Business
  Model Canvas, proposta de valor) — mas com n=13, não deve ser citado como "quase 10 em cada 10
  clientes", e sim como indicativo de uma amostra pequena.

---


## 7. Log de decisões

- **13/08 e 17/08/2026** — ver histórico anterior no `git log`.
- **20/08/2026** — Entrevista com garçom do Cocobambu realizada e transcrita.
- **21/08/2026** — Validação de escopo com o orientador: núcleo de comandas aprovado como adicional
  (não foco); cardápio digital, comanda em tempo real e lista de pendências aprovados como extras
  condicionados a tempo; integração com a cozinha formalmente excluída do TG (feature pós-defesa).
- **21/08/2026** — RN03 recebe proposta de resolução baseada em entrevista + questionário; KPIs de
  BI/ranking e critério de recomendação de pratos definidos como proposta (ver seção 3 do
  Requisitos_RN.docx).
- **21/08/2026** — Definidos novos artefatos obrigatórios/planejados: Business Model Canvas, User
  Stories, prototipagem/UX-UI, arquitetura, cenários de teste, manual do usuário.
- **21/08/2026** — Perguntas dos dois formulários recuperadas via API do Jotform e documentadas em
  `docs/requisitos/GASTRA_Instrumento_Coleta.md`. Achados do questionário cruzados com a entrevista
  Cocobambu (seção 6). Ferramenta de análise definida: Python/pandas.

---

## 8. Dados do questionário e da entrevista — onde ficam e como tratar

**Regra geral (já em `CONTRIBUTING.md`, seção 6): dado bruto identificável nunca vai para o Git.**
Isso vale tanto para a transcrição da entrevista quanto para a exportação bruta do Jotform.

| O quê | Onde fica | Vai para o Git? |
|---|---|---|
| Transcrição bruta da entrevista (docx) | `data-science/data/raw/` local (pasta já no `.gitignore`) + backup no Google Drive restrito | **Não** |
| Exportação bruta do Jotform (CSV/JSON, resposta por resposta) | `data-science/data/raw/` local | **Não** |
| Achados da entrevista, anonimizados e em texto corrido (sem nome, sem caso identificável de cliente) | `docs/GASTRA_STATUS.md`, seção 6 (preenchida em 21/08/2026) | **Sim** |
| Dado do questionário do **cliente** (n=13), agregado/anonimizado | `data-science/data/processed/` | **Sim** |
| Dado do questionário do **garçom** (n=2) | **Não estruturar como dataset** — com amostra de 2, até um "agregado" praticamente expõe a resposta individual. Manter só como texto narrativo no STATUS.md, como já está (seção 6) | **Não como dataset** |

**Sobre recuperar as perguntas originais:** já feito — `docs/requisitos/GASTRA_Instrumento_Coleta.md`
tem a lista completa das perguntas dos dois formulários (puxadas direto da API do Jotform em
21/08/2026), sem nenhuma resposta. Esse é o documento a citar no TG como instrumento de coleta.

**Sobre exportar as respostas brutas:** a API do Jotform disponível aqui só permite *analisar*
submissões (retorna estatística agregada) ou *exibir* a listagem numa interface — não devolve o
conteúdo bruto em texto pra automação. Então esse passo específico continua manual: Jotform →
Configurações do formulário → Respostas → Baixar (CSV), salvar direto em `data-science/data/raw/`.

**Ferramenta de análise — decisão em 21/08/2026:** **Python + pandas**, começando em notebook
exploratório (`data-science/notebooks/`), promovido pra `data-science/src/` quando estabilizar.
Racional: (1) já é a linguagem de todo o resto do bloco de Ciência de Dados e Programação Linear —
não faz sentido introduzir uma segunda ferramenta (Excel/Power BI) só pra essa etapa e depois
reescrever em Python de qualquer forma; (2) reprodutibilidade citável na seção de metodologia do TG
(notebook versionado > cálculo manual em planilha); (3) Excel é mais rápido pra uma conferência
visual pontual com n tão pequeno — pode ser usado como checagem rápida informal, mas não como
ferramenta oficial de análise citada no TG; (4) Power BI não está na stack definida (ASP.NET Core +
Angular + Python) e seria uma ferramenta a mais só pra essa etapa exploratória — não recomendado.

**Sobre os dois arquivos-guia da FATEC** (Guia de Orientação do TG II e Modelo de Estrutura):
recomendo `docs/pesquisa/referencias/` — são material institucional de referência (não autoral do
GASTRA), mas vale versionar para rastrear qual versão do guia a dupla seguiu, especialmente se a
FATEC atualizar o modelo depois.

---

## 9. Cronograma oficial — aguardando

O cronograma esperado deve chegar no fim de semana. Até lá:
- Não mexer em Milestones no GitHub Projects (evitar retrabalho se as datas vierem diferentes do
  que está hoje).
- Os itens novos desta seção (Canvas, User Stories, etc.) já podem virar Issues no Backlog — a
  ausência de data não impede a issue existir, só a Milestone/Iteration exata.
- Assim que o cronograma chegar: revisar M1–M10, marcar Issues concluídas (RF/RN atualizados, Matriz
  atualizada, entrevista realizada), e distribuir os itens novos entre as Milestones existentes ou
  criar uma nova, conforme o prazo real informado.
