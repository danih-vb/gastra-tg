# GASTRA — Revisão de modelagem: consistência entre desenho, código e banco

Registra o critério da revisão, a matriz do que cada artefato precisa bater e o que a conferência
encontrou. Issues #190, #196 e #197.

## 1. Regra de desempate

Quando dois artefatos divergem, **um deles está errado** — e a correção nunca é silenciosa. A ordem de
autoridade adotada:

1. **O requisito manda**, quando a divergência é sobre *o que o sistema deve fazer*. Se o código faz algo
   que requisito nenhum pediu, ou o requisito passa a descrever, ou o código sai.
2. **O código manda**, quando a divergência é sobre *como está construído* — nome de classe, camada,
   coluna. Desenho que discorda do código é desenho desatualizado.
3. **O banco manda** sobre o modelo físico, e o **MER** manda sobre o conceitual. Diferença entre os dois
   é legítima quando tem justificativa escrita (chave estrangeira, atributo derivado, multivalorado).

Toda divergência encontrada vira linha na seção 4, com a decisão tomada.

## 2. Checklist por tipo de diagrama

| Tipo | O que se confere |
|---|---|
| Casos de uso | Cada caso tem ator; o identificador (UCxx) bate com o documento de casos de uso; `<<include>>` e `<<extend>>` usados pelo significado certo, e não como "depois disso"; fronteira do sistema desenhada |
| Classes | Nome, atributo e método existem no código com a mesma grafia; visibilidade correta; constante aparece como estática; navegabilidade e multiplicidade coerentes com o EF |
| Sequência | Participantes existem como classe ou serviço; mensagem síncrona × assíncrona; retorno desenhado; fragmento `alt`/`opt` onde há decisão |
| Atividade | Início e fim; decisão com guarda em toda saída; raia por ator quando há mais de um |
| Implantação | Nó, artefato e protocolo entre eles; bate com o `docker-compose.yml` |
| MER / DER | Entidade × tabela, atributo × coluna, cardinalidade × chave estrangeira |

## 3. Matriz de consistência

| Diagrama | Precisa bater com | Situação |
|---|---|---|
| 6 de casos de uso | UCxx do `GASTRA_Casos_de_Uso.docx`, atores, RF | ✅ inventário conferido, ver 4.7 |
| 4 de classes | Nomes do código (`Gastra.Domain`, `Gastra.Infrastructure`) | ✅ corrigidos, ver 4.3 e 4.9 |
| 3 de sequência | Classes e endpoints existentes | ✅ corrigido, ver 4.10 |
| 2 de atividade | RN01–RN08 e o fluxo do código | ✅ corrigidos, ver 4.11 |
| Implantação | `infra/docker-compose.yml` | ✅ corrigido, ver 4.2 |
| MER e DER | Banco depois das migrations | ✅ conferido, ver 4.1 |

## 4. O que a conferência encontrou

### 4.1 O banco tem uma tabela que o modelo conceitual não tem (#196)

O catálogo do banco foi extraído depois das últimas migrations e comparado com o DER conceitual. As 11
entidades e as 2 tabelas associativas que a comparação anterior já cobria continuam batendo, com as mesmas
justificativas de sempre (chave estrangeira só no físico, atributo derivado, multivalorado que virou
tabela).

**A divergência nova é uma só:**

| Tabela no banco | No MER/DER | Decisão |
|---|---|---|
| `avaliacao_atendimento` | **ausente** | Entrar no MER e no DER como entidade fraca de `Comanda`, cardinalidade (0,1) |

A tabela nasceu com o RF25 (avaliação do atendimento pelo cliente) e é entidade de verdade: tem
identidade, atributos próprios (`nota`, `comentario`, `data_hora_envio`) e vive presa a uma comanda, uma
por comanda. **Essa correção precisa do brModelo**, que é aplicação gráfica — está registrada como tarefa
manual na seção 5.

### 4.2 O diagrama de implantação (#195)

**Correção de uma afirmação anterior deste documento:** escrevi que o diagrama mostrava três nós e que
faltava o `web`. Estava errado — os quatro nós sempre estiveram lá. O que havia era outra coisa, e duas:

