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

## 2. Convenção de commits

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

## 3. Pull Requests e revisão

- Todo PR usa o template em `.github/PULL_REQUEST_TEMPLATE.md` (preenchido automaticamente ao abrir).
- Vincule o PR à Issue correspondente (`Closes #12`, por exemplo) — isso move o card automaticamente
  no GitHub Projects.
- **Sempre peça revisão do outro integrante da dupla antes do merge**, mesmo em documentação.
- Ao revisar, comentem o que foi ajustado e por quê — o objetivo é que os dois consigam defender
  qualquer trecho do repositório na banca, não só quem escreveu.

## 4. Segurança e integridade de dados

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

## 5. Onde cada coisa vai

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
