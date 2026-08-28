# GASTRA — Status do Projeto

TG (Trabalho de Graduação) — FATEC Araraquara
Alunos: Daniel Velluto Bento e Pedro Luis Otrente de Campos | Orientador: Prof. Me. Leonardo José de Lima Ferrucci
Última atualização: 27/08/2026 (Sprint 1 encerrada por completo)

> **Como manter este arquivo vivo:** ele fica versionado no repositório (`docs/GASTRA_STATUS.md`).
> Toda vez que um item mudar de status, edite aqui e faça commit com uma mensagem clara.

---

## 1. Escopo do projeto

**Foco e entrega central do TG:**
1. BI (Business Intelligence)
2. Ciência de dados — clusterização e regras de associação (recomendação de pratos)
3. Programação linear — alocação/distribuição de garçons
4. Conformidade LGPD


| Item | Status |
|---|---|
| Núcleo do módulo de comandas (abrir/pedir/fechar) | 🟢 **Validado como adicional** — necessário para alimentar os 4 blocos, mas **não é o foco** do TG |
| Cardápio digital (consulta, QR/tablet) | 🟠 **Validado como extra opcional** — só se sobrar tempo |
| Cliente acompanhar a comanda em tempo real | 🟠 **Validado como extra opcional** — achado da entrevista, só se sobrar tempo |
| Lista de pendências do garçom (itens não entregues) | 🟠 **Validado como extra opcional** — achado da entrevista, simples de implementar |
| Integração com a cozinha (acesso a pedidos, confirmar preparo) | ⚫ **Fora do escopo do TG** — feature futura, pós-defesa |
| Programa de fidelização (cadastro do cliente, identificador persistente, histórico entre visitas) | ⚫ **Fora do escopo do TG** — feature futura, pós-defesa (decisão de 27/08/2026) |

**Regra prática:** esforço de desenvolvimento e de documentação prioriza sempre o núcleo (comandas) +
os 4 blocos analíticos. Os itens 🟠 só começam depois que houver um MVP apresentável do núcleo. O
item ⚫ não entra em nenhuma issue deste ciclo do TG.

---

## 2. Decisões de design

