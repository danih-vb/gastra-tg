# GASTRA — Números da pesquisa

Reúne num lugar só os números que saem dos questionários e das entrevistas, **cada um com o denominador
explícito**. Quem escrever qualquer capítulo do documento de pesquisa cita daqui, e não de memória nem de
outro documento. Issue #205.

> **Regra:** porcentagem sem denominador não entra no documento. "77% dos clientes" não diz nada sozinho;
> "77% (10 de 13)" diz, e permite a quem lê refazer a conta.

## 1. Fonte de verdade

A fonte é o **CSV exportado do questionário** (Jotform). Ele responde por todos os números desta ficha.

> ⚠️ **O CSV não está no repositório.** Isso é uma lacuna: hoje ninguém consegue refazer nenhuma conta
> desta tabela a partir do que está versionado, e foi exatamente por isso que três números divergiram
> entre documentos. Antes da entrega, o CSV **anonimizado** deveria entrar em
> `docs/requisitos/questionarios/`, junto do instrumento de coleta. Sem dado pessoal: o que a pesquisa usa
> são as respostas, não quem respondeu.

## 2. Questionário de clientes (n = 13)

| Achado | Número correto | Conta | Onde costuma aparecer errado |
|---|---|---|---|
| Dificuldade de identificar restrições alimentares no cardápio | **77%** | 10 de 13 | Aparecia como 83% no Roteiro de Entrevista — corrigido |
| Conforto com uso de histórico anonimizado | **85%** | 11 de 13 (84,6%, arredondado) | — |
| "O que mais incomoda": demora | **40%** das seleções, ou **46%** dos respondentes | 6 de 15 seleções; 6 de 13 pessoas | Aparecia como 50% no Roteiro — corrigido |
| "O que mais incomoda": segundo colocado | **33%** das seleções, ou **38%** dos respondentes | 5 de 15 seleções; 5 de 13 pessoas | — |

### Cuidado com a pergunta de múltipla escolha

"O que mais incomoda" aceita **mais de uma marcação por pessoa**: 13 pessoas fizeram 15 seleções. Isso dá
duas porcentagens diferentes e igualmente corretas, que respondem a perguntas diferentes:

- **Sobre as seleções** (denominador 15): "40% das reclamações são sobre demora".
- **Sobre as pessoas** (denominador 13): "46% dos clientes citaram demora".

As duas podem ser usadas, **desde que o denominador venha junto**. O que não pode é somar as porcentagens
da primeira forma e esperar 100% por pessoa.

## 3. Questionário de garçons (n = 2)

| Achado | Número | Conta |
|---|---|---|
| Percebem diferença de potencial entre as praças | 100% | 2 de 2 |
| Motivação com ranking de desempenho | 100% | 2 de 2 |
| Nota para justiça na distribuição das praças | 4,0 de 5 | média de 2 respostas |

> **n = 2 não sustenta porcentagem.** Escrever "100% dos garçons" é tecnicamente verdadeiro e
> retoricamente desonesto. No documento, esses achados devem aparecer como **indício qualitativo**, com o
> n visível, e é assim que o Roteiro de Entrevista já os trata — a entrevista existe justamente para
> aprofundar o que o questionário não sustenta.

## 4. Instrumentos

| Instrumento | Perguntas | Onde está |
|---|---|---|
| Questionário de garçons | **29** | `questionarios/GASTRA_Instrumento_Coleta.docx` |
| Questionário de clientes | ver instrumento | `questionarios/GASTRA_Instrumento_Coleta.docx` |
| Roteiro de entrevista (semiestruturada) | — | `entrevistas/GASTRA_Roteiro_Entrevista_Restaurante.docx` |

O Roteiro dizia "24 perguntas" para o questionário de garçons; o instrumento e o CSV têm **29**. Corrigido.

## 5. Numeração dos requisitos

| Conjunto | Quantidade | Faixa |
|---|---:|---|
| Requisitos funcionais (RF) aprovados | **24** | RF01–RF25, **sem RF12** |
| Requisito funcional reprovado | 1 | RF12 |
| Requisitos não funcionais (RNF) | 5 | RNF01–RNF05 |
| Regras de negócio (RN) | 9 | RN01–RN09 (a RN09, bloqueio por tentativas, veio da #230, e não da pesquisa) |

> **O RF12 foi avaliado e reprovado.** Era "o Cliente poder solicitar a exclusão do seu histórico de pedidos
> a qualquer momento", vindo do questionário de clientes (LGPD). A Matriz de Rastreabilidade mantém a linha
> com o status **Reprovado**, que é o registro da decisão; os demais não foram renumerados, para não quebrar
> as referências cruzadas. São **24 requisitos funcionais aprovados, numerados até 25**, e o documento de
> pesquisa precisa dizer isso — senão a pergunta "cadê o RF12?" aparece na banca.
>
> O motivo da reprovação agora está na matriz: o sistema **não identifica o cliente** (RN04, RN08), então não
> existe histórico de cliente para excluir (#229).
>
> *Correção: a primeira versão desta ficha dizia que o RF12 não aparecia em nenhum documento. Aparece, na
> matriz. A busca tinha falhado porque o Word parte o texto das células em pedaços; ver
> `docs/modelagem/GASTRA_Revisao_Modelagem.md`, seção 4.12.*

## 6. Correções aplicadas

| Documento | Estava | Ficou |
|---|---|---|
| `GASTRA_Roteiro_Entrevista_Restaurante.docx` | 83% relatam dificuldade com flags | 77% (10 de 13) |
| `GASTRA_Roteiro_Entrevista_Restaurante.docx` | questionário com 24 perguntas | 29 perguntas |
| `GASTRA_Roteiro_Entrevista_Restaurante.docx` | demora como maior incômodo (50%) | 40% das seleções (6 de 15) |

Nenhum outro documento do repositório cita esses números hoje — a varredura procurou em todos os `.docx`
de `docs/`. O capítulo 4 do documento de pesquisa, quando for escrito, é o próximo lugar em que eles vão
aparecer, e é para isso que esta ficha existe.
