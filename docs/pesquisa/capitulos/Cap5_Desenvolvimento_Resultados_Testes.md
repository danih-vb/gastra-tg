# 5 DESENVOLVIMENTO, RESULTADOS E TESTES

Este capítulo apresenta o produto construído e as evidências de que ele cumpre o que foi especificado. A seção 5.1
descreve como o desenvolvimento foi conduzido; a seção 5.2 apresenta o sistema do ponto de vista de cada usuário;
a seção 5.3 reúne os testes automatizados; e as seções 5.4 e 5.5 apresentam os dois estudos feitos sobre os
algoritmos: a calibração dos pesos da alocação de garçons e a validação dos blocos analíticos sobre um histórico
simulado. A seção 5.6 trata dos testes operacionais. Os números deste capítulo correspondem ao estado do
repositório em 25 de setembro de 2026 e são reproduzíveis: cada tabela indica o script ou o teste que a gera.

## 5.1 PROCESSO DE DESENVOLVIMENTO

O trabalho foi organizado em dez marcos no quadro de projeto do GitHub, do levantamento de requisitos (M1) à
defesa (M10), e executado em sprints semanais, conforme descrito na seção 1.5. Cada tarefa foi registrada como
issue, com critérios de aceite, prioridade, marco e sprint, e cada mudança no repositório entrou por Pull
Request: uma regra do repositório impede a integração na branch de desenvolvimento sem a aprovação do outro
integrante. Entre 17 de agosto e 25 de setembro de 2026, foram
integrados 84 Pull Requests e fechadas 114 issues.

A revisão cruzada foi apoiada por integração contínua: a cada Pull Request, o GitHub Actions executa cinco
verificações independentes, cujo resultado aparece no próprio Pull Request para o revisor. A primeira compila o backend e
executa os seus testes contra um MySQL real e contra o serviço analítico em execução, e falha também se algum
teste tiver sido ignorado — o que impede que um teste de banco deixe de rodar sem que ninguém perceba. A segunda
executa os testes da camada analítica; a terceira compila o frontend e executa os seus testes; a quarta constrói
a imagem Docker do frontend e verifica que ela sobe, responde e se declara saudável; e a quinta varre o
repositório em busca de segredos, como senhas e chaves, que não devem ser versionados.

Esse conjunto de verificações encontrou defeitos que a revisão humana deixaria passar. Um exemplo registrado no
projeto é o da imagem do frontend: o servidor nginx recusava-se a iniciar quando o backend ainda não estava no ar,
porque tentava resolver o nome do serviço na partida, e o problema só apareceu na integração contínua, que sobe a
imagem sozinha. Outro exemplo mostra o limite da própria verificação: o contêiner do frontend respondia
normalmente pela porta publicada, mas a sua checagem de saúde interna falhava, porque, dentro do contêiner, o nome
"localhost" resolvia para o endereço IPv6, que o servidor não escutava. A integração contínua, que testava apenas
por fora, não o detectou; o defeito foi encontrado durante a execução do sistema completo, corrigido e passou a
ser verificado também pela integração contínua.

## 5.2 O SISTEMA CONSTRUÍDO

O GASTRA é uma aplicação web única, que apresenta a cada papel apenas as telas que lhe cabem. Esta seção percorre
o sistema pela perspectiva de cada usuário; o funcionamento detalhado de cada tela está no manual do usuário, no
Apêndice A.

> **[SUBSTITUIR as figuras desta seção pelas capturas das telas em Angular.]** As imagens abaixo são do protótipo
> navegável (seção 4.4.5). As telas do Cliente podem ser capturadas sem login; as dos demais papéis precisam de
> uma sessão aberta. Manter os mesmos enquadramentos.

O Garçom usa o sistema no celular. A tela inicial mostra as mesas da praça em que ele foi alocado no turno,
separadas das mesas dos colegas, e abre uma mesa em dois toques — a mesa livre e a quantidade de pessoas —, dentro
do limite do RNF02. Na comanda (Figura 10, no capítulo 4), ele lança os itens, acompanha os pendentes, registra a
restrição alimentar informada pelo cliente e recebe as sugestões de pratos; se o serviço analítico estiver fora do
ar, a comanda continua funcionando e apenas as sugestões deixam de aparecer. O fechamento só é liberado com todos
os itens entregues ou cancelados com motivo (RN02), e a taxa de serviço vem incluída, removível a pedido do
cliente.

**Figura 11 – Mesas do Garçom**

![Mesas do Garçom](../../ux-ui/telas/garcom-mesas.png)

Fonte: elaborado pelos autores.