1. **`web`, `api` e `analitica` estavam marcados como "planejado"** (borda tracejada, com a legenda
   dizendo "Dockerfile ainda não criado"). Os três Dockerfiles existem e os quatro contêineres rodam.
   Marcação removida e legenda reescrita. A seta da leitura somente-leitura das views também estava
   tracejada, e a D10 foi implementada na #121.
2. **O desenho contradizia a D12.** O navegador aparecia falando **direto com a `api`**, o que era verdade
   quando as origens eram diferentes. Com o proxy reverso, quem fala com a API é o nginx: agora o
   navegador chega no `web`, e do `web` sai a seta "proxy reverso (D12)" para a `api`.

O segundo ponto só apareceu **depois** de corrigir o primeiro: com os nós deixando de ser "planejado", a
topologia errada ficou visível. Serve de argumento para revisar diagrama olhando o desenho, e não só a
lista de elementos.

### 4.3 `PercentualTaxaServico` é constante, não coluna (#196)

A issue perguntava se é constante ou coluna, porque aparece na classe `Comanda` gerada do código e não
aparece no MER. **É constante de domínio:**

```csharp
public const decimal PercentualTaxaServico = 0.10m;
```

Os dois artefatos estão certos, cada um no seu papel: o MER não a tem porque ela **não é persistida** — o
que o banco guarda é `comanda.taxa_servico_removida`, um booleano, e o valor da taxa é recalculado. O
diagrama de classes a mostra porque ela faz parte da regra (RF04).

**O que precisava mudar era a notação:** em UML, membro estático se representa **sublinhado**. A correção foi
feita no gerador (`backend/tools/GeradorDiagramaClasses`), e por isso vale para todo membro estático, não só
para este: são 18 entre constantes, campos e métodos. No Markdown, onde o texto fica dentro de crase e não
aceita formatação, eles saem com «static» por extenso.

De quebra, apareceu um segundo defeito justamente nesta constante: o diagrama a mostrava **sem o valor**.
`const decimal` não é literal para a reflection — o compilador o transforma em `static readonly` com
`[DecimalConstant]` —, e o gerador só lia o valor de literais. Agora aparece
`PercentualTaxaServico: decimal = 0.10`, e `PrecoMinimo = 0.01` pelo mesmo motivo.

A regeração também trouxe para o diagrama a `AvaliacaoAtendimento` e os métodos novos da `Comanda`, que ele
ainda não tinha.

### 4.4 O código faz coisas que o requisito não descrevia (#197)

Desde a #141 o sistema **redefine senha** e **reinicia a verificação em duas etapas** de outra conta, com
endpoint, tela e teste. O RF18 falava só em cadastrar, editar e inativar.

**Decisão, pela regra 1:** o requisito passa a descrever o que existe. O RF18 foi ampliado:

> O sistema deve permitir ao Gerente cadastrar, editar, inativar (*soft delete*) e reativar contas de
> usuário, atribuindo o papel correspondente, além de redefinir a senha e reiniciar a verificação em duas
> etapas de outra conta.

Ampliar o RF18 foi preferido a criar um requisito novo porque é a mesma capacidade — gestão de contas pelo
Gerente — e criar RF26 espalharia UC04 em dois lugares. **O UC04 e a Matriz de Rastreabilidade precisam
acompanhar**, e isso é trabalho no `.docx`: seção 5.

### 4.5 O nome do módulo do frontend não batia (#197)

A arquitetura listava os módulos como `autenticacao`, `cardapio`, `comandas`, `alocacao`, `analises`,
`cliente`. O repositório tem `acesso`, `alocacao`, `analises`, `cardapio`, `cliente`, `comandas`, `salao` e
`usuarios` — um nome diferente e dois módulos que nem existiam quando o documento foi escrito. Corrigido
pela regra 2, com o código mandando.

### 4.6 Onze marcações de "planejado" estavam vencidas (#197)

A arquitetura ainda marcava como 🔜 coisas entregues há semanas: as telas dos quatro perfis, a organização
em módulos, a comunicação com a API, o log e auditoria, a restrição alimentar, os testes de frontend e os
testes dos algoritmos em Python. Todas corrigidas para ✅.

Sobraram três 🔜, e esses são honestos: a avaliação de Nielsen, a análise estática no SonarCloud e a
coluna "Demonstração / banca".

A linha de **Prototipagem** também estava errada de outro jeito: dizia "Figma, antes da implementação". O
protótipo foi feito em HTML navegável e depois importado no Figma pelo plugin — o documento agora descreve
o que aconteceu.

