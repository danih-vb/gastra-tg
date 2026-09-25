# 6 CONSIDERAÇÕES FINAIS

> **[Rascunho para a dupla reescrever com a própria voz.]** Este capítulo avalia o trabalho, e a avaliação é de
> quem o fez: os fatos abaixo estão conferidos no repositório, mas a forma, a ênfase e as reflexões sobre as
> dificuldades devem ser da dupla. Os trechos entre colchetes dependem de trabalho ainda não concluído.

Este trabalho partiu de uma pergunta: como apoiar a tomada de decisão em um restaurante a partir dos próprios dados
gerados pela operação? O GASTRA responde a ela com um sistema que registra a operação do salão — a abertura da
mesa, os pedidos, o fechamento da conta — e usa esse registro para três finalidades que, no restaurante estudado,
dependiam da intuição ou da hierarquia informal: distribuir os garçons entre as praças, sugerir combinações de
pratos e acompanhar o desempenho da operação. A resposta vem acompanhada de uma condição que atravessou todo o
projeto: fazer isso sem identificar o cliente e tratando os dados dos funcionários com o mínimo necessário.

## 6.1 CUMPRIMENTO DOS OBJETIVOS

O objetivo geral — desenvolver um sistema de apoio à decisão que combine relatórios de Business Intelligence,
recomendação de pratos por ciência de dados e distribuição de garçons por programação linear, alimentado por um
módulo operacional de pedidos e em conformidade com a LGPD — foi atingido, com a ressalva sobre a clusterização
discutida adiante. Os objetivos específicos são avaliados um a um a seguir.

O levantamento de requisitos por entrevista e questionários foi realizado e formalizado em 24 requisitos
funcionais aprovados, cinco não funcionais e nove regras de negócio, cada um rastreado até a sua fonte, aos casos
de uso e aos testes que o verificam (seção 4.1). O banco de dados relacional foi modelado do nível conceitual ao
físico, validado por engenharia reversa e verificado quanto às formas normais, com uma única exceção consciente e
protegida por restrição no próprio banco (seção 4.3). O módulo operacional de comandas foi implementado e é o
alicerce do sistema: é ele que gera os dados que os blocos analíticos consomem, e ele continua funcionando mesmo
quando o serviço analítico está indisponível.

A aplicação web foi construída com backend em ASP.NET Core e frontend em Angular, incluindo os relatórios de
Business Intelligence por praça, por garçom, por horário e por item. A distribuição de garçons foi modelada como
um problema de designação e resolvida por programação linear, com pesos calibrados por simulação; sobre o histórico
simulado, a alocação entregou as melhores praças a quem menos havia faturado, que é exatamente o comportamento
pretendido (seções 5.4 e 5.5). A conformidade com a LGPD foi tratada como requisito de projeto, e não como
funcionalidade adicional: o cliente não é identificado em nenhuma tabela, o único dado sensível é apagado no
fechamento da comanda, e cada proteção foi traduzida em teste automatizado. Por fim, o sistema foi validado por 489
casos de teste automatizados, todos aprovados, nas três camadas de software [e pelos testes operacionais descritos
na seção 5.6].

A recomendação de pratos foi implementada por regras de associação, e a validação mostrou que o algoritmo
redescobre, a partir dos dados, as combinações de consumo embutidas no histórico. **A clusterização por padrão de
consumo, prevista no objetivo específico ao lado das regras de associação, não foi implementada.** [Se for
implementada antes da entrega, substituir este parágrafo pela descrição do resultado.]

## 6.2 LIMITAÇÕES

Os resultados deste trabalho devem ser lidos com as limitações abaixo, registradas ao longo do desenvolvimento.

- **Os algoritmos foram validados com dados simulados.** O restaurante colaborador não forneceu histórico, e os
  dados pessoais reais não poderiam ser usados sem as salvaguardas que a LGPD exige. As premissas da simulação
  estão explícitas, mas nenhuma delas substitui a operação de um restaurante de verdade. Tanto a calibração dos
  pesos da alocação quanto os limiares das regras de associação precisam ser refeitos com movimento real.
- **A amostra da pesquisa de campo é pequena.** O questionário de garçons teve dois respondentes e foi tratado
  como indício qualitativo, aprofundado pela entrevista; o de clientes teve treze. Os achados orientaram a
  especificação, mas não permitem generalizar para o setor.
- **A calibração considerou uma única configuração de salão.** Restaurantes com mais praças de alto potencial, ou
  com praças de potencial semelhante, podem exigir outros pesos.
- **O efeito da alocação sobre a desigualdade não foi medido ao longo do tempo sobre o histórico do sistema.** A
  calibração mostra o comportamento da regra em simulação, e a validação mostra uma alocação isolada; falta a curva
  da desigualdade caindo turno a turno com o próprio sistema alocando.
- **A avaliação do atendimento pelo cliente entrou no fim do desenvolvimento** e não tem dado de uso; por isso, o
  mínimo de cinco avaliações por garçom para que a nota entre no índice de desempenho é uma escolha de projeto, e
  não um valor calibrado.
- **[A avaliação heurística teve dois avaliadores, que também projetaram as telas]** — condição que tende a deixar
  passar problemas que um avaliador externo encontraria.
- **Os requisitos não funcionais de tempo de resposta [foram / não foram] medidos no sistema em operação**
  [conforme os testes operacionais da seção 5.6].

## 6.3 TRABALHOS FUTUROS

As limitações apontam os próximos passos mais diretos. O primeiro é operar o sistema em um restaurante real, com
o consentimento adequado, e refazer a calibração dos pesos e dos limiares a partir do histórico acumulado — a
arquitetura já prevê isso, porque o serviço analítico passa a usar o histórico do banco assim que ele atinge o
volume mínimo. O segundo é medir a evolução da desigualdade entre os garçons ao longo de sucessivos turnos alocados
pelo sistema, que é a demonstração mais forte do valor da RN03. O terceiro é a clusterização das comandas por
padrão de consumo, que permitiria segmentar a recomendação — por exemplo, por período do dia ou por composição da
mesa — sem recorrer a nenhum atributo pessoal do cliente, em linha com a RN05.

Do ponto de vista do produto, algumas evoluções foram identificadas e registradas durante o desenvolvimento: o
funcionamento do aplicativo do garçom com conexão instável, instalável no celular; a impressão dos códigos QR
fixos de cada mesa; e a publicação do sistema em um provedor de nuvem, cujos custos foram estimados na seção 3.2.

## 6.4 CONSIDERAÇÕES DA DUPLA

> **[A ESCREVER pela dupla.]** Sugestões de pontos, a partir do que o repositório registra: o que mudou na visão
> do problema depois da entrevista; a decisão de não identificar o cliente, que simplificou a LGPD e eliminou o
> RF12; o achado da calibração (o faturamento acumulado punia quem faltava e premiava de volta), que só apareceu
> simulando; a disciplina de Pull Request com revisão e integração contínua em uma dupla; e o que cada um levou do
> trabalho para a formação.
