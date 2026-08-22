# Como contribuir com o GASTRA

Este documento é o guia de referência para Daniel e Pedro trabalharem no mesmo repositório sem
pisar no trabalho um do outro, e para manter o histórico do Git útil na hora de explicar decisões
na banca. Leiam isto antes do primeiro commit.

## 1. Modelo de branches — GitFlow

Usamos GitFlow simplificado (a versão completa tem branches `support/*` que não fazem sentido aqui).

| Branch | Papel | Regras |
|---|---|---|
| `main` | Sempre reflete a versão estável/entregável (ex.: o que foi apresentado numa entrega parcial ou na banca) | **Protegida.** Nunca commitar direto. Só recebe merge de `release/*` ou `hotfix/*` via Pull Request. |
| `dev` | Branch de integração — onde as features se juntam. **Branch padrão do repositório** desde 19/08/2026 | **Protegida.** Só recebe merge de `feature/*` via Pull Request. |
| `feature/<nome-curto>` | Uma funcionalidade ou bloco específico | Nasce de `dev`, volta para `dev`. Ex.: `feature/comandas`, `feature/cardapio-digital`, `feature/clusterizacao`, `feature/programacao-linear`, `feature/lgpd`, `feature/mer-der` |
| `release/<versao>` | Preparação de uma entrega (ex.: entrega parcial do TG) | Nasce de `dev`, só ajustes finais (docs, bugs pequenos), depois vai para `main` **e** volta para `dev` |
| `hotfix/<descricao>` | Correção urgente em algo já em `main` | Nasce de `main`, volta para `main` **e** para `dev` |

> **Nota:** a branch padrão do repositório é `dev`, não `main` — o botão "Compare & pull request"
> do GitHub já sugere `dev` como base automaticamente. Mesmo assim, sempre confira a base antes de
> abrir o PR, principalmente ao abrir um PR de release (nesse caso a base É `main`, de propósito).

### Por que proteger `main` e `dev`?

Porque commit direto sem PR = sem revisão do outro integrante = maior risco de um dos dois não
conseguir explicar um trecho na banca. A regra de "sempre passar por PR" força os dois a lerem o
código/documento um do outro antes de considerar pronto — o mesmo princípio já registrado no
`GASTRA_STATUS.md` para a coluna "Em Revisão" do quadro de tarefas.

### Fluxo do dia a dia

```bash
git checkout dev
git pull origin dev
git checkout -b feature/nome-da-tarefa

# ...trabalham na tarefa, commitam...

git push origin feature/nome-da-tarefa
# abrir Pull Request no GitHub: feature/nome-da-tarefa -> dev
# pedir revisão do outro integrante
# só faz merge depois de aprovado
```

Depois do merge, a branch de feature é apagada automaticamente (auto-delete ativo no repositório).
Localmente, sincronizem antes de começar a próxima tarefa:

```bash
git checkout dev
git pull origin dev
git branch -d feature/nome-da-tarefa   # limpa a cópia local também
```

### Fluxo de release (checkpoints entregáveis)

Usado quando um bloco de trabalho fica estável o suficiente pra representar um ponto que poderia
ser mostrado ao orientador ou na banca — não precisa ser "o TG pronto", só "estável até aqui".

```bash
git checkout dev
git pull origin dev
git checkout -b release/<versao>

# ...só ajustes finais (docs, bugs pequenos), nada de feature nova...

git push origin release/<versao>
# abrir PR: release/<versao> -> main, revisão do Pedro, merge
```

Depois do merge na `main`, criem uma **tag** — é a tag, não a branch, que preserva o checkpoint
permanentemente:

```bash
git checkout main
git pull origin main
git tag -a v<versao> -m "Descrição curta do que essa versão contém"
git push origin v<versao>
```

Por fim, tragam o conteúdo de volta para `dev` (garante que qualquer ajuste feito durante a release
não fique só na `main`):

```bash
# abrir PR: release/<versao> -> dev, revisão do Pedro, merge
```

**Se a branch `release/<versao>` já tiver sido apagada** (pelo auto-delete, antes desse último
passo), não tem problema — o conteúdo já está preservado na `main` via o merge commit. Basta abrir
o PR `main -> dev` no lugar; o resultado final é o mesmo.

