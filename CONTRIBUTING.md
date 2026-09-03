# Como contribuir com o GASTRA

Este documento é o guia de referência para trabalharmos no mesmo repositório sem pisar no
trabalho um do outro, e para manter o histórico do Git útil na hora de explicar decisões na
banca. Leia isto antes do primeiro commit.

## 1. Modelo de branches — GitFlow

Usamos GitFlow simplificado (a versão completa tem branches `support/*` que não fazem sentido
aqui).

| Branch                  | Papel                                                                 | Regras                                                                                   |
|--------------------------|------------------------------------------------------------------------|--------------------------------------------------------------------------------------------|
| `main`                   | Sempre reflete a versão estável/entregável (ex.: o que foi apresentado numa entrega parcial ou na banca) | **Protegida.** Nunca commitar direto. Só recebe merge de `release/*` ou `hotfix/*` via Pull Request. |
| `dev`                    | Branch de integração — onde as features se juntam. **Branch padrão do repositório** | **Protegida.** Só recebe merge de `feature/*` via Pull Request.                             |
| `feature/<nome-curto>`   | Uma funcionalidade ou bloco específico                                | Nasce de `dev`, volta para `dev`. Ex.: `feature/comandas`, `feature/cardapio-digital`, `feature/clusterizacao`, `feature/programacao-linear`, `feature/lgpd`, `feature/mer-der` |
| `release/<versao>`       | Preparação de uma entrega (ex.: entrega parcial do TG)                 | Nasce de `dev`, só ajustes finais (docs, bugs pequenos), depois vai para `main` **e** volta para `dev` |
| `hotfix/<descricao>`     | Correção urgente em algo já em `main`                                  | Nasce de `main`, volta para `main` **e** para `dev`                                        |

> **Nota:** a branch padrão do repositório é `dev`, não `main` — o botão "Compare & pull
> request" do GitHub já sugere `dev` como base automaticamente. Mesmo assim, sempre confira a
> base antes de abrir o PR, principalmente ao abrir um PR de release (nesse caso a base É
> `main`, de propósito).

### Fluxo do dia a dia (feature)

```bash
git checkout dev
git pull origin dev
git checkout -b feature/nome-da-tarefa

# ...trabalhe na tarefa, commite...

git push origin feature/nome-da-tarefa
# abra Pull Request no GitHub: feature/nome-da-tarefa -> dev
# peça revisão do outro integrante
# só faz merge depois de aprovado
```

Depois do merge, a branch de feature é apagada automaticamente (auto-delete ativo no
repositório). Localmente, sincronize antes de começar a próxima tarefa:

```bash
git checkout dev
git pull origin dev
git branch -d feature/nome-da-tarefa   # limpe a cópia local também
```

### Fluxo de release (checkpoints entregáveis)

Usado quando um bloco de trabalho fica estável o suficiente pra representar um ponto que
poderia ser mostrado ao orientador ou na banca.

```bash
git checkout dev
git pull origin dev
git checkout -b release/<versao>

# ...só ajustes finais (docs, bugs pequenos), nada de feature nova...

git push origin release/<versao>
# abra PR: release/<versao> -> main, revisão do outro integrante, merge
```

Depois do merge na `main`, crie uma **tag** — é a tag, não a branch, que preserva o checkpoint
permanentemente:

```bash
git checkout main
git pull origin main
git tag -a v<versao> -m "Descrição curta do que essa versão contém"
git push origin v<versao>
```

Por fim, traga o conteúdo de volta para `dev` (garante que qualquer ajuste feito durante a
release não fique só na `main`):

```bash
# abrir PR: release/<versao> -> dev, revisão do outro integrante, merge
```

**Se a branch `release/<versao>` já tiver sido apagada** (pelo auto-delete, antes desse último
passo), não tem problema — o conteúdo já está preservado na `main` via o merge commit. Basta
abrir o PR `main -> dev` no lugar; o resultado final é o mesmo.