### 4.7 O código citava uma UC25 que não existia (#191)

Os identificadores dos seis diagramas foram extraídos das fontes `.drawio` e cruzados com o
`GASTRA_Casos_de_Uso.docx`. As 24 UCs do documento aparecem nos diagramas, sem sobra nem falta:

| Diagrama | Casos de uso |
|---|---|
| Autenticação | UC01–UC04 |
| Gestão de cardápio | UC05–UC09 |
| Núcleo de comandas | UC10–UC14, UC23 |
| Blocos analíticos | UC15–UC18, UC21, UC22 |
| Cliente | UC19, UC20 |
| Configuração do salão | UC24 |

**Mas o código cita uma UC25 em três lugares** — no controller, no caso de uso e no serviço do
frontend —, e ela não existia nem no documento nem em diagrama nenhum. É a avaliação do atendimento,
que entrou com o RF25: o código referenciou um caso de uso que ninguém tinha escrito.

Corrigido pela regra 1: a **UC25 — Avaliar o Atendimento** (ator: Cliente) entrou no documento de casos
de uso e no diagrama do Cliente, com a nota das restrições da RN08. A **UC04** também foi ampliada junto
com o RF18, para continuar descrevendo a mesma coisa que ele.

### 4.8 A UC11 também é iniciada pelo Garçom (#191)

No diagrama do núcleo de comandas, a UC11 (confirmar/ajustar composição da mesa) aparece **só** como
`<<include>>` da UC10 (abrir comanda), sem associação com o Garçom.

Isso descreve bem a abertura: os dois toques do RNF02 confirmam a composição sugerida. Mas o sistema
também permite **ajustar a composição depois**, com a comanda já aberta (`PATCH /api/comandas/{id}/composicao`,
usado na tela da comanda). Nesse caminho a UC11 é iniciada diretamente pelo Garçom, e o diagrama não mostra
isso.

**Decisão:** manter o `<<include>>` e **acrescentar** a associação do Garçom com a UC11. As duas leituras
são verdadeiras e o diagrama agora mostra as duas: na abertura a UC11 vem incluída na UC10; depois, o Garçom a
inicia sozinho. A linha nova sai do topo do ator e passa por cima dos outros casos de uso, para não cruzar
o `<<include>>`.

### 4.9 O diagrama da fatia vertical tinha quatro divergências (#192)

O `GASTRA_Classe_AbrirComanda` é desenhado à mão — mostra um caso de uso atravessando as camadas —, e por
isso não se atualiza sozinho. Os nomes dele foram cruzados com o repositório e com o `AbrirComandaUseCase`:

| No diagrama | No código | Decisão |
|---|---|---|
| `ComandasController` | `ComandaController` | Renomeado (regra 2) |
| `«interface» IGeradorCodigoAcesso` | **Não existe.** O código de acesso é gerado no construtor da `Comanda`, com `Guid` | Removido, com nota apontando para a #217, onde a forma do código está em discussão |
| *(ausente)* | `IRegistradorAuditoria` — o caso de uso registra `COMANDA_ABERTA` | Acrescentado, na cor da Application, que é onde a interface mora |
| `ComandaResponse.ComposicaoSugerida` | `Composicao` | Renomeado |

O construtor da `Comanda` também passou a aparecer, porque é nele que o código de acesso nasce hoje.

O `GASTRA_Classe_Pacotes`, o outro desenhado à mão, não tinha divergência: todo nome dele existe no
código.

### 4.10 Uma rota errada no diagrama de sequência (#193)

As 11 rotas citadas nos três diagramas de sequência foram cruzadas com os atributos `[Http*]` dos
controllers e com as rotas do serviço Python. Dez batem. **Uma não:**

| No diagrama | No código |
|---|---|
| `PATCH /api/itens/{id}/situacao` | `PATCH /api/comandas/{id}/itens/{itemId}/situacao` |

O item do pedido não tem rota própria: ele vive dentro da comanda, e a rota diz isso. Corrigido, com o
rótulo quebrado em duas linhas para não atravessar a linha de vida do controller. Os nomes de classe e
método dos três diagramas existem todos no código.

### 4.11 Os diagramas de atividade (#194)

