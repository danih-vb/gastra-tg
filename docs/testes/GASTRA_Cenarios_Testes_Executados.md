# GASTRA — Cenários de teste executados

> **Gerado a partir da execução real dos testes automatizados** (`docs/testes/gerar_relatorio_testes.py`).
> Não edite à mão: rode os testes e o script de novo.

- **Execução:** 17/09/2026 01:51, commit `155a29b`.
- **Ambiente:** MySQL 8.4 real (testes de views, triggers e restrições), serviço Python no ar (testes de contrato),
  .NET 10 e Python 3.13.
- **Resultado:** 242 cenários e 284 casos executados: 284 passaram, 0 falharam e 0 foram ignorados.

Cobre o critério da issue #48 para os **testes automatizados**: pelo menos um cenário por bloco (comandas,
alocação por programação linear, recomendação e LGPD), com o resultado executado. Os cenários operacionais
de ponta a ponta com o frontend entram no marco M6.

## Resumo por bloco

| Bloco | Cenários | Casos | Passaram | Falharam | Ignorados |
|---|---|---|---|---|---|
| Núcleo de comandas | 55 | 63 | 63 | 0 | 0 |
| Cardápio e promoções | 37 | 54 | 54 | 0 | 0 |
| Alocação de garçons (programação linear) | 37 | 43 | 43 | 0 | 0 |
| Recomendação de pratos (ciência de dados) | 22 | 24 | 24 | 0 | 0 |
| BI e índice de desempenho | 20 | 21 | 21 | 0 | 0 |
| LGPD, auditoria e segurança | 39 | 40 | 40 | 0 | 0 |
| Banco de dados (views, triggers e restrições) | 14 | 14 | 14 | 0 | 0 |
| Integração backend ↔ Python | 11 | 14 | 14 | 0 | 0 |
| Arquitetura e infraestrutura | 7 | 11 | 11 | 0 | 0 |

## Núcleo de comandas