| Decisão | Resolução | Data |
|---|---|---|
| Captação da composição da mesa | Híbrido: sistema sugere por regras, garçom confirma/ajusta — confirmado por entrevista | 13/08/2026, confirmado 21/08/2026 |
| Uso de atributos sensíveis na segmentação | Excluídos deliberadamente; sinais observáveis + comunicação voluntária | 13/08/2026 |
| Regras de inferência de estrutura da mesa | Solo/Casal/Grupo pequeno/Família/Grupo grande | 13/08/2026 |
| Ferramenta de levantamento de requisitos | Jotform | 13/08/2026 |
| **Critério de rotação de garçons (RN03)** | **Proposta:** equilibrar faturamento acumulado por garçom, ponderado pelo faturamento histórico da praça — ver `GASTRA_Requisitos_RN.docx`, seção 3.1 | 21/08/2026 — decisão interna proposta |
| **KPIs de BI e ranking (RF10/RF11)** | Faturamento por garçom/praça, ticket médio, tempo médio de atendimento, índice composto (não só venda) | 21/08/2026 — decisão interna proposta |
| **Critério de recomendação de pratos (RF09)** | Regras de associação com limiares de suporte/confiança a calibrar; clusterização por padrão de consumo, nº de clusters a definir experimentalmente | 21/08/2026 — decisão interna proposta, calibração numérica pendente de dado |
| **Notação dos diagramas de apoio** | UML (casos de uso, classes, sequência, atividades) | 21/08/2026 |
| **Função objetivo do algoritmo de PL (RN03)** | Estrutura fechada (issue #6) — equilibrar faturamento acumulado por garçom, ponderado pelo faturamento histórico da praça | 22/08/2026 |
| **Qualificador de restrição/preferência alimentar (RF14)** | Modelo híbrido: lista fechada de categorias comuns + campo de observação livre, vinculado à comanda ativa; o campo livre não entra na análise estruturada do Bloco 6 | 23/08/2026 — issue #7 |
| **Resolução da contradição RF12 x RN04/RN05** | RF12 removido do escopo — sem identificador persistente de cliente entre visitas, não existe histórico pessoal individualizável para excluir após a comanda fechar/agregar. Minimização por desenho substitui a funcionalidade de exclusão | 23/08/2026 — issue #53 |
| **Consolidação de #26/#54/#55 (RN03)** | Issue #26 fechada como duplicata; #54 (validação estrutural do RN03 com o orientador) tornou-se sub-issue de #5; #55 (calibração numérica) segue no Milestone M5, sem sobreposição | 27/08/2026 |
| **Estrutura final do RN03** | Validada com o orientador: dois fatores combinados por soma ponderada — faturamento acumulado ponderado pelo histórico da praça + tempo desde a última alocação em praça de alto potencial (w1+w2=1, ambos normalizados). Issue #54 fechada. Ver `GASTRA_Requisitos_RN.docx`, seção 3.1 | 27/08/2026 |
| **Fonte de dado para calibração de RN03** | Dado simulado, com premissas documentadas, confirmado como suficiente para o TG. Issue #55 desbloqueada | 27/08/2026 |
| **Valores de RNF01/RNF02** | Definidos como meta inicial: 2 segundos (RNF01) e 5 toques (RNF02), baseados em referencial de usabilidade (Nielsen); a validar empiricamente na Etapa 4 (M6) | 27/08/2026 |
| **Programa de fidelização** | Mantido fora do escopo do TG — feature futura pós-defesa. Contradiz o compromisso de anonimização já assumido no projeto de pesquisa formal | 27/08/2026 |
| **Milestones M3–M10 reestruturadas** | Realinhadas ao Cronograma de Entregas do TGII da coordenação; blocos analíticos tratados como frentes paralelas dentro de uma sprint única de backend. Ver `GASTRA_MEMORIA_GITHUB_PROJECTS.md`, seção 13 | 27/08/2026 |

**Pendente de decisão:**
- Pesos exatos (w1, w2) do critério de rotação (RN03) — estrutura fechada, calibração numérica em andamento com dado simulado (issue #55).
- Limiares de suporte/confiança do RF09 — depende de volume de dado.

---

## 3. Checklist

### ✅ Concluído
- [x] Definição do escopo original (BI + clusterização/regras de associação + PL + LGPD)
- [x] Questionário de levantamento de requisitos — Garçom (24 perguntas, Jotform, n=2)
- [x] Questionário de levantamento de requisitos — Cliente (18 perguntas, Jotform, n=13)
- [x] Decisão de escopo: módulo de comandas como complemento operacional
- [x] Decisão de design: segmentação sem dado sensível
- [x] Regras de inferência de estrutura de mesa
- [x] Coleta de respostas encerrada (17/08/2026)
- [x] Roteiro de entrevista com garçom do restaurante colaborador criado e aplicado
- [x] Entrevista transcrita e achados sistematizados (issue #4, fechada em 23/08/2026)
- [x] **Validação de escopo com o orientador (21/08/2026)**
- [x] RF/RNF/RN atualizados pós-entrevista
- [x] Matriz de Rastreabilidade atualizada com os novos status
- [x] Achados do questionário cruzados com a entrevista — agora com documento próprio: `GASTRA_Dados_Processados.docx`
- [x] Notação de diagramas de apoio confirmada com o orientador (UML)
- [x] User Stories formalizadas (`GASTRA_User_Stories.docx`)
- [x] Função objetivo do algoritmo de PL (RN03) — estrutura definida (issue #6)
- [x] Especificação de gráficos por bloco temático (issue #22)
- [x] Exportação manual do CSV do Jotform (garçom e cliente) — `data-science/data/raw/` (fora do Git)
- [x] Dados de questionário e entrevista processados, interpretados e versionados (`GASTRA_Dados_Processados.docx`)
- [x] Decisão do qualificador de restrição alimentar — modelo híbrido (RF14, issue #7)
- [x] Resolução da contradição RF12 x RN04/RN05 — RF12 removido do escopo (issue #53)
- [x] Consolidação das issues de acompanhamento de RN03 (#26/#54/#55)
- [x] Validação formal de RN03 com o orientador (issue #54) — estrutura final fechada em 27/08/2026
- [x] Valores de RNF01/RNF02 definidos (2s / 5 toques)
- [x] Decisão sobre programa de fidelização — mantido fora do escopo do TG
- [x] Reestruturação das Milestones M3–M10 alinhada ao cronograma oficial do TGII
- [x] Fechamento completo da Sprint 1 (27/08/2026)

### 🔄 Em andamento
- [ ] Decidir se o restaurante colaborador é citado nominalmente no TG ou tratado como "restaurante colaborador" (permanece como termo padrão até decisão contrária)

### ⏳ A fazer

**Levantamento de requisitos**
- [ ] Redigir seção de resultados do TG com achados do questionário + entrevista (base pronta em `GASTRA_Dados_Processados.docx`), limitações de amostra explícitas

**Artefatos novos definidos em 21/08/2026 (Sprint 2)**
- [ ] **Business Model Canvas** — `docs/negocio/`
- [ ] **Prototipagem / UX-UI** — `docs/ux-ui/`
- [ ] **Definição de arquitetura** — `docs/arquitetura/`
- [ ] **Tabela de cenários de teste executados** — `docs/testes/`
- [ ] **Manual de uso do usuário** — `docs/manual-usuario/`

**Documentação (TG)**
- [ ] Parágrafo de justificativa da expansão de escopo (comandas como adicional, não foco)
- [ ] Revisão ABNT do que já foi escrito até aqui

**Modelagem (Sprint 2)**
- [ ] MER do núcleo do módulo de comandas
- [ ] DER formal (a partir do MER)
- [ ] Modelagem do banco de dados (comanda, cardápio, taxa de serviço) com pontos LGPD sinalizados

**Diagramas de apoio (Sprint 2)**
- [x] Confirmar notação esperada pela banca/orientador — UML, confirmado 21/08/2026
- [ ] Diagrama de casos de uso
- [ ] Diagrama de classes (depende do DER)
- [ ] Diagrama de sequência — fluxo do garçom, fluxo de alocação (PL), fluxo de recomendação
- [ ] Diagrama de atividades — mesmos fluxos principais

**Back-end / Front-end**
- [ ] Implementação do núcleo de comandas
- [ ] Implementação dos algoritmos (clusterização/recomendação, PL com critério RN03)
- [ ] Extras (🟠), só depois do MVP do núcleo estar apresentável

**Infraestrutura**
- [x] Setup do repositório GitHub
- [x] Setup do GitHub Projects
- [ ] Atualizar issues/milestones assim que o cronograma oficial chegar

---

## 6. Resultados preliminares (questionário e entrevista)

**A partir de 23/08/2026, os achados detalhados, com gráficos e interpretação, vivem em
[`data-science/data/processed/GASTRA_Dados_Processados.docx`](../data-science/data/processed/GASTRA_Dados_Processados.docx).**
Esta seção mantém apenas um resumo executivo; para qualquer citação numérica no texto do TG, usar o
documento processado como fonte, não este resumo.

> Amostras pequenas (garçom n=2, cliente n=13).

- **Flags dietéticas:** 77% dos clientes relatam dificuldade em identificar pratos vegano/vegetariano/sem glúten/sem lactose no cardápio; a entrevista mostra que a causa raiz é escassez de opções no cardápio, não falha de comunicação do garçom.
- **Rotação de praças (RN03):** 100% dos garçons concordam que rotação por dado seria mais justa que escala fixa; entrevista fornece o racional concreto (praças com potencial de faturamento estruturalmente diferente).
- **Sugestão automática de pratos (RF09):** interesse alto do cliente (média 4,31/5); interesse dividido do garçom — reforça que a ferramenta deve ser apoio opcional, não substituição do julgamento humano.
- **Taxa de serviço:** 54% dos clientes sempre reparam no valor, mas resistência ativa é baixa (62% nunca pediram para retirar); a entrevista traz o ângulo de disputa de conta, que motivou RF13.
- **Ranking/gamificação (RF11):** 100% dos garçons dizem que um ranking motivaria; a entrevista qualifica esse número (nenhum já trabalhou com meta formal antes).
- **Personalização:** média de 9,46/10 para o quanto uma experiência mais ágil e personalizada aumentaria a recomendação do restaurante — achado de maior magnitude, base da justificativa de negócio (Sprint 2).
- **Conforto com histórico anonimizado (LGPD):** 84% dos clientes aceitam histórico de pedidos sem nome/CPF para recomendação futura, mas 46% condicionam isso a poder apagar o histórico quando quiser — achado que motivou a discussão de RF12/RN04/RN05 (issue #53, resolvida em 23/08/2026: ver seção 2).

---

## 7. Log de decisões

- **13/08 e 17/08/2026** — ver histórico anterior no `git log`.
- **20/08/2026** — Entrevista com garçom do restaurante colaborador realizada e transcrita.
- **21/08/2026** — Validação de escopo com o orientador: núcleo de comandas aprovado como adicional; cardápio digital, comanda em tempo real e lista de pendências aprovados como extras condicionados a tempo; integração com a cozinha formalmente excluída do TG.
- **21/08/2026** — RN03 recebe proposta de resolução; KPIs de BI/ranking e critério de recomendação de pratos definidos como proposta.
- **21/08/2026** — Definidos novos artefatos obrigatórios/planejados: Business Model Canvas, User Stories, prototipagem/UX-UI, arquitetura, cenários de teste, manual do usuário.
- **21/08/2026** — Perguntas dos dois formulários recuperadas via API do Jotform (`GASTRA_Instrumento_Coleta.md`). Ferramenta de análise definida: Python/pandas.
- **22/08/2026** — Função objetivo do algoritmo de PL (RN03) formalizada em sua estrutura (issue #6 fechada). Validação final com o orientador e calibração numérica dos pesos seguem pendentes.
- **22/08/2026** — Exportação manual do CSV do Jotform (garçom e cliente) concluída, salva em `data-science/data/raw/` (fora do Git).
- **22/08/2026** — Especificação de gráficos por bloco temático definida (issue #22).
- **22/08/2026** — Fechamento da Sprint 1 (ver seção 10, atualizada em 23/08).
- **23/08/2026** — PR #52 mesclada, fechando #6, #18 e #22. Issues #53, #54 e #55 criadas; issue #7 movida de Backlog para Sprint 1. Título da issue #3 corrigido para remover o nome do estabelecimento.
- **23/08/2026** — Sobreposição entre #26/#54/#55 resolvida: #26 fechada como duplicata de #54; #54 tornou-se sub-issue de #5.
- **23/08/2026** — Issue #4 (sistematização da entrevista) fechada — achados já cobertos na seção 6 e em `GASTRA_Dados_Processados.docx`.
- **23/08/2026** — Issue #7 fechada: decisão híbrida (lista fechada + campo livre) para restrição/preferência alimentar, formalizada como RF14/US11.
- **23/08/2026** — Issue #53 fechada: RF12 removido do escopo por minimização de dados por desenho — ver seção 2 e `GASTRA_Requisitos_RN.docx`, seção 5.1.
- **23/08/2026** — Dados de questionário e entrevista processados e versionados em `GASTRA_Dados_Processados.docx`; pendências com o orientador organizadas em `GASTRA_Pendencias_Orientador.docx`.
- **27/08/2026** — Cronograma oficial do TGII recebido (`Cronograma_TGII_ADS_2026.pdf`); Milestones M3–M10 reestruturadas no GitHub Projects para alinhar às 8 etapas oficiais; issues reatribuídas; `GASTRA_MEMORIA_GITHUB_PROJECTS.md` atualizado.
- **27/08/2026** — Reunião com o orientador: RN03 validado em sua estrutura final (dois fatores, soma ponderada); dado simulado aceito como fonte para calibração numérica (issue #55); argumento de remoção de RF12 confirmado como aceitável para a banca; decisão de manter o programa de fidelização fora do escopo do TG.
- **27/08/2026** — RNF01/RNF02 definidos como meta inicial (2s / 5 toques), com base em referencial de usabilidade (Nielsen).
- **27/08/2026** — Issue #54 fechada; Sprint 1 encerrada por completo.

---

## 8. Dados do questionário e da entrevista

**Regra geral (já em `CONTRIBUTING.md`, seção 6): dado bruto identificável nunca vai para o Git.**

| O quê | Onde fica | Vai para o Git? |
|---|---|---|
| Transcrição bruta da entrevista (docx) | `data-science/data/raw/` local + backup restrito | **Não** |
| Exportação bruta do Jotform (CSV, resposta por resposta) | `data-science/data/raw/` local | **Não** |
| Dados processados, agregados e interpretados (cliente + garçom + entrevista, com gráficos) | `data-science/data/processed/GASTRA_Dados_Processados.docx` | **Sim** |
| Achados da entrevista, em texto corrido resumido | `docs/GASTRA_STATUS.md`, seção 6 | **Sim** |

**Sobre recuperar as perguntas originais:** `docs/requisitos/GASTRA_Instrumento_Coleta.md` tem a lista completa das perguntas dos dois formulários, sem nenhuma resposta.

**Ferramenta de análise:** Python + pandas, notebook exploratório (`data-science/notebooks/`), promovido para `data-science/src/` quando estabilizar.

---

## 9. Cronograma oficial — recebido em 27/08/2026

O `Cronograma_TGII_ADS_2026.pdf` chegou com 8 etapas de entrega fixadas pela coordenação do curso,
cobrindo de 26/08/2026 até a banca (janela 30/11–05/12/2026). As Milestones M3–M10 do GitHub
Projects foram reestruturadas no mesmo dia para alinhar a essas etapas — detalhamento completo em
`GASTRA_MEMORIA_GITHUB_PROJECTS.md`, seção 13. Essa reestruturação ainda não foi validada
formalmente com o Prof. Ferrucci (ver `GASTRA_Pendencias_Orientador.docx`, seção 4).

---

## 10. Fechamento da Sprint 1 (concluído em 27/08/2026)

PR #52 mesclada, fechando #6, #18 e #22. Ao longo do dia 23/08, as pendências estruturais da Sprint 1
foram resolvidas: #4 (entrevista sistematizada), #7 (qualificador híbrido), #53 (RF12 removido), e a
sobreposição #26/#54/#55 (consolidada).

**Item que fechou a Sprint 1:**
- **#54 (sub-issue de #5) — Validar RN03 com o orientador**: reunião realizada em 27/08/2026.
  Estrutura final validada (dois fatores, soma ponderada); dado simulado aceito como fonte para a
  calibração numérica (#55, que segue aberta no Milestone M5 — calibração não é pré-requisito para
  a Sprint 2 começar). Issue #54 fechada; #5 fecha junto por consequência.

Junto com o fechamento de #54, ficaram definidos nesta mesma rodada: valores de RNF01/RNF02 (meta
inicial), o argumento de banca para a remoção de RF12 (confirmado pelo orientador), e a decisão de
manter o programa de fidelização fora do escopo do TG. Toda a Sprint 1 está, portanto, encerrada.

Business Model Canvas, MER/DER e diagramas de apoio (UML) — já mapeados nas Milestones M3 e M4 —
são o conteúdo da Sprint 2, cujo planning segue logo após o Sprint Review/Retrospectiva da Sprint 1
e a release de apresentação ao orientador.

Pauta completa e histórico de decisões com o orientador: ver `docs/GASTRA_Pendencias_Orientador.docx`.