O Metre monta o turno em três passos: marca os garçons presentes, recebe a sugestão de alocação e a confirma,
podendo antes mover qualquer garçom de praça (Figura 12). A sugestão mostra, embaixo do nome de cada garçom, o
motivo da escolha — a faixa de vendas em relação à equipe e há quantos turnos ele não recebe uma praça de alto
movimento —, sem exibir valores de faturamento, que são indicadores de desempenho restritos ao Gerente (decisão
D14). Depois de confirmada, a alocação não muda mais e passa a compor o histórico usado pela RN03.

**Figura 12 – Sugestão de alocação de garçons**

![Sugestão de alocação de garçons](../../ux-ui/telas/metre-sugestao.png)

Fonte: elaborado pelos autores.

O Gerente acompanha a operação pelos relatórios de Business Intelligence (Figura 13): faturamento por praça, com
destaque para as de alto potencial, pela mesma regra usada na alocação; faturamento por hora e por dia da semana,
em mapa de calor; ranking dos garçons pelo índice de desempenho; faturamento por item e por categoria; e a média
das avaliações dos clientes, sempre agregada. O Gerente também gerencia o cardápio, as promoções, as praças, as
mesas e as contas de usuário; o Coordenador tem acesso somente ao cardápio e às promoções.

**Figura 13 – Relatórios de Business Intelligence do Gerente**

![Relatórios de Business Intelligence](../../ux-ui/telas/gerente-analises.png)

Fonte: elaborado pelos autores.

O Cliente não se autentica. Pelo QR code da mesa, ele consulta o cardápio digital, com filtros por restrição
alimentar, e acompanha a própria conta em tempo real, com a situação de cada item (Figura 14). Depois que a conta
é fechada, pode avaliar o atendimento com uma nota de 1 a 5 e um comentário opcional, sem que nada o identifique.

**Figura 14 – Conta do Cliente**

![Conta do Cliente](../../ux-ui/telas/cliente-conta.png)

Fonte: elaborado pelos autores.

## 5.3 TESTES AUTOMATIZADOS

A verificação do GASTRA apoia-se em testes automatizados escritos junto do código, em todas as camadas. No
backend, os testes de unidade verificam as regras do domínio sem banco nem rede — a composição da mesa, o cálculo
da taxa, o bloqueio de conta, o índice de desempenho —, e os testes de integração sobem a interface de programação
completa e fazem requisições reais a ela, verificando código de resposta, mensagem, permissão de cada papel e
registro de auditoria. Os objetos que só existem no banco — views, triggers e restrições CHECK — são testados
contra um MySQL real, e a comunicação com o serviço analítico é testada tanto com um dublê, para simular falhas e
lentidão, quanto contra o serviço real em execução. Na camada analítica, os testes verificam os algoritmos e o
contrato do serviço; no frontend, verificam cada tela e os serviços de sessão e navegação. Testes de arquitetura,
nas duas linguagens, garantem que a regra de dependência entre as camadas continua valendo.

A Tabela 2 apresenta o resultado da execução completa, organizado pelos blocos do trabalho. Um cenário é o que o
teste verifica; um caso é cada execução do cenário, já que um mesmo cenário pode ser executado com vários
conjuntos de dados.

**Tabela 2 – Testes automatizados executados, por bloco**

| Bloco | Cenários | Casos | Aprovados |
|---|---:|---:|---:|
| Núcleo de comandas | 109 | 118 | 118 |
| Cardápio e promoções | 53 | 72 | 72 |
| Alocação de garçons (programação linear) | 58 | 64 | 64 |
| Recomendação de pratos (ciência de dados) | 37 | 42 | 42 |
| BI e índice de desempenho | 42 | 43 | 43 |
| LGPD, auditoria e segurança | 108 | 109 | 109 |
| Banco de dados (views, triggers e restrições) | 14 | 14 | 14 |
| Integração backend e serviço analítico | 11 | 14 | 14 |
| Arquitetura e infraestrutura | 9 | 13 | 13 |
| **Total** | **441** | **489** | **489** |

Fonte: elaborado pelos autores, a partir da execução de 25/09/2026 (script `gerar_relatorio_testes.py`).

Os 489 casos foram aprovados, nenhum falhou e nenhum foi ignorado. Por camada, são 316 casos no backend, 50 na
camada analítica e 123 no frontend. Cada bloco do trabalho é verificado nas três camadas: a mesma regra de
negócio é testada no domínio, na interface de programação e na tela que a apresenta ao usuário. O bloco de
proteção de dados é o segundo maior, porque cada restrição da LGPD foi traduzida em ao menos um teste que falha se
ela for violada — por exemplo, o que verifica que nenhuma view expõe dado pessoal, o que verifica que o Gerente
nunca recebe a restrição alimentar do cliente e o que verifica que a avaliação do atendimento não guarda nada que
identifique quem avaliou. A relação entre cada história de usuário e os testes que comprovam os seus critérios de
aceite está no Quadro 5 (seção 4.2).