| # | Cenário | Origem | Casos | Resultado |
|---|---|---|---|---|
| 1 | Abrir com mesa e pessoas retorna 201 com composicao sugerida e codigo de acesso | `ComandaControllerTests` | 1 | ✅ Passou |
| 2 | Abrir com mesa inexistente retorna 404 | `ComandaControllerTests` | 1 | ✅ Passou |
| 3 | Abrir comanda como gerente retorna 403 mas leitura e permitida | `ComandaControllerTests` | 1 | ✅ Passou |
| 4 | Abrir sem pessoas retorna 400 | `ComandaControllerTests` | 1 | ✅ Passou |
| 5 | Ajustar composicao na mao impede que o sistema reclassifique | `ComandaControllerTests` | 1 | ✅ Passou |
| 6 | Cancelar item com motivo tira o item da conta | `ComandaControllerTests` | 1 | ✅ Passou |
| 7 | Cancelar item sem motivo retorna 400 | `ComandaControllerTests` | 1 | ✅ Passou |
| 8 | Comanda fechada nao aceita novo item | `ComandaControllerTests` | 1 | ✅ Passou |
| 9 | Comandas sem login retorna 401 | `ComandaControllerTests` | 1 | ✅ Passou |
| 10 | Consulta do cliente com codigo inexistente retorna 404 | `ComandaControllerTests` | 1 | ✅ Passou |
| 11 | Consulta do cliente pelo codigo de acesso funciona sem login e nao expoe restricao | `ComandaControllerTests` | 1 | ✅ Passou |
| 12 | Fechar apaga a observacao livre da restricao e mantem a categoria | `ComandaControllerTests` | 1 | ✅ Passou |
| 13 | Fechar com item entregue calcula total com taxa de dez por cento | `ComandaControllerTests` | 1 | ✅ Passou |
| 14 | Fechar com item pendente retorna 422 | `ComandaControllerTests` | 1 | ✅ Passou |
| 15 | Lancar item copia o preco do cardapio e soma no subtotal | `ComandaControllerTests` | 1 | ✅ Passou |
| 16 | Lancar item indisponivel retorna 422 | `ComandaControllerTests` | 1 | ✅ Passou |
| 17 | Lancar item infantil em mesa de tres muda a composicao para familia | `ComandaControllerTests` | 1 | ✅ Passou |
| 18 | Remover taxa servico zera a taxa no fechamento | `ComandaControllerTests` | 1 | ✅ Passou |
| 19 | Restricao depois do fechamento nem o garcom ve | `ComandaControllerTests` | 1 | ✅ Passou |
| 20 | Restricao garcom ve com a comanda aberta | `ComandaControllerTests` | 1 | ✅ Passou |
| 21 | Restricao gerente nao ve nem na consulta nem no painel | `ComandaControllerTests` | 1 | ✅ Passou |
| 22 | Restricao metre ve com a comanda aberta | `ComandaControllerTests` | 1 | ✅ Passou |
| 23 | Abrir com zero pessoas lanca excecao | `ComandaTests` | 1 | ✅ Passou |
| 24 | Abrir sugere a composicao pela quantidade de pessoas | `ComandaTests` | 6 | ✅ Passou |
| 25 | Adicionar item copia o preco do momento e nao muda depois | `ComandaTests` | 1 | ✅ Passou |
| 26 | Adicionar item indisponivel lanca excecao | `ComandaTests` | 1 | ✅ Passou |
| 27 | Adicionar item infantil em mesa de duas pessoas nao vira familia | `ComandaTests` | 1 | ✅ Passou |
| 28 | Adicionar item infantil em mesa de tres ou mais reclassifica como familia | `ComandaTests` | 1 | ✅ Passou |
| 29 | Ajustar composicao na mao congela a regra automatica | `ComandaTests` | 1 | ✅ Passou |
| 30 | Calcular total soma taxa de dez por cento | `ComandaTests` | 1 | ✅ Passou |
| 31 | Comanda fechada nao aceita novo item nem restricao | `ComandaTests` | 1 | ✅ Passou |
| 32 | Confirmar a sugestao sem mudar nao congela a regra automatica | `ComandaTests` | 1 | ✅ Passou |
| 33 | Confirmar composicao guarda o ajuste do garcom | `ComandaTests` | 1 | ✅ Passou |
| 34 | Fechar apaga a observacao livre da restricao e mantem a categoria | `ComandaTests` | 1 | ✅ Passou |
| 35 | Fechar com item pendente lanca excecao | `ComandaTests` | 1 | ✅ Passou |
| 36 | Fechar com todos os itens entregues fecha e registra a hora | `ComandaTests` | 1 | ✅ Passou |
| 37 | Item cancelado nao entra na conta e nao impede o fechamento | `ComandaTests` | 1 | ✅ Passou |
| 38 | Registrar restricao guarda categoria e observacao sem espacos sobrando | `ComandaTests` | 1 | ✅ Passou |
| 39 | Registrar restricao sem observacao fica nula | `ComandaTests` | 1 | ✅ Passou |
| 40 | Remover taxa servico zera a taxa | `ComandaTests` | 1 | ✅ Passou |
| 41 | Restricoes visiveis com comanda aberta so para quem atende a mesa | `ComandaTests` | 4 | ✅ Passou |
| 42 | Restricoes visiveis depois do fechamento ninguem ve | `ComandaTests` | 1 | ✅ Passou |
| 43 | Cadastrar mesa com numero repetido retorna 422 | `SalaoControllerTests` | 1 | ✅ Passou |
| 44 | Cadastrar mesa em praca inexistente retorna 404 | `SalaoControllerTests` | 1 | ✅ Passou |
| 45 | Cadastrar mesa sem capacidade retorna 400 | `SalaoControllerTests` | 1 | ✅ Passou |
| 46 | Cadastrar mesa vincula a praca e aparece na listagem | `SalaoControllerTests` | 1 | ✅ Passou |
| 47 | Cadastrar praca com codigo repetido retorna 422 | `SalaoControllerTests` | 1 | ✅ Passou |
| 48 | Cadastrar praca com dados validos retorna 201 | `SalaoControllerTests` | 1 | ✅ Passou |
| 49 | Cadastrar praca sem garcons retorna 400 | `SalaoControllerTests` | 1 | ✅ Passou |
| 50 | Editar mesa altera numero e capacidade sem trocar de praca | `SalaoControllerTests` | 1 | ✅ Passou |
| 51 | Editar praca altera a quantidade de garcons | `SalaoControllerTests` | 1 | ✅ Passou |
| 52 | Editar praca inexistente retorna 404 | `SalaoControllerTests` | 1 | ✅ Passou |
| 53 | Garcom nao cadastra mesa mas precisa enxergar a lista | `SalaoControllerTests` | 1 | ✅ Passou |
| 54 | Mesa cadastrada pelo gerente permite ao garcom abrir comanda | `SalaoControllerTests` | 1 | ✅ Passou |
| 55 | Salao sem login retorna 401 | `SalaoControllerTests` | 1 | ✅ Passou |