> ⚠️ **Cautela com `main` e o auto-delete:** com "Automatically delete head branches" ativo, toda
> branch de origem (`compare`) de um PR mergeado é apagada — inclusive `main`, se ela for a origem
> de algum PR (como no PR `main -> dev` acima). A documentação do GitHub garante que branches
> protegidas são poupadas dessa exclusão automática, mas há relatos de comportamento inconsistente
> especificamente com Rulesets. **Depois de qualquer PR onde `main` é a origem, confiram manualmente
> que ela continua existindo na lista de branches.** Se um dia sumir mesmo assim, ela pode ser
> recriada a partir de qualquer tag (`git checkout -b main v<ultima-tag>`) ou a partir da `dev`
> atualizada, já que o conteúdo nunca é perdido de fato.

## 2. Escopo do TG — status atual (revisado em 21/08/2026)

O `docs/GASTRA_STATUS.md` (seção 1) e a `docs/requisitos/GASTRA_Matriz_Rastreabilidade.docx`
mantêm o status de validação de cada item de escopo, com a legenda completa de status. Resumo do
que vale hoje:

- **Foco e entrega central do TG:** os quatro blocos analíticos do projeto de pesquisa formal — BI,
  Ciência de Dados, Programação Linear, LGPD.
- **Núcleo do módulo de comandas** (abrir/pedir/fechar) — **validado com o orientador em
  21/08/2026** como componente **adicional**: necessário para alimentar os blocos analíticos com
  dado real, mas não é o centro da entrega. Antes dessa data era tratado como "pendente de
  validação" — essa fase já passou.
- **Extras condicionados a sobrar tempo** (validados, mas não são compromisso de entrega): cardápio
  digital (consulta via QR/tablet), consulta da comanda em tempo real pelo cliente, e lista de
  pendências do garçom. Só entram em desenvolvimento depois que o núcleo (comandas + os 4 blocos)
  tiver um MVP apresentável.
- **Fora do escopo do TG:** integração com a cozinha (acesso a pedidos, confirmação de preparo).
  Não é pendência — é decisão de excluir, tratada como feature futura pós-defesa. Nenhuma issue
  deste ciclo do TG deve incluir esse item.

**Regra que continua valendo:** nenhum código ou texto commitado neste repositório deve tratar um
item como escopo mais avançado do que o status registrado no `GASTRA_STATUS.md` permite — nem em
comentário de código, nem em texto de TG, nem em nome de branch/PR. Antes de escrever algo que
dependa de um item de escopo, confiram o status atual em `docs/GASTRA_STATUS.md`, seção 1.

Isso deixa de valer automaticamente assim que um item mudar de status na Matriz de Rastreabilidade —
quem atualizar a matriz após nova validação com o orientador deve, no mesmo PR, atualizar qualquer
texto que já dependia disso.

## 3. Convenção de commits