Os requisitos não funcionais foram verificados da forma que cada um permite. O RNF02 é verificado por um teste da
tela de mesas, que abre uma mesa em dois toques. O RNF05 é verificado pelos testes que derrubam o serviço
analítico e conferem que a comanda continua aceitando pedidos e que o Metre consegue alocar manualmente. O RNF03 e
o RNF04 são verificados pelos testes de permissão de cada papel e de proteção de dados. O RNF01, que limita a dois
segundos o tempo para um pedido aparecer na comanda, depende do sistema em operação e é tratado na seção 5.6.

## 5.4 CALIBRAÇÃO DOS PESOS DA ALOCAÇÃO

A RN03 combina dois fatores com os pesos w1, para o equilíbrio de faturamento, e w2, para o tempo desde a última
praça de alto potencial, com w1 + w2 = 1. Como o restaurante colaborador não forneceu histórico e o sistema ainda
não tinha movimento real, os pesos foram definidos por simulação, com premissas explícitas: um salão com sete
garçons e três praças de potencial diferente — forte, com três vagas e faturamento médio de R$ 1.800 por turno;
média, com três vagas e R$ 1.200; e fraca, com duas vagas e R$ 600 —, presença de 85% por garçom e por turno,
variação de 25% no movimento de cada praça entre turnos e de 10% no desempenho individual. Cada simulação cobriu
120 turnos, o equivalente a 60 dias com almoço e jantar, e foi repetida com 20 sementes aleatórias para cada peso
testado. A cada turno, a alocação foi resolvida pela mesma função de programação linear usada pelo sistema em
produção.

Duas métricas foram medidas. A principal é a desigualdade, dada pelo coeficiente de Gini do faturamento médio por
turno de cada garçom, em que zero significa todos iguais: é a dor que apareceu espontaneamente na pesquisa de
campo. A secundária é a espera máxima: a maior sequência de turnos em que algum garçom trabalhou sem receber uma
praça de alto potencial. Para comparação, duas políticas de referência rodaram nas mesmas sementes: uma sem regra,
em que cada garçom tende a ficar sempre na mesma praça, e um rodízio simples, que gira a ordem dos garçons a cada
turno. Os resultados estão na Tabela 3 e na Figura 15.

**Tabela 3 – Resultado da calibração dos pesos da RN03**

| Política | Gini médio | Espera máxima média (turnos) |
|---|---:|---:|
| w1 = 0,0 | 0,0473 | 4,30 |
| w1 = 0,1 | 0,0302 | 4,65 |
| w1 = 0,2 | 0,0282 | 4,80 |
| w1 = 0,3 | 0,0264 | 5,35 |
| w1 = 0,4 | 0,0233 | 6,30 |
| w1 = 0,5 | 0,0158 | 8,25 |
| **w1 = 0,6** | **0,0067** | **8,90** |
| w1 = 0,7 | 0,0054 | 9,35 |
| w1 = 0,8 | 0,0055 | 9,80 |
| w1 = 0,9 | 0,0048 | 10,85 |
| w1 = 1,0 | 0,0055 | 10,85 |
| Sem regra | 0,0456 | 34,80 |
| Rodízio simples | 0,0153 | 5,90 |

Fonte: elaborado pelos autores, com 20 sementes de 120 turnos para cada política.

**Figura 15 – Desigualdade e espera máxima em função do peso w1**

![Calibração dos pesos da RN03](../../analises/calibracao_rn03.png)

Fonte: elaborado pelos autores.

Qualquer peso é muito melhor do que não ter regra: a espera máxima cai de 34,8 para, no máximo, 10,9 turnos. As
duas métricas, porém, puxam para lados opostos — mais peso no faturamento reduz a desigualdade e aumenta a espera
—, e nenhum peso supera o rodízio simples nas duas ao mesmo tempo. O critério de escolha seguiu a prioridade da
pesquisa: entre os pesos cuja desigualdade fica até 50% acima da menor obtida, escolhe-se o menor w1, que é o que
mais protege a espera. O menor Gini foi 0,0048, com w1 = 0,9; aceitam-se, portanto, valores até 0,0072, o que
inclui os pesos de 0,6 a 1,0, e o menor deles é w1 = 0,6. Com esse peso, a desigualdade fica 56% menor que a do
rodízio simples, com uma espera 51% maior, de 8,9 turnos — cerca de quatro dias e meio de trabalho. A escolha é
estável: com tolerância de 25%, o critério escolheria 0,7; com 100%, novamente 0,6.