## Cardápio e promoções

| # | Cenário | Origem | Casos | Resultado |
|---|---|---|---|---|
| 1 | Atualizar preco de item existente retorna 204 e o preco muda | `CardapioControllerTests` | 1 | ✅ Passou |
| 2 | Atualizar preco de item inexistente retorna 404 com mensagem | `CardapioControllerTests` | 1 | ✅ Passou |
| 3 | Cadastrar com categoria inexistente retorna 400 no formato padrao | `CardapioControllerTests` | 1 | ✅ Passou |
| 4 | Cadastrar com dados invalidos em ingles retorna mensagens em ingles | `CardapioControllerTests` | 1 | ✅ Passou |
| 5 | Cadastrar com dados validos retorna 201 com o item | `CardapioControllerTests` | 1 | ✅ Passou |
| 6 | Cadastrar com nome vazio e preco zero retorna 400 com os dois erros em portugues | `CardapioControllerTests` | 1 | ✅ Passou |
| 7 | Cadastrar como garcom retorna 403 | `CardapioControllerTests` | 1 | ✅ Passou |
| 8 | Cadastrar sem login retorna 401 | `CardapioControllerTests` | 1 | ✅ Passou |
| 9 | Cardapio digital sem login retorna 200 | `CardapioControllerTests` | 1 | ✅ Passou |
| 10 | Marcar indisponivel item sai do cardapio digital mas continua na gestao | `CardapioControllerTests` | 1 | ✅ Passou |
| 11 | Atende restricao alergia e outro nao filtram porque dependem do texto livre | `ItemDoCardapioTests` | 2 | ✅ Passou |
| 12 | Atende restricao confere as flags do cardapio | `ItemDoCardapioTests` | 7 | ✅ Passou |
| 13 | Atende restricao item sem flag nao atende restricao verificavel | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 14 | Atualizar preco com valor invalido mantem preco anterior | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 15 | Atualizar preco com valor valido altera preco | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 16 | Criar com dados validos fica disponivel | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 17 | Criar com flag repetida guarda uma vez so | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 18 | Criar com preco zero ou negativo lanca excecao | `ItemDoCardapioTests` | 2 | ✅ Passou |
| 19 | Flags dieteticas nao pode ser alterada de fora | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 20 | Marcar disponibilidade falso torna item indisponivel | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 21 | Cardapio digital mostra o preco promocional so do item em promocao | `PromocaoControllerTests` | 1 | ✅ Passou |
| 22 | Criar com dados invalidos retorna 400 | `PromocaoControllerTests` | 3 | ✅ Passou |
| 23 | Criar com desconto fixo maior que o preco retorna 422 | `PromocaoControllerTests` | 1 | ✅ Passou |
| 24 | Criar com item inexistente retorna 404 | `PromocaoControllerTests` | 1 | ✅ Passou |
| 25 | Criar devolve preco com desconto de cada item e registra na auditoria | `PromocaoControllerTests` | 1 | ✅ Passou |
| 26 | Garcom nao gerencia promocoes | `PromocaoControllerTests` | 1 | ✅ Passou |
| 27 | Item lancado na comanda sai com o preco promocional | `PromocaoControllerTests` | 1 | ✅ Passou |
| 28 | Remover desativa sem apagar e o preco volta ao normal | `PromocaoControllerTests` | 1 | ✅ Passou |
| 29 | Aplicar desconto arredonda em centavos | `PromocaoTests` | 3 | ✅ Passou |
| 30 | Aplicar desconto nunca deixa o item de graca | `PromocaoTests` | 1 | ✅ Passou |
| 31 | Comanda guarda o preco promocional no pedido mas nunca acima do cardapio | `PromocaoTests` | 1 | ✅ Passou |
| 32 | Criar com desconto invalido lanca excecao | `PromocaoTests` | 3 | ✅ Passou |
| 33 | Criar com fim antes do inicio ou sem itens lanca excecao | `PromocaoTests` | 1 | ✅ Passou |
| 34 | Desativada deixa de valer e nao desativa duas vezes | `PromocaoTests` | 1 | ✅ Passou |
| 35 | Preco promocional com duas promocoes vale a de maior desconto | `PromocaoTests` | 1 | ✅ Passou |
| 36 | Preco promocional sem promocao para o item ou fora do periodo e nulo | `PromocaoTests` | 1 | ✅ Passou |
| 37 | Vigente em inclui os dois extremos | `PromocaoTests` | 4 | ✅ Passou |