Usamos [Conventional Commits](https://www.conventionalcommits.org/) — mensagens em português está
ok, o que importa é o prefixo, porque ele deixa o `git log` legível como histórico de decisões
(o próprio `GASTRA_STATUS.md` já sugere isso na nota do topo do arquivo).

```
<tipo>: <descrição curta no imperativo>

[corpo opcional explicando o porquê, não só o quê]
```

| Tipo | Quando usar |
|---|---|
| `feat` | Nova funcionalidade (ex.: `feat: implementa abertura de comanda`) |
| `fix` | Correção de bug |
| `docs` | Mudança em documentação (ex.: `docs: marca MER do módulo de comandas como concluído`) |
| `refactor` | Mudança de código sem alterar comportamento |
| `test` | Testes automatizados |
| `chore` | Tarefa de manutenção (setup, dependências, configuração) |
| `style` | Formatação, sem mudança de lógica |

Exemplos reais para o GASTRA:
- `feat: adiciona classificação híbrida de composição de mesa (RF02)`
- `docs: atualiza matriz de rastreabilidade após validação do orientador`
- `fix: corrige cálculo de taxa de serviço no fechamento de comanda`

## 4. Pull Requests e revisão

Antes de aprovar qualquer PR, siga o roteiro em [`docs/CHECKLIST_REVISAO_PR.md`](docs/CHECKLIST_REVISAO_PR.md)
— ele cobre pontos que passam batido numa leitura rápida (escopo pendente de validação, dados
sensíveis, consistência com RF/RNF/RN).

- Todo PR usa o template em `.github/PULL_REQUEST_TEMPLATE.md` (preenchido automaticamente ao abrir).
- Vincule o PR à Issue correspondente (`Closes #12`, por exemplo) — isso move o card automaticamente
  no GitHub Projects.
- **Sempre peça revisão do outro integrante da dupla antes do merge**, mesmo em documentação.
- Ao revisar, comentem o que foi ajustado e por quê — o objetivo é que os dois consigam defender
  qualquer trecho do repositório na banca, não só quem escreveu.

### Sub-issues

As issues criadas a partir do checklist do `GASTRA_STATUS.md` (seção 3) costumam ser genéricas —
elas funcionam como **issue guarda-chuva** de um bloco de trabalho (ex.: "Formalizar RF/RNF/RN do
módulo de comandas em tabela rastreável"), não como a unidade de execução real.

Conforme um item começa a ficar concreto, quebrem a issue-mãe em **sub-issues nativas do GitHub**
(não checkboxes soltos na descrição) — usando `gh issue create --parent <numero>` (requer `gh` CLI
≥ 2.94.0) ou pela interface do GitHub. Vantagens: cada sub-issue herda Status/Iteration
independentemente, o workflow "Auto-add sub-issues to project" já garante que toda sub-issue criada
entra automaticamente no board, e o progresso aparece como barra (X de Y concluídas) direto no card
da issue-mãe, sem atualização manual.

Exemplo:
- Issue-mãe: *"Formalizar RF/RNF/RN do módulo de comandas em tabela rastreável"*
  - Sub-issue: "Revisar RF05 pra refletir cardápio digital opção B"
  - Sub-issue: "Preencher coluna Origem/Fonte de cada RF na matriz"
  - Sub-issue: "Validar RN03 com orientador antes de fechar"

Scripts de criação em lote ficam em `scripts/` (ex.: `create_issues_from_checklist.sh` para o lote
inicial, `create_sprint_issues.sh` para sub-issues e issues novas de sprint). **Antes de rodar um
script de criação em lote, confira se ele já não foi rodado antes** — nenhum deles verifica
duplicidade sozinho; rodar duas vezes cria issues repetidas.

## 5. Quadro de tarefas (GitHub Projects)

Três conceitos diferentes, que se complementam:

| Conceito | O que representa | Escala de tempo |
|---|---|---|
| **Milestone** | Um entregável temático do TG (ex.: "Levantamento de Requisitos") | Semanas a meses — pode abranger várias sprints |
| **Iteration** (campo Sprint) | Um bloco de tempo fixo de trabalho | 1 semana (ajustado de 2 semanas em 19/08/2026) |
| **Status** (coluna do board) | O estado atual de uma issue específica | Muda dia a dia |

Uma issue pode pertencer à Milestone M1, estar na Iteration "Sprint 1", e ter Status "Em Andamento"
ao mesmo tempo — os três campos coexistem sem conflito.

O board tem 5 colunas de Status: Backlog → A Fazer → Em Andamento → Em Revisão → Concluído. Nem
toda transição é automática — importante saber qual é qual pra não ficar esperando um card se mover
sozinho quando ele não vai:

| Transição | Como acontece |
|---|---|
| Backlog → A Fazer | **Manual.** Vocês decidem no planejamento do sprint o que entra na Iteration atual. |
| A Fazer → Em Andamento | **Manual.** Movam o card (ou mudem o Status) ao começar a trabalhar de fato. |
| Em Andamento → Em Revisão | **Automático.** Dispara quando um PR é aberto vinculado à issue (workflow "Pull request linked to issue"). |
| Em Revisão → Concluído | **Automático.** Dispara no merge do PR (workflow "Pull request merged"), ou ao fechar a issue diretamente sem PR (workflow "Item closed") — útil para itens de documentação/decisão que não passam por código. |

O campo **Iteration** (sprint, 1 semana) também é manual — não existe workflow que atribua sprint
sozinho. No planejamento de cada sprint, atribuam manualmente a Iteration de cada item que entrar
em "A Fazer", e o Assignee (quem da dupla fica responsável) junto, no mesmo momento — evita que os
dois comecem a mesma tarefa sem perceber.

> **Nota (21/08/2026):** o cronograma oficial do TG ainda está por vir. Issues dos artefatos novos
> (Business Model Canvas, User Stories, UX/UI, arquitetura, testes, manual do usuário — ver
> `docs/GASTRA_STATUS.md`) já podem ser criadas no Backlog sem Milestone/Iteration atribuída; a
> distribuição final entre Milestones acontece assim que o cronograma chegar.

## 6. Segurança e integridade de dados

Como o GASTRA lida com dados de clientes e garçons (questionários, entrevistas, futuramente dados
reais de pedidos), alguns cuidados são obrigatórios, não opcionais:

- **Nunca commitar dados brutos de questionário, gravação/transcrição de entrevista, ou qualquer
  dado que identifique uma pessoa.** A pasta `data-science/data/raw/` está no `.gitignore`
  propositalmente — dados brutos ficam só localmente ou num storage separado (ex.: Google Drive
  restrito), nunca no Git. Isso vale também para roteiros de entrevista: o **roteiro** (perguntas,
  estrutura) pode ser versionado normalmente, mas gravação/transcrição literal da entrevista, não.
- **Apenas dados agregados/anonimizados** entram em `data-science/data/processed/` e podem ser
  versionados — com uma ressalva: amostras muito pequenas (ex.: n=2, caso do questionário de
  garçom) não devem virar dataset estruturado ali, porque um "agregado" de 2 respostas praticamente
  reidentifica a resposta individual. Nesses casos, tratar como texto narrativo em
  `docs/GASTRA_STATUS.md`, não como planilha/CSV.
- **Nunca commitar segredos**: strings de conexão de banco, chaves de API, senhas. Usem variáveis de
  ambiente (`.env`, já no `.gitignore`) e um `.env.example` sem valores reais para documentar quais
  variáveis existem.
- Isso conecta diretamente com RNF03/RNF04/RN04/RN05 já documentados em
  `docs/requisitos/GASTRA_Requisitos_RN.docx` — a prática no repositório deve refletir o que está
  escrito no TG sobre minimização de dados.
- Se um segredo for commitado por engano: **não é suficiente apagar em um commit novo** (ele continua
  no histórico). Nesse caso, avisem um ao outro imediatamente e reescrevam o histórico
  (`git filter-repo` ou similar) antes de dar push para o remoto compartilhado.

## 7. Onde cada coisa vai

| Tipo de conteúdo | Pasta |
|---|---|
| Código do backend (ASP.NET Core) | `backend/` |
| Código do frontend (Angular) | `frontend/` |
| Notebooks de exploração (Python) | `data-science/notebooks/` |
| Código de produção dos algoritmos (clusterização, PL, etc.) | `data-science/src/` |
| Dados anonimizados/tratados | `data-science/data/processed/` |
| Dados brutos (NUNCA commitar) | `data-science/data/raw/` (local, fora do Git) |
| Documento de status vivo | `docs/GASTRA_STATUS.md` |
| Projeto de pesquisa formal | `docs/pesquisa/` |
| Material institucional de apoio (guia de orientação, modelo de estrutura da FATEC) | `docs/pesquisa/referencias/` |
| Business Model Canvas | `docs/negocio/` |
| RF/RNF/RN, matriz de rastreabilidade e User Stories | `docs/requisitos/` |
| Roteiros de entrevista (nunca gravação/transcrição bruta) | `docs/requisitos/entrevistas/` |
| MER (linguagem natural) e DER (formal) | `docs/modelagem/mer/` e `docs/modelagem/der/` |
| Definição de arquitetura do sistema | `docs/arquitetura/` |
| Wireframes e protótipo navegável (UX/UI) | `docs/ux-ui/` |
| Cenários de teste executados | `docs/testes/` |
| Manual de uso do usuário | `docs/manual-usuario/` |
| Diagramas de apoio (fluxos, casos de uso) | `docs/diagramas/` |
| Logo, identidade visual | `docs/assets/logo/` |
| Scripts de apoio (criação de issues em lote, etc.) | `scripts/` |

## 8. Licenciamento

Este repositório usa licença MIT (ver `LICENSE`), escolhida como **padrão sugerido** por ser
permissiva, simples e comum em projetos acadêmicos abertos — não é exigência da ABNT nem da FATEC.
É uma decisão de projeto em aberto. Antes da banca, confirmem:

1. **Regras institucionais da FATEC/orientador sobre propriedade intelectual de TG.** Algumas
   instituições preferem "todos os direitos reservados" até a defesa, especialmente se houver
   intenção de publicação futura ou uso comercial do GASTRA. Se for esse o caso, substituam a
   licença por "Todos os direitos reservados" (sem licença aberta) ou por uma licença mais
   restritiva (ex.: CC BY-NC-ND para a documentação).
2. **Autoria dupla.** Os dois nomes devem constar no copyright do `LICENSE` — já ajustado. Se o
   repositório for movido para uma conta/organização única, mantenham os dois nomes mesmo assim.
