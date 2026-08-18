# Como contribuir com o GASTRA

Este documento é o guia de referência para Daniel e Pedro trabalharem no mesmo repositório sem
pisar no trabalho um do outro, e para manter o histórico do Git útil na hora de explicar decisões
na banca. Leiam isto antes do primeiro commit.

## 1. Modelo de branches — GitFlow

Usamos GitFlow simplificado (a versão completa tem branches `support/*` que não fazem sentido aqui).

| Branch | Papel | Regras |
|---|---|---|
| `main` | Sempre reflete a versão estável/entregável (ex.: o que foi apresentado numa entrega parcial ou na banca) | **Protegida.** Nunca commitar direto. Só recebe merge de `release/*` ou `hotfix/*` via Pull Request. |
| `dev` | Branch de integração — onde as features se juntam | **Protegida.** Só recebe merge de `feature/*` via Pull Request. |
| `feature/<nome-curto>` | Uma funcionalidade ou bloco específico | Nasce de `dev`, volta para `dev`. Ex.: `feature/comandas`, `feature/cardapio-digital`, `feature/clusterizacao`, `feature/programacao-linear`, `feature/lgpd`, `feature/mer-der` |
| `release/<versao>` | Preparação de uma entrega (ex.: entrega parcial do TG) | Nasce de `dev`, só ajustes finais (docs, bugs pequenos), depois vai para `main` **e** volta para `dev` |
| `hotfix/<descricao>` | Correção urgente em algo já em `main` | Nasce de `main`, volta para `main` **e** para `dev` |

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

## 2. Escopo do TG — itens pendentes de validação

O `docs/GASTRA_STATUS.md` (seção 1) e a `docs/requisitos/GASTRA_Matriz_Rastreabilidade.docx`
mantêm o status de validação de cada item de escopo. Alguns itens — hoje, o módulo de comandas e o
cardápio digital (opção B) — estão marcados como **"Pendente de validação"**: foram decididos entre
a dupla, mas ainda não aprovados formalmente pelo Prof. Ferrucci, e colidem com a delimitação do
projeto de pesquisa formal (`docs/pesquisa/Gastra.pdf`), que exclui explicitamente sistemas de
pedido e cardápios digitais.

**Regra:** nenhum código ou texto commitado neste repositório deve tratar um item marcado
"Pendente de validação" como escopo fechado — nem em comentário de código, nem em texto de TG, nem
em nome de branch/PR que pressuponha aprovação. Antes de escrever algo que dependa desses itens,
confira o status atual em `docs/GASTRA_STATUS.md`, seção 1.

Isso deixa de valer automaticamente assim que o item mudar de status para "Validado (escopo
original)" ou equivalente na Matriz de Rastreabilidade — quem atualizar a matriz após validação com
o orientador deve, no mesmo PR, atualizar qualquer texto que já dependia disso.

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
(não checkboxes soltos na descrição). Vantagens: cada sub-issue herda Status/Iteration
independentemente, o workflow "Auto-add sub-issues to project" já garante que toda sub-issue criada
entra automaticamente no board, e o progresso aparece como barra (X de Y concluídas) direto no card
da issue-mãe, sem atualização manual.

Exemplo:
- Issue-mãe: *"Formalizar RF/RNF/RN do módulo de comandas em tabela rastreável"*
  - Sub-issue: "Revisar RF05 pra refletir cardápio digital opção B"
  - Sub-issue: "Preencher coluna Origem/Fonte de cada RF na matriz"
  - Sub-issue: "Validar RN03 com orientador antes de fechar"

## 5. Quadro de tarefas (GitHub Projects)

O board tem 5 colunas de Status: Backlog → A Fazer → Em Andamento → Em Revisão → Concluído. Nem
toda transição é automática — importante saber qual é qual pra não ficar esperando um card se mover
sozinho quando ele não vai:

| Transição | Como acontece |
|---|---|
| Backlog → A Fazer | **Manual.** Vocês decidem no planejamento do sprint o que entra na Iteration atual. |
| A Fazer → Em Andamento | **Manual.** Movam o card (ou mudem o Status) ao começar a trabalhar de fato. |
| Em Andamento → Em Revisão | **Automático.** Dispara quando um PR é aberto vinculado à issue (workflow "Pull request linked to issue"). |
| Em Revisão → Concluído | **Automático.** Dispara no merge do PR (workflow "Pull request merged"), ou ao fechar a issue diretamente sem PR (workflow "Item closed") — útil para itens de documentação/decisão que não passam por código. |

O campo **Iteration** (sprint) também é manual — não existe workflow que atribua sprint sozinho.
No planejamento de cada sprint (sugestão: ciclos de 2 semanas), atribuam manualmente o Iteration de
cada item que entrar em "A Fazer".

## 6. Segurança e integridade de dados

Como o GASTRA lida com dados de clientes e garçons (questionários, futuramente dados reais de
pedidos), alguns cuidados são obrigatórios, não opcionais:

- **Nunca commitar dados brutos de questionário ou qualquer dado que identifique uma pessoa.**
  A pasta `data-science/data/raw/` está no `.gitignore` propositalmente — dados brutos ficam só
  localmente ou num storage separado (ex.: Google Drive restrito), nunca no Git.
- **Apenas dados agregados/anonimizados** entram em `data-science/data/processed/` e podem ser
  versionados.
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
| RF/RNF/RN e matriz de rastreabilidade | `docs/requisitos/` |
| MER (linguagem natural) e DER (formal) | `docs/modelagem/mer/` e `docs/modelagem/der/` |
| Diagramas de apoio (fluxos, casos de uso) | `docs/diagramas/` |
| Logo, identidade visual | `docs/assets/logo/` |

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