## Alocação de garçons (programação linear)

| # | Cenário | Origem | Casos | Resultado |
|---|---|---|---|---|
| 1 | Ajuste depois de confirmado retorna 422 | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 2 | Ajuste para praca lotada retorna 422 | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 3 | Ajuste troca a praca e registra sugerida e escolhida | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 4 | Confirmacao de turno sem alocacao retorna 404 | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 5 | Confirmacao trava o turno e registra na auditoria | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 6 | Garcom nao gera sugestao mas consulta o turno | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 7 | Periodo inexistente na rota retorna 400 | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 8 | Sugestao com python fora responde 200 para alocacao manual e registra a falha | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 9 | Sugestao com quem nao e garcom retorna 422 | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 10 | Sugestao com resposta incoerente do python nao grava nada | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 11 | Sugestao depois de confirmado retorna 422 | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 12 | Sugestao envia ao python os fatores da RN03 e grava o resultado | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 13 | Sugestao gerada de novo substitui a anterior nao confirmada | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 14 | Sugestao sem garcons retorna 400 | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 15 | Sugestao sem pracas cadastradas retorna 422 | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 16 | Ajustar antes de confirmar troca a praca | `AlocacaoTests` | 1 | ✅ Passou |
| 17 | Ajustar depois de confirmar lanca excecao | `AlocacaoTests` | 1 | ✅ Passou |
| 18 | Confirmar duas vezes lanca excecao | `AlocacaoTests` | 1 | ✅ Passou |
| 19 | Criar comeca nao confirmada | `AlocacaoTests` | 1 | ✅ Passou |
| 20 | Alto potencial sao as pracas acima da media das que tiveram movimento | `RegraDeDistribuicaoTests` | 1 | ✅ Passou |
| 21 | Alto potencial sem historico nenhuma praca | `RegraDeDistribuicaoTests` | 1 | ✅ Passou |
| 22 | Alto potencial todas iguais nenhuma praca | `RegraDeDistribuicaoTests` | 1 | ✅ Passou |
| 23 | Pesos somam um | `RegraDeDistribuicaoTests` | 1 | ✅ Passou |
| 24 | Turnos desde praca de alto potencial | `RegraDeDistribuicaoTests` | 4 | ✅ Passou |
| 25 | Cada garcom recebe exatamente uma praca e as vagas sao respeitadas | `test_alocacao` | 1 | ✅ Passou |
| 26 | Custos ficam entre zero e um | `test_alocacao` | 1 | ✅ Passou |
| 27 | Pesos precisam somar um | `test_alocacao` | 1 | ✅ Passou |
| 28 | Praca de alto potencial vai para quem faturou menos | `test_alocacao` | 1 | ✅ Passou |
| 29 | Quem espera ha mais turnos ganha a praca boa no empate de faturamento | `test_alocacao` | 1 | ✅ Passou |
| 30 | Sem vagas suficientes a alocacao e inviavel | `test_alocacao` | 1 | ✅ Passou |
| 31 | So o peso da espera inverte a escolha | `test_alocacao` | 1 | ✅ Passou |
| 32 | Escolhe o menor peso com desigualdade proxima da menor | `test_calibracao` | 1 | ✅ Passou |
| 33 | Gini | `test_calibracao` | 4 | ✅ Passou |
| 34 | Peso todo na espera reduz a espera maxima | `test_calibracao` | 1 | ✅ Passou |
| 35 | Politicas de referencia ocupam todas as pracas sem estourar vagas | `test_calibracao` | 1 | ✅ Passou |
| 36 | Rn 03 com peso calibrado distribui melhor que deixar sem regra | `test_calibracao` | 1 | ✅ Passou |
| 37 | Simulacao e deterministica | `test_calibracao` | 1 | ✅ Passou |

## Recomendação de pratos (ciência de dados)