> ⚠️ **Cautela com `main` e o auto-delete:** com "Automatically delete head branches" ativo,
> toda branch de origem (`compare`) de um PR mergeado é apagada — inclusive `main`, se ela for
> a origem de algum PR (como no PR `main -> dev` acima). A documentação do GitHub garante que
> branches protegidas são poupadas dessa exclusão automática, mas há relatos de comportamento
> inconsistente especificamente com Rulesets. **Depois de qualquer PR onde `main` é a origem,
> confiram manualmente que ela continua existindo na lista de branches.** Se um dia sumir mesmo
> assim, ela pode ser recriada a partir de qualquer tag (`git checkout -b main v<ultima-tag>`)
> ou a partir da `dev` atualizada, já que o conteúdo nunca é perdido de fato.

## 2. Convenção de commits

Usamos [Conventional Commits](https://www.conventionalcommits.org/) — mensagens em português
está ok, o que importa é o prefixo, porque ele deixa o `git log` legível como histórico de
decisões.

```
<tipo>: <descrição curta no imperativo>

[corpo opcional explicando o porquê, não só o quê]
```

| Tipo       | Quando usar                                                     |
|------------|-------------------------------------------------------------------|
| `feat`     | Nova funcionalidade                                                |
| `fix`      | Correção de bug                                                    |
| `docs`     | Mudança em documentação                                            |
| `refactor` | Mudança de código sem alterar comportamento                        |
| `test`     | Testes automatizados                                               |
| `chore`    | Tarefa de manutenção (setup, dependências, configuração)           |
| `style`    | Formatação, sem mudança de lógica                                  |

Exemplos:
- `feat: implementa abertura de comanda`
- `fix: corrige cálculo de taxa de serviço no fechamento de comanda`
- `docs: atualiza matriz de rastreabilidade após validação do orientador`
- `refactor: extrai serviço de alocação de garçons`

## 3. Pull Requests e revisão

- Todo PR usa o template em `.github/PULL_REQUEST_TEMPLATE.md` (preenchido automaticamente ao
  abrir).
- Vincule o PR à Issue correspondente (`Closes #12`, por exemplo) — isso move o card
  automaticamente no GitHub Projects.
- **Sempre peça revisão do outro integrante da dupla antes do merge**, mesmo em documentação.
- Ao revisar, comentem o que foi ajustado e por quê — o objetivo é que os dois consigam
  defender qualquer trecho do repositório na banca, não só quem escreveu.
- Sem conflitos de merge pendentes antes de pedir aprovação.

## 4. Segurança de dados e LGPD

Como o GASTRA lida com dados de clientes e garçons (questionários, entrevistas, futuramente
dados reais de pedidos), alguns cuidados são obrigatórios, não opcionais:

- **Nunca commitar dados brutos de questionário, gravação/transcrição de entrevista, ou
  qualquer dado que identifique uma pessoa.** A pasta `data-science/data/raw/` está no
  `.gitignore` propositalmente — dados brutos ficam só localmente ou num storage separado (ex.:
  Google Drive restrito), nunca no Git. Isso vale também para roteiros de entrevista: o
  **roteiro** (perguntas, estrutura) pode ser versionado normalmente, mas
  gravação/transcrição literal da entrevista, não.
- **Apenas dados agregados/anonimizados e já interpretados** entram em
  `data-science/data/processed/` e podem ser versionados. Ressalva importante: uma amostra
  muito pequena (ex.: n=2) nunca é apresentada como recorte estruturado por resposta ou
  gráfico — só como síntese narrativa, porque um "agregado" de poucas respostas praticamente
  reidentifica a resposta individual.
- **Nunca commitar segredos**: strings de conexão de banco, chaves de API, senhas. Usem
  variáveis de ambiente (`.env`, já no `.gitignore`) e um `.env.example` sem valores reais para
  documentar quais variáveis existem.
- Essa prática no repositório deve refletir o que está escrito no TG sobre minimização de
  dados (RNF03/RNF04/RN04/RN05 em `docs/requisitos/GASTRA_Requisitos_RN.docx`).
- Se um segredo for commitado por engano: **não é suficiente apagar em um commit novo** (ele
  continua no histórico). Nesse caso, avisem um ao outro imediatamente e reescrevam o
  histórico (`git filter-repo` ou similar) antes de dar push para o remoto compartilhado.

## 5. Licenciamento

Este repositório usa licença MIT (ver `LICENSE`).