**Núcleo de comandas — o preço.** A ação dizia "copiar o preço atual (RF03)". O código congela o
**preço promocional quando há promoção vigente** (RF03 + RF22, em `RegistrarItemPedidoUseCase`). O rótulo agora
diz isso.

**Alocação — uma decisão que o desenho não tinha.** O backend recusa gerar a sugestão quando há mais
garçons presentes do que vagas nas praças (`AlocacaoSemVagas`), e faz essa checagem **antes** de chamar o
Python — logo depois de ler as praças. O diagrama ia direto para "o serviço está no ar?". Entrou a decisão
nova, na ordem do código, com a saída "mais presentes que vagas" voltando para o metre rever a presença.

**Alocação — uma seta que enganava.** Na imagem, parecia sair de "Confirmar a alocação" uma seta direto
para o fim, além da seta para "Registrar" — o que em UML é paralelismo implícito, e o fluxo real é
sequencial. **Na fonte, o fluxo estava certo:** a seta era Registrar → fim, roteada por cima da caixa
"Confirmar", atravessando o texto dela. O nó final foi para baixo de "Registrar", e a seta de "aceito a
sugestão", que dava a volta e entrava em "Confirmar" por um laço, passou a entrar direto pelo topo.

É a lição oposta à da seção 4.2: lá a lista de elementos escondia o problema e só a imagem o mostrou; aqui a
imagem sugeria um problema que a fonte desmentia. **Revisar diagrama exige olhar os dois.**

### 4.12 A Matriz de Rastreabilidade não tinha o que entrou com a avaliação (#197)

Faltavam **RF25, UC25 e RN08** — tudo o que nasceu com a avaliação do atendimento (#39). Entraram as três, com a
fonte "Decisão da dupla (21/09) — issue #39", e o RF18 passou a ter na matriz o mesmo texto ampliado do
documento de requisitos.

**Sobre o RF12, uma correção.** A ficha de números da pesquisa (#205) dizia que o RF12 não aparecia em
lugar nenhum. Aparece, na matriz, e a história é melhor do que "foi retirado":

| ID | Descrição | Fonte | Artefatos | Status |
|---|---|---|---|---|
| RF12 | O Cliente poder solicitar a exclusão do seu histórico de pedidos a qualquer momento | Questionário de clientes (n = 13) — LGPD | (removido) — o sistema não identifica o cliente (RN04, RN08): não existe histórico de cliente a excluir | **Reprovado** |

Ou seja: o requisito foi **avaliado e reprovado**, e a matriz mantém a linha com o status para registrar a
decisão. É o jeito certo de documentar um requisito recusado — e responde a pergunta "cadê o RF12?" melhor
do que qualquer nota. Faltava o motivo, que agora está na coluna de artefatos: **o sistema não identifica o
cliente** (RN04, e agora RN08), então não existe histórico de cliente para excluir. A preocupação de LGPD
que originou o pedido é atendida justamente por não guardar dado pessoal do cliente.

**Por que as duas buscas erraram.** O Word parte o texto em pedaços (*runs*) sem critério visível: "UC15"
estava gravado como "UC1" + "5". Tirar as tags trocando-as por espaço separa os pedaços ("UC1 5") e a
busca não acha; tirar as tags sem espaço junta também células vizinhas ("RF11RF12RF13") e a busca por
palavra inteira falha. O primeiro erro quase registrou a UC15 como ausente; o segundo produziu a afirmação
errada sobre o RF12. **A extração certa é por célula:** juntar os pedaços dentro de cada `<w:tc>` e separar
uma célula da outra. Foi assim que esta seção foi conferida.

## 5. O que ficou pendente, e por quê

| Pendência | Por que não foi feito aqui | Issue |
|---|---|---|
| `avaliacao_atendimento` no MER e no DER | Exige o brModelo, que é aplicação gráfica | #196 |

## 6. Como repetir a conferência do banco

```bash
docker exec -i gastra-mysql sh -c 'mysql -N -B -u"$MYSQL_USER" -p"$MYSQL_PASSWORD" gastra_dev' \
  < docs/modelagem/engenharia-reversa/extrair_schema.sql > schema.tsv
```

E comparar com o DER conceitual exportado do brModelo, como descreve
[`engenharia-reversa/README.md`](engenharia-reversa/README.md). O passo do brModelo é manual: ele não tem
linha de comando para exportar XML.