| # | Cenário | Origem | Casos | Resultado |
|---|---|---|---|---|
| 1 | Comanda fechada retorna 422 | `SugestoesComandaTests` | 1 | ✅ Passou |
| 2 | Comanda inexistente retorna 404 | `SugestoesComandaTests` | 1 | ✅ Passou |
| 3 | Descarta id que o servico nao podia sugerir | `SugestoesComandaTests` | 1 | ✅ Passou |
| 4 | Devolve os itens na ordem do servico com nome categoria e preco | `SugestoesComandaTests` | 1 | ✅ Passou |
| 5 | Envia ao servico so o que pode ser oferecido | `SugestoesComandaTests` | 1 | ✅ Passou |
| 6 | Gerente nao recebe sugestao retorna 403 | `SugestoesComandaTests` | 1 | ✅ Passou |
| 7 | Restricao que o cardapio nao confere pede confirmacao com o cliente | `SugestoesComandaTests` | 3 | ✅ Passou |
| 8 | Sem item valido na comanda nem chama o servico | `SugestoesComandaTests` | 1 | ✅ Passou |
| 9 | Servico fora responde 200 com lista vazia e sem travar a comanda | `SugestoesComandaTests` | 1 | ✅ Passou |
| 10 | Alocacao com pesos que nao somam um retorna 422 | `test_api_analitica` | 1 | ✅ Passou |
| 11 | Alocacao devolve uma praca para cada garcom | `test_api_analitica` | 1 | ✅ Passou |
| 12 | Alocacao sem vagas suficientes retorna 422 | `test_api_analitica` | 1 | ✅ Passou |
| 13 | Recomendacao aceita historico informado pelo backend | `test_api_analitica` | 1 | ✅ Passou |
| 14 | Recomendacao filtra pelos itens disponiveis | `test_api_analitica` | 1 | ✅ Passou |
| 15 | Recomendacao usa o historico simulado quando nenhum e informado | `test_api_analitica` | 1 | ✅ Passou |
| 16 | Descarta regras com lift abaixo de um | `test_recomendacao` | 1 | ✅ Passou |
| 17 | Historico sem combinacoes nao gera regra | `test_recomendacao` | 1 | ✅ Passou |
| 18 | Nao sugere item que ja esta na comanda | `test_recomendacao` | 1 | ✅ Passou |
| 19 | Recupera a combinacao plantada no simulador | `test_recomendacao` | 1 | ✅ Passou |
| 20 | Respeita a lista de itens disponiveis | `test_recomendacao` | 1 | ✅ Passou |
| 21 | Respeita o limite de sugestoes | `test_recomendacao` | 1 | ✅ Passou |
| 22 | Resultado e o mesmo para a mesma semente | `test_recomendacao` | 1 | ✅ Passou |

## BI e índice de desempenho

| # | Cenário | Origem | Casos | Resultado |
|---|---|---|---|---|
| 1 | Periodo invalido ou longo demais retorna 400 | `IndicadoresControllerTests` | 2 | ✅ Passou |
| 2 | Periodo sem filtro sao os ultimos 30 dias com fim inclusivo | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 3 | Ranking o garcom ve so a propria posicao sem nome dos colegas | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 4 | Ranking o gerente ve todos | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 5 | Relatorio de garcons calcula medias e totais com nome | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 6 | Relatorio de horarios traz hora e dia da semana com nome | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 7 | Relatorio de pracas mostra todas as pracas inclusive sem movimento | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 8 | Relatorio do cardapio agrupa por categoria com participacao | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 9 | Relatorios sao do gerente e registram a consulta | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 10 | Compara por turno quem trabalhou mais nao ganha so por isso | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 11 | Empate no indice divide a posicao | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 12 | Nao e apenas venda mesas atendidas pesam metade | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 13 | Quem lidera faturamento e mesas por turno faz 100 | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 14 | Sem turnos fica fora do ranking | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 15 | Faturamento medio por praca inclui praca sem movimento | `RepositorioIndicadoresTests` | 1 | ✅ Passou |
| 16 | Faturamento por hora e por dia da semana usam o horario de brasilia | `RepositorioIndicadoresTests` | 1 | ✅ Passou |
| 17 | Faturamento por turno do garcom e a media dos turnos no intervalo com fim exclusivo | `RepositorioIndicadoresTests` | 1 | ✅ Passou |
| 18 | Indicadores por garcom contam turnos mesas e minutos de atendimento | `RepositorioIndicadoresTests` | 1 | ✅ Passou |
| 19 | Indicadores por item trazem categoria quantidade e faturamento | `RepositorioIndicadoresTests` | 1 | ✅ Passou |
| 20 | Indicadores por praca somam comandas e turnos com movimento | `RepositorioIndicadoresTests` | 1 | ✅ Passou |

