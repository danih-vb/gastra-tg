# 6 CONSIDERAÇÕES FINAIS

Este trabalho partiu de uma pergunta: como apoiar a tomada de decisão em um restaurante a partir dos próprios dados
gerados pela operação? O GASTRA responde a ela com um sistema que registra a operação do salão (a abertura da
mesa, os pedidos, o fechamento da conta) e usa esse registro para três finalidades que, no restaurante estudado,
dependiam da intuição ou da hierarquia informal: distribuir os garçons entre as praças, sugerir combinações de
pratos e acompanhar o desempenho da operação. A resposta vem com uma condição que atravessou todo o projeto: fazer
isso sem identificar o cliente e tratando os dados dos funcionários com o mínimo necessário.

## 6.1 CUMPRIMENTO DOS OBJETIVOS

O objetivo geral foi atingido: o GASTRA combina relatórios de Business Intelligence, recomendação de pratos por
ciência de dados e distribuição de garçons por programação linear, alimentados por um módulo operacional de pedidos
e em conformidade com a LGPD. Os objetivos específicos são avaliados um a um a seguir.

O levantamento de requisitos por entrevista e questionários foi realizado e formalizado em 25 requisitos
funcionais aprovados, cinco não funcionais e nove regras de negócio, cada um rastreado até a sua fonte, aos casos
de uso e aos testes que o verificam (seção 4.1). O banco de dados relacional foi modelado do nível conceitual ao
físico, validado por engenharia reversa e verificado quanto às formas normais, com uma única exceção consciente e
protegida por restrição no próprio banco (seção 4.3). O módulo operacional de comandas foi implementado e é o
alicerce do sistema: é ele que gera os dados que os blocos analíticos consomem, e ele continua funcionando mesmo
quando o serviço analítico está fora do ar, como mostraram os testes operacionais (seção 5.2.6).

A aplicação web foi construída com backend em ASP.NET Core e frontend em Angular, incluindo os relatórios de
Business Intelligence por praça, por garçom, por horário e por item, além da engenharia de cardápio. A distribuição
de garçons foi modelada como um problema de designação e resolvida por programação linear, com pesos calibrados por
simulação. Sobre o histórico simulado, a alocação combinou os dois fatores da regra, levando a praça de maior
potencial a quem faturava pouco e esperava havia mais tempo por ela (seções 5.2.3 e 5.2.4), e, aplicada por turnos
sucessivos, reduziu em 92% a desigualdade de faturamento entre os garçons em 30 dias. A conformidade com a LGPD foi
tratada como requisito de projeto, e não como funcionalidade adicional: o cliente não é identificado em nenhuma
tabela, o único dado sensível é apagado no fechamento da comanda, e cada proteção foi traduzida em teste
automatizado. Por fim, o sistema foi validado por 576 casos de teste automatizados, todos aprovados, nas três
camadas de software, e por duas rodadas de testes operacionais com o sistema completo, que encontraram e levaram à
correção de dois defeitos que os testes automatizados não haviam acusado (seção 5.2.6).

A recomendação de pratos combinou as duas técnicas previstas no objetivo específico. A clusterização agrupou as
comandas em perfis de consumo e, sobre o histórico simulado, redescobriu os três perfis nele embutidos sem receber
nenhuma informação além dos itens pedidos; as regras de associação redescobriram as combinações embutidas. Juntas,
na recomendação segmentada por perfil, elevaram o acerto das sugestões de 76,2% para 83,1% na avaliação por item
escondido (seção 5.2.4). Durante o desenvolvimento, verificou-se que essa parte do objetivo havia ficado de fora da
primeira implementação, que só tinha as regras de associação; a lacuna foi identificada na revisão dos capítulos e
corrigida antes da entrega.

## 6.2 LIMITAÇÕES

Os resultados deste trabalho devem ser lidos com as limitações abaixo, registradas ao longo do desenvolvimento.

- **Os algoritmos foram validados com dados simulados.** O restaurante colaborador não forneceu histórico, e dados
  pessoais reais não poderiam ser usados sem as salvaguardas que a LGPD exige. As premissas da simulação estão
  explícitas, mas nenhuma delas substitui a operação de um restaurante de verdade. Tanto a calibração dos pesos da
  alocação quanto os limiares das regras de associação precisam ser refeitos com movimento real.
- **A amostra da pesquisa de campo é pequena.** O questionário de garçons teve dois respondentes e foi tratado como
  indício qualitativo, aprofundado pela entrevista; o de clientes teve treze. Os achados orientaram a especificação,
  mas não permitem generalizar para o setor.
- **A calibração considerou uma única configuração de salão.** Restaurantes com mais praças de alto potencial, ou com
  praças de potencial semelhante, podem exigir outros pesos.
