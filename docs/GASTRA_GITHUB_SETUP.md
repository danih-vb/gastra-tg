# Setup do GitHub — passo a passo

Este guia cobre a parte que **não dá para deixar pronta dentro do .zip**: criar o repositório remoto,
convidar o Pedro, proteger branches, criar labels/milestones e montar o quadro no GitHub Projects.
Tudo aqui usa a interface do GitHub (ou o `gh` CLI, indicado como atalho onde fizer sentido) — precisa
da conta de vocês, então não é algo que eu (Claude) consigo fazer por vocês diretamente.

> Pré-requisito: já ter o `.zip` deste repositório extraído localmente, com `git init` e o primeiro
> commit já feitos (isso já vem pronto no pacote — ver instruções no final do README raiz do zip).

## 1. Criar o repositório remoto

1. No GitHub, criar um repositório **vazio** (sem README/gitignore/license — já temos os nossos):
   nome sugerido `gastra-tg`.
2. **Visibilidade: recomendo Privado** até a defesa. Dois motivos: (a) o repositório vai conter, ainda
   que em pastas separadas do código, referências e trechos de dados de questionários — mesmo
   anonimizados, mais seguro manter fechado; (b) o TG ainda não foi defendido/publicado formalmente.
   Depois da banca, se quiserem, é só mudar para público em Settings > General > Danger Zone.
3. Adicionar o Pedro como colaborador: **Settings > Collaborators and teams > Add people** — ele
   recebe um convite por e-mail/notificação e precisa aceitar.

## 2. Conectar o repositório local e enviar

```bash
cd gastra
git remote add origin <URL_DO_REPOSITORIO>
git push -u origin main
git push -u origin dev
```

## 3. Proteger as branches `main` e `dev`

**Settings > Branches > Add branch protection rule**, repetir para `main` e para `dev`:

- Branch name pattern: `main` (depois repetir para `dev`)
- ✅ Require a pull request before merging
- ✅ Require approvals — mínimo 1 (o outro integrante da dupla)
- ✅ Do not allow bypassing the above settings (nem os admins pulam a regra)
- ❌ Allow force pushes — deixar desmarcado
- ❌ Allow deletions — deixar desmarcado

Isso garante, na prática, a regra do GitFlow descrita em `CONTRIBUTING.md`: ninguém commita direto
em `main`/`dev`.

## 4. Labels

**Issues > Labels > New label.** Criar estas (nome / cor sugerida):

| Label | Cor sugerida |
|---|---|
| `bloco:bi` | `#1f77b4` |
| `bloco:clusterizacao` | `#2ca02c` |
| `bloco:pl` | `#9467bd` |
| `bloco:lgpd` | `#d62728` |
| `bloco:comandas` | `#ff7f0e` |
| `bloco:cardapio-digital` | `#e377c2` |
| `tipo:documentacao` | `#8c8c8c` |
| `tipo:modelagem` | `#17becf` |
| `tipo:codigo` | `#393939` |
| `prioridade:alta` | `#b60205` |
| `prioridade:media` | `#fbca04` |
| `prioridade:baixa` | `#0e8a16` |

**Atalho com `gh` CLI** (se instalado e autenticado — `gh auth login`), rodar de dentro do repositório
já conectado ao remoto:

```bash
bash scripts/create_labels.sh
```

(o script já está em `scripts/create_labels.sh`, incluído no pacote)

## 5. Milestones

**Issues > Milestones > New milestone.** Sugestão de marcos, alinhados aos blocos do TG — ajustem as
datas para o calendário real de entregas/banca de vocês, os nomes eu já conectei ao que está em
`docs/GASTRA_STATUS.md`:

| Milestone | Cobre | Sugestão de ordem |
|---|---|---|
| `M1 - Levantamento de Requisitos` | Questionários, entrevista Cocobambu, RF/RNF/RN | 1 |
| `M2 - Validação de Escopo` | Validação formal com orientador (comandas + cardápio) | 2 |
| `M3 - Modelagem` | MER, DER, banco de dados, diagramas de apoio | 3 |
| `M4 - Módulo de Comandas (MVP)` | Backend + frontend do núcleo de comandas | 4 |
| `M5 - BI` | Relatórios agregados | 5 |
| `M6 - Ciência de Dados` | Clusterização + regras de associação | 6 |
| `M7 - Programação Linear` | Alocação de garçons | 7 |
| `M8 - LGPD` | Conformidade aplicada em todo o sistema | 8 |
| `M9 - Redação Final (ABNT)` | Revisão e formatação final do texto do TG | 9 |
| `M10 - Defesa` | Preparação e apresentação da banca | 10 |

> Datas: preencham vocês — eu não tenho o calendário oficial da FATEC nem a data da banca.

## 6. GitHub Projects (quadro Kanban)

1. Na página do repositório: **Projects > New project > Board**.
2. Nome: `GASTRA — TG`.
3. Colunas (recriar exatamente as já definidas em `docs/GASTRA_STATUS.md`, seção 4):
   1. **Backlog**
   2. **A Fazer (Sprint atual)**
   3. **Em Andamento**
   4. **Em Revisão**
   5. **Concluído**
4. **Workflows automáticos** (ícone de raio no canto do board, "Workflows"):
   - Quando um item é adicionado → vai para **Backlog**
   - Quando um PR é aberto vinculado à issue → move para **Em Revisão**
   - Quando o PR é mergeado / issue fechada → move para **Concluído**
5. Vincule o Project ao repositório (**Settings do Project > Manage access**, adicionar o repo e o
   Pedro como colaborador do board também, se o Project for de organização/conta separada).

### Sprints

GitHub Projects (novo, "Projects v2") suporta um campo de **Iteration** — cria automaticamente uma
sequência de sprints com data de início/fim:

1. No board, **+ New field > Iteration**.
2. Definir duração (sugestão: **2 semanas**, ajustável conforme o ritmo real da dupla).
3. Cada issue entra numa iteração; o board pode ser filtrado/agrupado por "Iteration" para ver só a
   sprint atual.

## 7. Popular o board com as issues do checklist

O `docs/GASTRA_STATUS.md` já tem, na seção 3, o checklist completo em Markdown. Duas formas de
transformar isso em issues:

**Opção A — manual:** copiar cada item do checklist, criar uma issue com o template "Tarefa do TG"
(`.github/ISSUE_TEMPLATE/tarefa-tg.md`), aplicar a label do bloco correspondente.

**Opção B — em lote com `gh` CLI:** editar `scripts/create_issues_from_checklist.sh` (já no pacote)
com os itens desejados e rodar. Ele cria as issues e já aplica a label do bloco.

```bash
bash scripts/create_issues_from_checklist.sh
```

Issues criadas em um repositório com Project vinculado entram automaticamente no board, na coluna
Backlog (pelo workflow configurado no passo 6).

## 8. Checklist final desta configuração

- [ ] Repositório criado (privado) e Pedro convidado como colaborador
- [ ] `main` e `dev` protegidas (PR obrigatório + 1 aprovação)
- [ ] Labels criadas
- [ ] Milestones criados com datas reais preenchidas
- [ ] Project criado com as 5 colunas e workflows automáticos
- [ ] Campo de Iteration configurado (sprints de 2 semanas ou outro período definido pela dupla)
- [ ] Issues do checklist do `GASTRA_STATUS.md` criadas e no board
- [ ] Link do Project adicionado ao `README.md` (seção "Documentação")