## LGPD, auditoria e segurança

| # | Cenário | Origem | Casos | Resultado |
|---|---|---|---|---|
| 1 | Cardapio registra cadastro preco anterior e novo e disponibilidade | `AuditoriaTests` | 1 | ✅ Passou |
| 2 | Comanda registra ciclo completo sem codigo de acesso nem restricao | `AuditoriaTests` | 1 | ✅ Passou |
| 3 | Contas registram criacao edicao com papel anterior e novo sem nome nem email | `AuditoriaTests` | 1 | ✅ Passou |
| 4 | Login com email inexistente nao guarda o email digitado | `AuditoriaTests` | 1 | ✅ Passou |
| 5 | Login com senha errada registra falha com a conta e o motivo sem a senha | `AuditoriaTests` | 1 | ✅ Passou |
| 6 | Login com sucesso registra ator papel e ip | `AuditoriaTests` | 1 | ✅ Passou |
| 7 | Login de conta inativa registra o motivo | `AuditoriaTests` | 1 | ✅ Passou |
| 8 | Logoff registra o ator | `AuditoriaTests` | 1 | ✅ Passou |
| 9 | Operacao recusada nao gera registro de sucesso | `AuditoriaTests` | 1 | ✅ Passou |
| 10 | Salao registra cadastro e edicao de praca e mesa | `AuditoriaTests` | 1 | ✅ Passou |
| 11 | Segundo fator registra vinculacao confirmacao e recusa sem codigo nem segredo | `AuditoriaTests` | 1 | ✅ Passou |
| 12 | Login com senha errada ou email inexistente retorna 401 com a mesma mensagem | `AutenticacaoControllerTests` | 2 | ✅ Passou |
| 13 | Login garcom com credenciais validas recebe token de acesso direto | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 14 | Login gerente nao recebe token de acesso e precisa configurar o autenticador | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 15 | Login sem email e senha retorna 400 | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 16 | Login usuario inativo retorna 401 | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 17 | Logoff invalida o token no servidor | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 18 | Segundo fator com codigo errado retorna 401 | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 19 | Segundo fator fluxo completo libera acesso e o segredo nao pode ser gerado de novo | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 20 | Token do segundo fator nao serve como token de acesso | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 21 | Elimina so o vencido respeitando a folga do trigger e registra a eliminacao | `EliminacaoAuditoriaTests` | 1 | ✅ Passou |
| 22 | Sem nada vencido nao apaga nem registra | `EliminacaoAuditoriaTests` | 1 | ✅ Passou |
| 23 | Cadastrar com dados invalidos retorna 400 com todos os erros | `UsuarioControllerTests` | 1 | ✅ Passou |
| 24 | Cadastrar com dados validos retorna 201 e o usuario consegue entrar | `UsuarioControllerTests` | 1 | ✅ Passou |
| 25 | Cadastrar com email ja usado em outra caixa retorna 422 | `UsuarioControllerTests` | 1 | ✅ Passou |
| 26 | Cadastrar com senha acima do limite do b crypt retorna 400 | `UsuarioControllerTests` | 1 | ✅ Passou |
| 27 | Editar com email de outra conta retorna 422 | `UsuarioControllerTests` | 1 | ✅ Passou |
| 28 | Editar o proprio papel retorna 422 | `UsuarioControllerTests` | 1 | ✅ Passou |
| 29 | Editar trocando o papel derruba o token antigo e novo login vem com o novo papel | `UsuarioControllerTests` | 1 | ✅ Passou |
| 30 | Gestao de contas como garcom retorna 403 | `UsuarioControllerTests` | 1 | ✅ Passou |
| 31 | Gestao de contas sem login retorna 401 | `UsuarioControllerTests` | 1 | ✅ Passou |
| 32 | Inativar a propria conta retorna 422 | `UsuarioControllerTests` | 1 | ✅ Passou |
| 33 | Inativar bloqueia token e login e reativar devolve o acesso | `UsuarioControllerTests` | 1 | ✅ Passou |
| 34 | Listar traz contas ativas e inativas | `UsuarioControllerTests` | 1 | ✅ Passou |
| 35 | Obter inexistente retorna 404 | `UsuarioControllerTests` | 1 | ✅ Passou |
| 36 | Atualizar dados mantendo papel nao encerra as sessoes | `UsuarioTests` | 1 | ✅ Passou |
| 37 | Atualizar dados trocando papel encerra as sessoes | `UsuarioTests` | 1 | ✅ Passou |
| 38 | Criar normaliza email e comeca ativo | `UsuarioTests` | 1 | ✅ Passou |
| 39 | Inativar encerra as sessoes e reativar devolve o acesso | `UsuarioTests` | 1 | ✅ Passou |