A calibração também revelou um defeito na formulação original do primeiro fator. Na primeira rodada, o fator era
o faturamento acumulado do garçom nos últimos 30 dias, e a programação linear foi pior que o rodízio simples em
desigualdade. A causa: pela soma, quem faltou mais turnos aparece com menos faturamento e é tratado como quem
está para trás, recebendo as melhores praças ao voltar. O fator foi corrigido para o faturamento médio por turno
trabalhado, tanto no backend quanto no serviço analítico, e a Tabela 3 já corresponde à versão corrigida. O
episódio mostra o valor de simular antes de operar: o defeito não seria visível em um teste de unidade, porque a
fórmula estava implementada exatamente como especificada.

## 5.5 VALIDAÇÃO SOBRE DADOS SIMULADOS

Para verificar os blocos analíticos em funcionamento, e não apenas isoladamente, o banco foi preenchido com um
histórico simulado por uma ferramenta que grava pelas mesmas entidades do domínio usadas pelo sistema (decisão
D11), de modo que os dados respeitam as mesmas regras de composição da mesa, taxa e totais. Com a semente 42, a
ferramenta gerou 70 dias de operação, com 4.213 comandas fechadas e 12.753 itens. Os dados são fictícios e não
representam o restaurante colaborador.

No histórico, foram embutidas algumas combinações de consumo conhecidas, como "quem pede moqueca pede arroz de
coco" com 85% de probabilidade. Elas funcionam como gabarito: o algoritmo de regras de associação precisa
redescobri-las a partir dos dados, sem que ninguém as informe. Com 3.128 comandas de dois itens ou mais, o serviço
analítico passou a usar o histórico do banco, e não o simulado em memória, e, para uma comanda que tinha só a
moqueca, sugeriu os itens da Tabela 4.

**Tabela 4 – Sugestões para uma comanda com moqueca e combinações embutidas no histórico**

| Sugestão | Confiança obtida | Lift | Probabilidade embutida |
|---|---:|---:|---:|
| Arroz de coco | 0,871 | 1,552 | 0,85 |
| Caipirinha | 0,749 | 1,294 | 0,60 |

Fonte: elaborado pelos autores.

As duas combinações foram reencontradas, na ordem correta e com confiança próxima da embutida, e o lift acima de 1
indica associação maior do que o acaso explicaria. A alocação de garçons, calculada sobre o faturamento dos
últimos 30 dias devolvido pelas views, produziu o comportamento que a RN03 existe para produzir (Tabela 5): a ordem
saiu exatamente invertida, com as vagas da praça de maior potencial para os dois garçons que menos faturaram por
turno e as da praça mais fraca para os dois que mais faturaram.

**Tabela 5 – Alocação calculada sobre o histórico simulado**

| Garçom | Faturamento por turno (R$) | Praça designada | Potencial da praça (R$ por turno) |
|---|---:|---|---:|
| 5 | 386,85 | 1 | 1.876 |
| 7 | 516,75 | 1 | 1.876 |
| 6 | 532,06 | 2 | 1.244 |
| 8 | 626,90 | 2 | 1.244 |
| 9 | 664,81 | 2 | 1.244 |
| 10 | 729,85 | 3 | 615 |
| 11 | 816,42 | 3 | 615 |

Fonte: elaborado pelos autores.

Uma divergência foi registrada como achado. O Gini do histórico simulado é 0,1235, muito acima dos valores da
calibração. A diferença é esperada, porque as duas medidas não medem a mesma coisa: a calibração mede a
desigualdade depois de 120 turnos aplicando a RN03, e o histórico simulado é o estado inicial, com garçons de
desempenho propositalmente diferente — o que mais fatura ganha 2,1 vezes o que menos fatura — e distribuídos por
rodízio neutro. É o problema que a RN03 existe para corrigir. A demonstração mais forte da regra seria medir o
Gini depois de sucessivos turnos alocados pelo próprio sistema sobre essa base, acompanhando a queda a partir de
0,12; esse experimento ficou registrado como trabalho a realizar.

> **[ATUALIZAR se o experimento da issue #221 for feito antes da entrega: incluir a curva do Gini por turno.]**

## 5.6 TESTES OPERACIONAIS

> **[A PREENCHER depois da Sprint 5 — issues #207 a #211.]** Descrever os cenários de ponta a ponta executados
> com o sistema completo em contêineres (navegador contra a API e o banco reais, sem dublês): um roteiro por papel,
> do login à tarefa principal; a medição do RNF01 (tempo até o pedido aparecer na comanda do cliente) e do RNF02
> (toques para abrir mesa e lançar item) no sistema real; e os defeitos encontrados, com a correção de cada um.
> Incluir o teste em máquina limpa (#177): o sistema subindo com um único comando em um computador sem as
> ferramentas de desenvolvimento.