- **A queda da desigualdade foi medida em simulação.** O experimento da seção 5.2.4 parte do estado do banco e aplica
  a programação linear do sistema, mas o faturamento de cada turno seguinte é simulado, com o fator de venda de cada
  garçom mantido constante. Em operação real, o desempenho de cada um muda com o tempo e com a praça.
- **A avaliação do atendimento pelo cliente entrou no fim do desenvolvimento** e não tem dado de uso; por isso, o
  mínimo de cinco avaliações por garçom para que a nota entre no índice de desempenho é uma escolha de projeto, e
  não um valor calibrado.
- **A engenharia de cardápio usa o preço, e não a margem.** O método original compara a margem de contribuição de
  cada prato (KASAVANA; SMITH, 1982), mas o sistema não guarda o custo dos ingredientes. A classificação mostra o que
  traz faturamento, e não necessariamente o que dá lucro.
- **A avaliação heurística foi feita pelos próprios autores.** Os dois conhecem o sistema por dentro e são menos do
  que os três a cinco avaliadores recomendados (NIELSEN, 1994), o que tende a deixar passar problemas que
  avaliadores externos e usuários reais encontrariam.
- **Os testes de tempo de resposta não foram feitos no salão.** O RNF01 foi medido com duas janelas abertas no mesmo
  computador, uma com o garçom e outra com a conta do cliente; a leitura do QR code foi feita por um celular na mesma
  rede doméstica. Em um restaurante, com vários celulares e a rede do estabelecimento, os tempos podem ser outros.

## 6.3 TRABALHOS FUTUROS

As limitações apontam os próximos passos mais diretos. O primeiro é operar o sistema em um restaurante real, com o
consentimento adequado, e refazer a calibração dos pesos e dos limiares a partir do histórico acumulado; a
arquitetura já prevê isso, porque o serviço analítico passa a usar o histórico do banco assim que ele atinge o
volume mínimo. O segundo é acompanhar, nessa operação, a evolução da desigualdade entre os garçons, para confirmar
com dados reais a queda observada em simulação (seção 5.2.4). O terceiro é avaliar a recomendação segmentada com o
uso real: a taxa de acerto da seção 5.2.4 foi medida sobre dados simulados, e o indicador que importa ao
restaurante é quantas sugestões o garçom de fato oferece e quantas o cliente aceita. O quarto é observar garçons e
clientes usando o sistema no salão, o que complementaria a avaliação heurística com testes de usabilidade.

Do ponto de vista do produto, algumas evoluções foram identificadas durante o desenvolvimento:

- guardar o custo de cada prato, para que a engenharia de cardápio passe a usar a margem de contribuição, como no
  método original;
- tornar o aplicativo do garçom instalável no celular e capaz de funcionar com conexão instável;
- gerar, pelo próprio sistema, os códigos QR fixos de cada mesa para impressão;
- ampliar a tela de desempenho do garçom com a evolução do índice ao longo das semanas e com metas do turno, sempre
  comparando cada garçom com ele mesmo, e não com os colegas, para não contrariar o objetivo da RN03;
- oferecer um programa de fidelidade, o que exigiria identificar o cliente com consentimento explícito e, portanto,
  tratar dados pessoais conforme a LGPD, revendo uma decisão central deste trabalho;
- publicar o sistema em um provedor de nuvem, cujos custos foram estimados na seção 3.2.

## 6.4 CONSIDERAÇÕES DA DUPLA

A entrevista com o garçom deu peso a uma parte do problema que os números sozinhos não mostravam. Como o garçom
costuma receber por comissão, a praça em que ele trabalha define quanto ganha, e a entrevista confirmou que a
distribuição atual é percebida como desigual, por depender da hierarquia informal da equipe. Isso reforçou a RN03 e
orientou a forma de medir o resultado: mais importante do que faturar mais em um turno era reduzir a desigualdade
ao longo do tempo.

A decisão de não identificar o cliente foi a que mais simplificou o projeto. Ela eliminou o RF12, reduziu ao mínimo
o tratamento de dados dos clientes (o único dado sensível, a restrição alimentar, fica presa à comanda e é apagada
no fechamento) e fez com que a análise trabalhasse com comandas, e não com pessoas. Alguns achados só apareceram quando o sistema foi exercitado como um todo: a calibração mostrou que
somar o faturamento acumulado punia quem faltava, e os testes operacionais encontraram defeitos que os testes
automatizados não acusavam. Isso reforçou o valor de simular e de testar com o sistema completo, e não apenas cada
parte isoladamente.

Por fim, o trabalho em dupla com Pull Request, revisão do outro integrante e integração contínua exigiu disciplina,
mas garantiu que cada mudança fosse vista por duas pessoas antes de entrar. O projeto reuniu, em um único sistema,
conteúdos de banco de dados, engenharia de software, programação web, estatística, programação linear e segurança
da informação, e deixou aos autores a experiência de levar uma solução do levantamento de requisitos até um
sistema em execução e testado.