## Banco de dados (views, triggers e restrições)

| # | Cenário | Origem | Casos | Resultado |
|---|---|---|---|---|
| 1 | Comanda nao pode ter status e data de fechamento contraditorios | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 2 | Data turno e hora seguem o horario de brasilia | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 3 | Desempenho do garcom conta mesas distintas por turno | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 4 | Eliminacao por prazo apaga so o que passou de 6 meses | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 5 | Faturamento da comanda ignora itens cancelados e comandas abertas | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 6 | Faturamento da praca por turno soma as comandas do turno | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 7 | Faturamento medio da praca e calculado por turno e inclui praca sem movimento | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 8 | Faturamento por item traz a categoria do cardapio | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 9 | Itens por comanda juntam linhas do mesmo item e descartam cancelados | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 10 | Motivo de cancelamento existe so em item cancelado | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 11 | Promocao nao aceita periodo invertido nem percentual de 100 | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 12 | Registro de auditoria aceita insercao mas nunca alteracao | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 13 | Registro de auditoria dentro do prazo de retencao nao pode ser apagado | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 14 | Views nao expoem dado pessoal | `ObjetosDoBancoTests` | 1 | ✅ Passou |

## Integração backend ↔ Python

| # | Cenário | Origem | Casos | Resultado |
|---|---|---|---|---|
| 1 | Servico real da a praca boa para quem faturou menos | `ContratoComPythonTests` | 1 | ✅ Passou |
| 2 | Servico real sugere o arroz de coco para quem pediu moqueca | `ContratoComPythonTests` | 1 | ✅ Passou |
| 3 | Configuracao invalida impede a subida da api | `RegistroServicoAnaliticoTests` | 2 | ✅ Passou |
| 4 | Usa o cliente http com endereco e tempo limite da configuracao | `RegistroServicoAnaliticoTests` | 1 | ✅ Passou |
| 5 | Alocacao envia garcons pracas e pesos em snake case e le as designacoes | `ServicoAnaliticoHttpTests` | 1 | ✅ Passou |
| 6 | Cancelamento pedido por quem chamou nao eh tratado como falha do servico | `ServicoAnaliticoHttpTests` | 1 | ✅ Passou |
| 7 | Envia o contrato em snake case e le os ids na ordem | `ServicoAnaliticoHttpTests` | 1 | ✅ Passou |
| 8 | Resposta invalida vira servico indisponivel | `ServicoAnaliticoHttpTests` | 3 | ✅ Passou |
| 9 | Resposta lenta estoura o tempo limite e vira servico indisponivel | `ServicoAnaliticoHttpTests` | 1 | ✅ Passou |
| 10 | Servico fora do ar vira servico indisponivel | `ServicoAnaliticoHttpTests` | 1 | ✅ Passou |
| 11 | Status de erro vira servico indisponivel | `ServicoAnaliticoHttpTests` | 1 | ✅ Passou |

## Arquitetura e infraestrutura

| # | Cenário | Origem | Casos | Resultado |
|---|---|---|---|---|
| 1 | Application nao depende de infrastructure nem da api | `ArquiteturaTests` | 1 | ✅ Passou |
| 2 | Camada base nao depende de outra camada | `ArquiteturaTests` | 3 | ✅ Passou |
| 3 | Infrastructure depende apenas do domain | `ArquiteturaTests` | 1 | ✅ Passou |
| 4 | Documentacao fora do ambiente de desenvolvimento nao fica exposta | `DocumentacaoOpenApiTests` | 2 | ✅ Passou |
| 5 | Health responde ok quando a api sobe | `HealthCheckTests` | 1 | ✅ Passou |
| 6 | Algoritmo nao importa fastapi | `test_arquitetura` | 2 | ✅ Passou |
| 7 | Health responde healthy | `test_health` | 1 | ✅ Passou |
