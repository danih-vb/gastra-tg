# GASTRA — Cenários de teste executados

> **Gerado a partir da execução real dos testes automatizados** (`docs/testes/gerar_relatorio_testes.py`).
> Não edite à mão: rode os testes e o script de novo.

- **Execução:** 25/09/2026 12:50, commit `3bb7a75`.
- **Ambiente:** MySQL 8.4 real (testes de views, triggers e restrições), serviço Python no ar (testes de contrato),
  .NET 10, Python 3.13 e Angular 22 sobre Vitest.
- **Resultado:** 441 cenários e 489 casos executados: 489 passaram, 0 falharam e 0 foram ignorados.

Cobre o critério da issue #48 para os **testes automatizados**: pelo menos um cenário por bloco (comandas,
alocação por programação linear, recomendação e LGPD), com o resultado executado. Cada bloco aparece nas três
camadas em que o GASTRA foi construído — backend .NET, camada analítica Python e as telas Angular —, de modo
que a regra de negócio e a tela que a mostra ao usuário são verificadas no mesmo lugar.

O que **não** está aqui: os cenários operacionais de ponta a ponta (navegador contra a API e o banco reais,
sem dublê), que dependem de dados de operação e entram no marco M6.

## Resumo por camada

| Camada | Cenários | Casos | Passaram | Falharam | Ignorados |
|---|---|---|---|---|---|
| Backend .NET | 275 | 316 | 316 | 0 | 0 |
| Analítica Python | 43 | 50 | 50 | 0 | 0 |
| Frontend Angular | 123 | 123 | 123 | 0 | 0 |

## Resumo por bloco

| Bloco | Cenários | Casos | Passaram | Falharam | Ignorados |
|---|---|---|---|---|---|
| Núcleo de comandas | 109 | 118 | 118 | 0 | 0 |
| Cardápio e promoções | 53 | 72 | 72 | 0 | 0 |
| Alocação de garçons (programação linear) | 58 | 64 | 64 | 0 | 0 |
| Recomendação de pratos (ciência de dados) | 37 | 42 | 42 | 0 | 0 |
| BI e índice de desempenho | 42 | 43 | 43 | 0 | 0 |
| LGPD, auditoria e segurança | 108 | 109 | 109 | 0 | 0 |
| Banco de dados (views, triggers e restrições) | 14 | 14 | 14 | 0 | 0 |
| Integração backend ↔ Python | 11 | 14 | 14 | 0 | 0 |
| Arquitetura e infraestrutura | 9 | 13 | 13 | 0 | 0 |

## Núcleo de comandas

| # | Cenário | Camada | Origem | Casos | Resultado |
|---|---|---|---|---|---|
| 1 | Abrir com mesa e pessoas retorna 201 com composicao sugerida e codigo de acesso | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 2 | Abrir com mesa inexistente retorna 404 | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 3 | Abrir comanda como gerente retorna 403 mas leitura e permitida | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 4 | Abrir sem pessoas retorna 400 | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 5 | Ajustar composicao na mao impede que o sistema reclassifique | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 6 | Avaliacao com codigo inexistente retorna 404 | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 7 | Avaliacao com conta aberta recusa | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 8 | Avaliacao com nota fora da escala retorna 400 | Backend .NET | `ComandaControllerTests` | 2 | ✅ Passou |
| 9 | Avaliacao depois de fechar aceita sem login e nao deixa avaliar de novo | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 10 | Avaliacao nao identifica quem avaliou | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 11 | Cancelar item com motivo tira o item da conta | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 12 | Cancelar item sem motivo retorna 400 | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 13 | Comanda fechada nao aceita novo item | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 14 | Comanda traz o garcom que abriu na abertura na consulta e na lista | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 15 | Comandas sem login retorna 401 | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 16 | Consulta do cliente com codigo inexistente retorna 404 | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 17 | Consulta do cliente diz se pode avaliar e quando ja avaliou | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 18 | Consulta do cliente pelo codigo de acesso funciona sem login e nao expoe restricao | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 19 | Fechar apaga a observacao livre da restricao e mantem a categoria | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 20 | Fechar com item entregue calcula total com taxa de dez por cento | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 21 | Fechar com item pendente retorna 422 | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 22 | Lancar item copia o preco do cardapio e soma no subtotal | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 23 | Lancar item indisponivel retorna 422 | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 24 | Lancar item infantil em mesa de tres muda a composicao para familia | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 25 | Remover taxa servico zera a taxa no fechamento | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 26 | Restricao depois do fechamento nem o garcom ve | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 27 | Restricao garcom ve com a comanda aberta | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 28 | Restricao gerente nao ve nem na consulta nem no painel | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 29 | Restricao metre ve com a comanda aberta | Backend .NET | `ComandaControllerTests` | 1 | ✅ Passou |
| 30 | Abrir com zero pessoas lanca excecao | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 31 | Abrir sugere a composicao pela quantidade de pessoas | Backend .NET | `ComandaTests` | 6 | ✅ Passou |
| 32 | Adicionar item copia o preco do momento e nao muda depois | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 33 | Adicionar item indisponivel lanca excecao | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 34 | Adicionar item infantil em mesa de duas pessoas nao vira familia | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 35 | Adicionar item infantil em mesa de tres ou mais reclassifica como familia | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 36 | Ajustar composicao na mao congela a regra automatica | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 37 | Calcular total soma taxa de dez por cento | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 38 | Comanda fechada nao aceita novo item nem restricao | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 39 | Confirmar a sugestao sem mudar nao congela a regra automatica | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 40 | Confirmar composicao guarda o ajuste do garcom | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 41 | Fechar apaga a observacao livre da restricao e mantem a categoria | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 42 | Fechar com item pendente lanca excecao | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 43 | Fechar com todos os itens entregues fecha e registra a hora | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 44 | Item cancelado nao entra na conta e nao impede o fechamento | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 45 | Registrar restricao guarda categoria e observacao sem espacos sobrando | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 46 | Registrar restricao sem observacao fica nula | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 47 | Remover taxa servico zera a taxa | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 48 | Restricoes visiveis com comanda aberta so para quem atende a mesa | Backend .NET | `ComandaTests` | 4 | ✅ Passou |
| 49 | Restricoes visiveis depois do fechamento ninguem ve | Backend .NET | `ComandaTests` | 1 | ✅ Passou |
| 50 | Cadastrar mesa com numero repetido retorna 422 | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 51 | Cadastrar mesa em praca inexistente retorna 404 | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 52 | Cadastrar mesa sem capacidade retorna 400 | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 53 | Cadastrar mesa vincula a praca e aparece na listagem | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 54 | Cadastrar praca com codigo repetido retorna 422 | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 55 | Cadastrar praca com dados validos retorna 201 | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 56 | Cadastrar praca sem garcons retorna 400 | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 57 | Editar mesa altera numero e capacidade sem trocar de praca | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 58 | Editar praca altera a quantidade de garcons | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 59 | Editar praca inexistente retorna 404 | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 60 | Garcom nao cadastra mesa mas precisa enxergar a lista | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 61 | Mesa cadastrada pelo gerente permite ao garcom abrir comanda | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 62 | Salao sem login retorna 401 | Backend .NET | `SalaoControllerTests` | 1 | ✅ Passou |
| 63 | Cancelar só libera depois de escolher o motivo (RN02) | Frontend Angular | `Comanda (UC12, UC13, UC18, UC23)` | 1 | ✅ Passou |
| 64 | Com o serviço de análise fora do ar, avisa e mantém o resto da comanda (D3) | Frontend Angular | `Comanda (UC12, UC13, UC18, UC23)` | 1 | ✅ Passou |
| 65 | Entregar não pede confirmação, e o aviso não oferece desfazer (a API não volta atrás) | Frontend Angular | `Comanda (UC12, UC13, UC18, UC23)` | 1 | ✅ Passou |
| 66 | Lança o item e oferece desfazer, que cancela por erro de lançamento | Frontend Angular | `Comanda (UC12, UC13, UC18, UC23)` | 1 | ✅ Passou |
| 67 | Mostra a mesa, as pendências e o preço promocional do cardápio | Frontend Angular | `Comanda (UC12, UC13, UC18, UC23)` | 1 | ✅ Passou |
| 68 | Registra a restrição com a observação e avisa que ela é apagada no fechamento (RF14) | Frontend Angular | `Comanda (UC12, UC13, UC18, UC23)` | 1 | ✅ Passou |
| 69 | Atualizar agora busca a conta de novo | Frontend Angular | `Conta do cliente (UC20)` | 1 | ✅ Passou |
| 70 | Avisa para nao escrever dado pessoal no comentario | Frontend Angular | `Conta do cliente (UC20)` | 1 | ✅ Passou |
| 71 | Com a conta aberta, nao oferece avaliar: o atendimento ainda esta acontecendo | Frontend Angular | `Conta do cliente (UC20)` | 1 | ✅ Passou |
| 72 | Com a conta fechada, agradece e mostra o total final | Frontend Angular | `Conta do cliente (UC20)` | 1 | ✅ Passou |
| 73 | Código inexistente não vira erro de sistema: explica o que fazer | Frontend Angular | `Conta do cliente (UC20)` | 1 | ✅ Passou |
| 74 | Envia a nota e o comentario, e depois agradece (RF25) | Frontend Angular | `Conta do cliente (UC20)` | 1 | ✅ Passou |
| 75 | Erro da API na avaliacao aparece dentro do bloco, sem derrubar a conta | Frontend Angular | `Conta do cliente (UC20)` | 1 | ✅ Passou |
| 76 | Explica que a taxa é opcional e que nada pessoal aparece (RN04) | Frontend Angular | `Conta do cliente (UC20)` | 1 | ✅ Passou |
| 77 | Mostra os itens com a situação de cada um e os totais | Frontend Angular | `Conta do cliente (UC20)` | 1 | ✅ Passou |
| 78 | Sem escolher nota, o botao de enviar fica desabilitado | Frontend Angular | `Conta do cliente (UC20)` | 1 | ✅ Passou |
| 79 | Com item pendente, explica o bloqueio e não deixa fechar (RN02) | Frontend Angular | `FecharConta (UC14)` | 1 | ✅ Passou |
| 80 | Erro da API no fechamento aparece na tela | Frontend Angular | `FecharConta (UC14)` | 1 | ✅ Passou |
| 81 | Fecha depois de confirmar e avisa que as restrições foram apagadas | Frontend Angular | `FecharConta (UC14)` | 1 | ✅ Passou |
| 82 | Mostra subtotal, taxa de 10% e total, e cobra a taxa por padrão (RF04) | Frontend Angular | `FecharConta (UC14)` | 1 | ✅ Passou |
| 83 | Tira a taxa a pedido do cliente e não deixa recolocar | Frontend Angular | `FecharConta (UC14)` | 1 | ✅ Passou |
| 84 | Agrupa as mesas por praça, em ordem, e soma vagas e lugares | Frontend Angular | `Gestão do salão (UC24)` | 1 | ✅ Passou |
| 85 | Cria a praça com código e vagas | Frontend Angular | `Gestão do salão (UC24)` | 1 | ✅ Passou |
| 86 | Mostra dentro do painel a mensagem da API quando o código repete | Frontend Angular | `Gestão do salão (UC24)` | 1 | ✅ Passou |
| 87 | Nova mesa vai para a praça escolhida; editar mesa não deixa mudar de praça (REL01) | Frontend Angular | `Gestão do salão (UC24)` | 1 | ✅ Passou |
| 88 | Sem praça cadastrada, explica o primeiro passo e não deixa criar mesa | Frontend Angular | `Gestão do salão (UC24)` | 1 | ✅ Passou |
| 89 | Código incompleto não chama a API: avisa no próprio campo | Frontend Angular | `Início do cliente (QR code da mesa)` | 1 | ✅ Passou |
| 90 | Deixa claro que o pedido é feito com o garçom | Frontend Angular | `Início do cliente (QR code da mesa)` | 1 | ✅ Passou |
| 91 | Leva para a conta da mesa com o código digitado, em maiúsculas | Frontend Angular | `Início do cliente (QR code da mesa)` | 1 | ✅ Passou |
| 92 | Abre a mesa em dois toques: a mesa livre e a quantidade de pessoas (RNF02) | Frontend Angular | `Mesas (UC10)` | 1 | ✅ Passou |
| 93 | Abre na praça do garçom e separa as mesas dele das dos colegas | Frontend Angular | `Mesas (UC10)` | 1 | ✅ Passou |
| 94 | Mesa de colega não abre comanda: avisa de quem é | Frontend Angular | `Mesas (UC10)` | 1 | ✅ Passou |
| 95 | Sem alocação confirmada, mostra todas as praças e avisa o motivo | Frontend Angular | `Mesas (UC10)` | 1 | ✅ Passou |
| 96 | Abre a comanda só para leitura, com as restrições e o aviso de privacidade (RN04) | Frontend Angular | `Salão agora (painel do Metre)` | 1 | ✅ Passou |
| 97 | Agrupa as mesas por praça, com garçom, pendências e restrição | Frontend Angular | `Salão agora (painel do Metre)` | 1 | ✅ Passou |
| 98 | Soma o que está em consumo e o que falta entregar | Frontend Angular | `Salão agora (painel do Metre)` | 1 | ✅ Passou |
| 99 | Conta os minutos desde o lançamento | Frontend Angular | `rótulos e regras de apresentação` | 1 | ✅ Passou |
| 100 | Escreve o tempo do jeito que o garçom lê | Frontend Angular | `rótulos e regras de apresentação` | 1 | ✅ Passou |
| 101 | Sugere a composição de 1 pessoas (infantil: false) como Solo | Frontend Angular | `rótulos e regras de apresentação` | 1 | ✅ Passou |
| 102 | Sugere a composição de 2 pessoas (infantil: false) como Casal | Frontend Angular | `rótulos e regras de apresentação` | 1 | ✅ Passou |
| 103 | Sugere a composição de 3 pessoas (infantil: false) como GrupoPequeno | Frontend Angular | `rótulos e regras de apresentação` | 1 | ✅ Passou |
| 104 | Sugere a composição de 3 pessoas (infantil: true) como Familia | Frontend Angular | `rótulos e regras de apresentação` | 1 | ✅ Passou |
| 105 | Sugere a composição de 4 pessoas (infantil: false) como GrupoPequeno | Frontend Angular | `rótulos e regras de apresentação` | 1 | ✅ Passou |
| 106 | Sugere a composição de 5 pessoas (infantil: false) como GrupoGrande | Frontend Angular | `rótulos e regras de apresentação` | 1 | ✅ Passou |
| 107 | Sugere a composição de 6 pessoas (infantil: true) como Familia | Frontend Angular | `rótulos e regras de apresentação` | 1 | ✅ Passou |
| 108 | Trata até as 17h como almoço, como as views de BI | Frontend Angular | `turno atual` | 1 | ✅ Passou |
| 109 | Usa a data do aparelho, e não UTC | Frontend Angular | `turno atual` | 1 | ✅ Passou |

## Cardápio e promoções

| # | Cenário | Camada | Origem | Casos | Resultado |
|---|---|---|---|---|---|
| 1 | Alterar imagem de item inexistente retorna 404 | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 2 | Alterar imagem troca e depois remove a foto | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 3 | Atualizar preco de item existente retorna 204 e o preco muda | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 4 | Atualizar preco de item inexistente retorna 404 com mensagem | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 5 | Cadastrar com categoria inexistente retorna 400 no formato padrao | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 6 | Cadastrar com dados invalidos em ingles retorna mensagens em ingles | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 7 | Cadastrar com dados validos retorna 201 com o item | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 8 | Cadastrar com endereco de foto guarda a imagem | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 9 | Cadastrar com endereco de foto invalido retorna 400 | Backend .NET | `CardapioControllerTests` | 3 | ✅ Passou |
| 10 | Cadastrar com nome vazio e preco zero retorna 400 com os dois erros em portugues | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 11 | Cadastrar como garcom retorna 403 | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 12 | Cadastrar sem login retorna 401 | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 13 | Cardapio digital sem login retorna 200 | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 14 | Marcar indisponivel item sai do cardapio digital mas continua na gestao | Backend .NET | `CardapioControllerTests` | 1 | ✅ Passou |
| 15 | Atende restricao alergia e outro nao filtram porque dependem do texto livre | Backend .NET | `ItemDoCardapioTests` | 2 | ✅ Passou |
| 16 | Atende restricao confere as flags do cardapio | Backend .NET | `ItemDoCardapioTests` | 7 | ✅ Passou |
| 17 | Atende restricao item sem flag nao atende restricao verificavel | Backend .NET | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 18 | Atualizar preco com valor invalido mantem preco anterior | Backend .NET | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 19 | Atualizar preco com valor valido altera preco | Backend .NET | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 20 | Criar com dados validos fica disponivel | Backend .NET | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 21 | Criar com flag repetida guarda uma vez so | Backend .NET | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 22 | Criar com preco zero ou negativo lanca excecao | Backend .NET | `ItemDoCardapioTests` | 2 | ✅ Passou |
| 23 | Flags dieteticas nao pode ser alterada de fora | Backend .NET | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 24 | Marcar disponibilidade falso torna item indisponivel | Backend .NET | `ItemDoCardapioTests` | 1 | ✅ Passou |
| 25 | Cardapio digital mostra o preco promocional so do item em promocao | Backend .NET | `PromocaoControllerTests` | 1 | ✅ Passou |
| 26 | Criar com dados invalidos retorna 400 | Backend .NET | `PromocaoControllerTests` | 3 | ✅ Passou |
| 27 | Criar com desconto fixo maior que o preco retorna 422 | Backend .NET | `PromocaoControllerTests` | 1 | ✅ Passou |
| 28 | Criar com item inexistente retorna 404 | Backend .NET | `PromocaoControllerTests` | 1 | ✅ Passou |
| 29 | Criar devolve preco com desconto de cada item e registra na auditoria | Backend .NET | `PromocaoControllerTests` | 1 | ✅ Passou |
| 30 | Garcom nao gerencia promocoes | Backend .NET | `PromocaoControllerTests` | 1 | ✅ Passou |
| 31 | Item lancado na comanda sai com o preco promocional | Backend .NET | `PromocaoControllerTests` | 1 | ✅ Passou |
| 32 | Remover desativa sem apagar e o preco volta ao normal | Backend .NET | `PromocaoControllerTests` | 1 | ✅ Passou |
| 33 | Aplicar desconto arredonda em centavos | Backend .NET | `PromocaoTests` | 3 | ✅ Passou |
| 34 | Aplicar desconto nunca deixa o item de graca | Backend .NET | `PromocaoTests` | 1 | ✅ Passou |
| 35 | Comanda guarda o preco promocional no pedido mas nunca acima do cardapio | Backend .NET | `PromocaoTests` | 1 | ✅ Passou |
| 36 | Criar com desconto invalido lanca excecao | Backend .NET | `PromocaoTests` | 3 | ✅ Passou |
| 37 | Criar com fim antes do inicio ou sem itens lanca excecao | Backend .NET | `PromocaoTests` | 1 | ✅ Passou |
| 38 | Desativada deixa de valer e nao desativa duas vezes | Backend .NET | `PromocaoTests` | 1 | ✅ Passou |
| 39 | Preco promocional com duas promocoes vale a de maior desconto | Backend .NET | `PromocaoTests` | 1 | ✅ Passou |
| 40 | Preco promocional sem promocao para o item ou fora do periodo e nulo | Backend .NET | `PromocaoTests` | 1 | ✅ Passou |
| 41 | Vigente em inclui os dois extremos | Backend .NET | `PromocaoTests` | 4 | ✅ Passou |
| 42 | Agrupa por categoria e mostra o preço promocional junto do cheio | Frontend Angular | `Cardápio digital (UC19)` | 1 | ✅ Passou |
| 43 | Avisa que o pedido é com o garçom e que alergia precisa ser confirmada | Frontend Angular | `Cardápio digital (UC19)` | 1 | ✅ Passou |
| 44 | Os filtros combinam entre si | Frontend Angular | `Cardápio digital (UC19)` | 1 | ✅ Passou |
| 45 | Sem resultado, explica e oferece limpar os filtros | Frontend Angular | `Cardápio digital (UC19)` | 1 | ✅ Passou |
| 46 | Cadastra o item com preço em reais, flags e foto | Frontend Angular | `Gestão do cardápio (UC05–UC09)` | 1 | ✅ Passou |
| 47 | Cadastro mostra dentro do painel as mensagens da API | Frontend Angular | `Gestão do cardápio (UC05–UC09)` | 1 | ✅ Passou |
| 48 | Cria a promoção com a prévia dos preços e os itens escolhidos | Frontend Angular | `Gestão do cardápio (UC05–UC09)` | 1 | ✅ Passou |
| 49 | Lista os itens com preço promocional e filtra pela busca | Frontend Angular | `Gestão do cardápio (UC05–UC09)` | 1 | ✅ Passou |
| 50 | Remover promoção pede confirmação e chama a API | Frontend Angular | `Gestão do cardápio (UC05–UC09)` | 1 | ✅ Passou |
| 51 | Tirar do ar oferece desfazer, que devolve a disponibilidade | Frontend Angular | `Gestão do cardápio (UC05–UC09)` | 1 | ✅ Passou |
| 52 | Calcula a prévia da promoção como o domínio: percentual, fixo e mínimo de R$ 0,01 | Frontend Angular | `contas do cardápio` | 1 | ✅ Passou |
| 53 | Lê valores em reais do jeito que o gerente digita | Frontend Angular | `contas do cardápio` | 1 | ✅ Passou |

## Alocação de garçons (programação linear)

| # | Cenário | Camada | Origem | Casos | Resultado |
|---|---|---|---|---|---|
| 1 | Ajuste com troca com as pracas cheias troca os dois lados e registra as duas mudancas | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 2 | Ajuste com troca com garcom de outra praca retorna 422 e nao muda nada | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 3 | Ajuste com troca com quem nao esta na praca de destino retorna 422 | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 4 | Ajuste com troca de quem ainda nao tem praca deixa o outro sem praca | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 5 | Ajuste com troca na mesma praca retorna 422 | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 6 | Ajuste depois de confirmado retorna 422 | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 7 | Ajuste para praca lotada retorna 422 | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 8 | Ajuste troca a praca e registra sugerida e escolhida | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 9 | Confirmacao de turno sem alocacao retorna 404 | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 10 | Confirmacao trava o turno e registra na auditoria | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 11 | Consulta como garcom ve o salao mas nao a faixa dos colegas | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 12 | Garcom nao gera sugestao mas consulta o turno | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 13 | Garcons como garcom retorna 403 | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 14 | Garcons para o metre traz so os ativos com id e nome | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 15 | Periodo inexistente na rota retorna 400 | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 16 | Sugestao com python fora responde 200 para alocacao manual e registra a falha | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 17 | Sugestao com quem nao e garcom retorna 422 | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 18 | Sugestao com resposta incoerente do python nao grava nada | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 19 | Sugestao depois de confirmado retorna 422 | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 20 | Sugestao envia ao python os fatores da RN03 e grava o resultado | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 21 | Sugestao explica a escolha com faixa e turnos sem mostrar valores | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 22 | Sugestao gerada de novo substitui a anterior nao confirmada | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 23 | Sugestao sem garcons retorna 400 | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 24 | Sugestao sem pracas cadastradas retorna 422 | Backend .NET | `AlocacaoControllerTests` | 1 | ✅ Passou |
| 25 | Ajustar antes de confirmar troca a praca | Backend .NET | `AlocacaoTests` | 1 | ✅ Passou |
| 26 | Ajustar depois de confirmar lanca excecao | Backend .NET | `AlocacaoTests` | 1 | ✅ Passou |
| 27 | Confirmar duas vezes lanca excecao | Backend .NET | `AlocacaoTests` | 1 | ✅ Passou |
| 28 | Criar comeca nao confirmada | Backend .NET | `AlocacaoTests` | 1 | ✅ Passou |
| 29 | Alto potencial sao as pracas acima da media das que tiveram movimento | Backend .NET | `RegraDeDistribuicaoTests` | 1 | ✅ Passou |
| 30 | Alto potencial sem historico nenhuma praca | Backend .NET | `RegraDeDistribuicaoTests` | 1 | ✅ Passou |
| 31 | Alto potencial todas iguais nenhuma praca | Backend .NET | `RegraDeDistribuicaoTests` | 1 | ✅ Passou |
| 32 | Faixas comparam com a media dos colegas do turno | Backend .NET | `RegraDeDistribuicaoTests` | 1 | ✅ Passou |
| 33 | Faixas diferenca pequena ainda e na media | Backend .NET | `RegraDeDistribuicaoTests` | 1 | ✅ Passou |
| 34 | Faixas quem nao faturou na janela fica sem historico e nao puxa a media | Backend .NET | `RegraDeDistribuicaoTests` | 1 | ✅ Passou |
| 35 | Pesos somam um | Backend .NET | `RegraDeDistribuicaoTests` | 1 | ✅ Passou |
| 36 | Turnos desde praca de alto potencial | Backend .NET | `RegraDeDistribuicaoTests` | 4 | ✅ Passou |
| 37 | Cada garcom recebe exatamente uma praca e as vagas sao respeitadas | Analítica Python | `test_alocacao` | 1 | ✅ Passou |
| 38 | Custos ficam entre zero e um | Analítica Python | `test_alocacao` | 1 | ✅ Passou |
| 39 | Pesos precisam somar um | Analítica Python | `test_alocacao` | 1 | ✅ Passou |
| 40 | Praca de alto potencial vai para quem faturou menos | Analítica Python | `test_alocacao` | 1 | ✅ Passou |
| 41 | Quem espera ha mais turnos ganha a praca boa no empate de faturamento | Analítica Python | `test_alocacao` | 1 | ✅ Passou |
| 42 | Sem vagas suficientes a alocacao e inviavel | Analítica Python | `test_alocacao` | 1 | ✅ Passou |
| 43 | So o peso da espera inverte a escolha | Analítica Python | `test_alocacao` | 1 | ✅ Passou |
| 44 | Escolhe o menor peso com desigualdade proxima da menor | Analítica Python | `test_calibracao` | 1 | ✅ Passou |
| 45 | Gini | Analítica Python | `test_calibracao` | 4 | ✅ Passou |
| 46 | Peso todo na espera reduz a espera maxima | Analítica Python | `test_calibracao` | 1 | ✅ Passou |
| 47 | Politicas de referencia ocupam todas as pracas sem estourar vagas | Analítica Python | `test_calibracao` | 1 | ✅ Passou |
| 48 | Rn 03 com peso calibrado distribui melhor que deixar sem regra | Analítica Python | `test_calibracao` | 1 | ✅ Passou |
| 49 | Simulacao e deterministica | Analítica Python | `test_calibracao` | 1 | ✅ Passou |
| 50 | Com mais presentes que vagas, explica o problema e bloqueia a geração | Frontend Angular | `Alocação (UC15, UC21, UC22)` | 1 | ✅ Passou |
| 51 | Começa pela presença, com todos marcados e as vagas à vista | Frontend Angular | `Alocação (UC15, UC21, UC22)` | 1 | ✅ Passou |
| 52 | Confirma o turno e trava os ajustes (UC21) | Frontend Angular | `Alocação (UC15, UC21, UC22)` | 1 | ✅ Passou |
| 53 | Erro da API aparece na tela | Frontend Angular | `Alocação (UC15, UC21, UC22)` | 1 | ✅ Passou |
| 54 | Gera a sugestão da RN03 com quem está presente | Frontend Angular | `Alocação (UC15, UC21, UC22)` | 1 | ✅ Passou |
| 55 | Mostra o motivo de cada garçom e marca a praça de alto movimento, sem valores | Frontend Angular | `Alocação (UC15, UC21, UC22)` | 1 | ✅ Passou |
| 56 | Na praça cheia, exige escolher com quem trocar e envia a troca (#140) | Frontend Angular | `Alocação (UC15, UC21, UC22)` | 1 | ✅ Passou |
| 57 | Sem o serviço de análise, todos ficam sem praça e a tela explica (D3) | Frontend Angular | `Alocação (UC15, UC21, UC22)` | 1 | ✅ Passou |
| 58 | Sem os fatores na resposta, não inventa motivo | Frontend Angular | `Alocação (UC15, UC21, UC22)` | 1 | ✅ Passou |

## Recomendação de pratos (ciência de dados)

| # | Cenário | Camada | Origem | Casos | Resultado |
|---|---|---|---|---|---|
| 1 | Comanda fechada retorna 422 | Backend .NET | `SugestoesComandaTests` | 1 | ✅ Passou |
| 2 | Comanda inexistente retorna 404 | Backend .NET | `SugestoesComandaTests` | 1 | ✅ Passou |
| 3 | Descarta id que o servico nao podia sugerir | Backend .NET | `SugestoesComandaTests` | 1 | ✅ Passou |
| 4 | Devolve os itens na ordem do servico com nome categoria e preco | Backend .NET | `SugestoesComandaTests` | 1 | ✅ Passou |
| 5 | Envia ao servico so o que pode ser oferecido | Backend .NET | `SugestoesComandaTests` | 1 | ✅ Passou |
| 6 | Gerente nao recebe sugestao retorna 403 | Backend .NET | `SugestoesComandaTests` | 1 | ✅ Passou |
| 7 | Restricao que o cardapio nao confere pede confirmacao com o cliente | Backend .NET | `SugestoesComandaTests` | 3 | ✅ Passou |
| 8 | Sem item valido na comanda nem chama o servico | Backend .NET | `SugestoesComandaTests` | 1 | ✅ Passou |
| 9 | Servico fora responde 200 com lista vazia e sem travar a comanda | Backend .NET | `SugestoesComandaTests` | 1 | ✅ Passou |
| 10 | Alocacao com pesos que nao somam um retorna 422 | Analítica Python | `test_api_analitica` | 1 | ✅ Passou |
| 11 | Alocacao devolve uma praca para cada garcom | Analítica Python | `test_api_analitica` | 1 | ✅ Passou |
| 12 | Alocacao sem vagas suficientes retorna 422 | Analítica Python | `test_api_analitica` | 1 | ✅ Passou |
| 13 | Recomendacao aceita historico informado pelo backend | Analítica Python | `test_api_analitica` | 1 | ✅ Passou |
| 14 | Recomendacao filtra pelos itens disponiveis | Analítica Python | `test_api_analitica` | 1 | ✅ Passou |
| 15 | Recomendacao informa o motivo de usar o simulado | Analítica Python | `test_api_analitica` | 1 | ✅ Passou |
| 16 | Recomendacao usa o historico do banco quando disponivel | Analítica Python | `test_api_analitica` | 1 | ✅ Passou |
| 17 | Recomendacao usa o historico simulado quando nenhum e informado | Analítica Python | `test_api_analitica` | 1 | ✅ Passou |
| 18 | Agrupa os itens por comanda | Analítica Python | `test_historico_banco` | 1 | ✅ Passou |
| 19 | Configuracao usa valores padrao do ambiente local | Analítica Python | `test_historico_banco` | 1 | ✅ Passou |
| 20 | Filtra a janela de historico | Analítica Python | `test_historico_banco` | 1 | ✅ Passou |
| 21 | Sem senha no ambiente o banco fica desligado | Analítica Python | `test_historico_banco` | 1 | ✅ Passou |
| 22 | Senha nao aparece na representacao da engine | Analítica Python | `test_historico_banco` | 1 | ✅ Passou |
| 23 | Usuario analitico le a view | Analítica Python | `test_historico_banco` | 1 | ✅ Passou |
| 24 | Usuario analitico nao le tabelas nem grava | Analítica Python | `test_historico_banco` | 4 | ✅ Passou |
| 25 | Banco fora do ar nao derruba a recomendacao | Analítica Python | `test_provedor_modelo` | 1 | ✅ Passou |
| 26 | Historico real pequeno demais usa o simulado | Analítica Python | `test_provedor_modelo` | 1 | ✅ Passou |
| 27 | Historico real suficiente treina com o banco | Analítica Python | `test_provedor_modelo` | 1 | ✅ Passou |
| 28 | Invalidar forca nova leitura | Analítica Python | `test_provedor_modelo` | 1 | ✅ Passou |
| 29 | O modelo fica em cache e e recalculado depois do prazo | Analítica Python | `test_provedor_modelo` | 1 | ✅ Passou |
| 30 | Sem banco configurado usa o simulado e diz o motivo | Analítica Python | `test_provedor_modelo` | 1 | ✅ Passou |
| 31 | Descarta regras com lift abaixo de um | Analítica Python | `test_recomendacao` | 1 | ✅ Passou |
| 32 | Historico sem combinacoes nao gera regra | Analítica Python | `test_recomendacao` | 1 | ✅ Passou |
| 33 | Nao sugere item que ja esta na comanda | Analítica Python | `test_recomendacao` | 1 | ✅ Passou |
| 34 | Recupera a combinacao plantada no simulador | Analítica Python | `test_recomendacao` | 1 | ✅ Passou |
| 35 | Respeita a lista de itens disponiveis | Analítica Python | `test_recomendacao` | 1 | ✅ Passou |
| 36 | Respeita o limite de sugestoes | Analítica Python | `test_recomendacao` | 1 | ✅ Passou |
| 37 | Resultado e o mesmo para a mesma semente | Analítica Python | `test_recomendacao` | 1 | ✅ Passou |

## BI e índice de desempenho

| # | Cenário | Camada | Origem | Casos | Resultado |
|---|---|---|---|---|---|
| 1 | Periodo invalido ou longo demais retorna 400 | Backend .NET | `IndicadoresControllerTests` | 2 | ✅ Passou |
| 2 | Periodo sem filtro sao os ultimos 30 dias com fim inclusivo | Backend .NET | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 3 | Ranking com avaliacoes usa a nota de quem tem o minimo e esconde a dos outros | Backend .NET | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 4 | Ranking o garcom ve so a propria posicao sem nome dos colegas | Backend .NET | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 5 | Ranking o gerente ve todos | Backend .NET | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 6 | Relatorio de avaliacoes sem nenhuma avaliacao nao divide por zero | Backend .NET | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 7 | Relatorio de avaliacoes traz media e as cinco faixas | Backend .NET | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 8 | Relatorio de garcons calcula medias e totais com nome | Backend .NET | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 9 | Relatorio de horarios traz hora e dia da semana com nome | Backend .NET | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 10 | Relatorio de pracas mostra todas as pracas inclusive sem movimento | Backend .NET | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 11 | Relatorio do cardapio agrupa por categoria com participacao | Backend .NET | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 12 | Relatorios sao do gerente e registram a consulta | Backend .NET | `IndicadoresControllerTests` | 1 | ✅ Passou |
| 13 | Abaixo do minimo nota nao aparece e o garcom recebe a media geral | Backend .NET | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 14 | Com avaliacoes pesos viram 40 30 30 | Backend .NET | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 15 | Compara por turno quem trabalhou mais nao ganha so por isso | Backend .NET | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 16 | Empate no indice divide a posicao | Backend .NET | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 17 | Media bayesiana poucas notas altas nao passam muitas notas quase altas | Backend .NET | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 18 | Mesma venda e mesmas mesas avaliacao desempata | Backend .NET | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 19 | Ninguem com o minimo avaliacao sai do indice como antes | Backend .NET | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 20 | Quem lidera faturamento e mesas por turno faz 100 | Backend .NET | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 21 | Sem avaliacoes mesas atendidas pesam metade | Backend .NET | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 22 | Sem turnos fica fora do ranking | Backend .NET | `IndiceDeDesempenhoTests` | 1 | ✅ Passou |
| 23 | Avaliacoes por garcom somam as notas das comandas dele pelo dia do fechamento | Backend .NET | `RepositorioIndicadoresTests` | 1 | ✅ Passou |
| 24 | Faturamento medio por praca inclui praca sem movimento | Backend .NET | `RepositorioIndicadoresTests` | 1 | ✅ Passou |
| 25 | Faturamento por hora e por dia da semana usam o horario de brasilia | Backend .NET | `RepositorioIndicadoresTests` | 1 | ✅ Passou |
| 26 | Faturamento por turno do garcom e a media dos turnos no intervalo com fim exclusivo | Backend .NET | `RepositorioIndicadoresTests` | 1 | ✅ Passou |
| 27 | Indicadores por garcom contam turnos mesas e minutos de atendimento | Backend .NET | `RepositorioIndicadoresTests` | 1 | ✅ Passou |
| 28 | Indicadores por item trazem categoria quantidade e faturamento | Backend .NET | `RepositorioIndicadoresTests` | 1 | ✅ Passou |
| 29 | Indicadores por praca somam comandas e turnos com movimento | Backend .NET | `RepositorioIndicadoresTests` | 1 | ✅ Passou |
| 30 | Destaca como alto potencial a praça acima da média, pela mesma regra da RN03 | Frontend Angular | `Análises (UC16, UC17)` | 1 | ✅ Passou |
| 31 | Monta o mapa de calor por praça e hora, com o pico | Frontend Angular | `Análises (UC16, UC17)` | 1 | ✅ Passou |
| 32 | Mostra a média das avaliações e a distribuição, sem ligar nota a garçom (RF25) | Frontend Angular | `Análises (UC16, UC17)` | 1 | ✅ Passou |
| 33 | Pede os seis relatórios com o mesmo período e mostra os totais | Frontend Angular | `Análises (UC16, UC17)` | 1 | ✅ Passou |
| 34 | Período sem comanda fechada explica em vez de mostrar zeros | Frontend Angular | `Análises (UC16, UC17)` | 1 | ✅ Passou |
| 35 | Ranking com os detalhes do garçom e itens ordenados pelo faturamento | Frontend Angular | `Análises (UC16, UC17)` | 1 | ✅ Passou |
| 36 | Sem avaliação no período, diz isso em vez de mostrar média zero | Frontend Angular | `Análises (UC16, UC17)` | 1 | ✅ Passou |
| 37 | Trocar o período pede os relatórios de novo | Frontend Angular | `Análises (UC16, UC17)` | 1 | ✅ Passou |
| 38 | Abaixo do mínimo, explica que entrou a média do restaurante em vez de mostrar a nota | Frontend Angular | `Desempenho (UC17)` | 1 | ✅ Passou |
| 39 | Com avaliações, mostra a nota do próprio garçom e o peso dela | Frontend Angular | `Desempenho (UC17)` | 1 | ✅ Passou |
| 40 | Mostra a posição do próprio garçom e o que entra no índice | Frontend Angular | `Desempenho (UC17)` | 1 | ✅ Passou |
| 41 | Sem turnos no período, explica em vez de mostrar tela vazia | Frontend Angular | `Desempenho (UC17)` | 1 | ✅ Passou |
| 42 | Inclui o dia de hoje em cada opção | Frontend Angular | `datas do período` | 1 | ✅ Passou |

## LGPD, auditoria e segurança

| # | Cenário | Camada | Origem | Casos | Resultado |
|---|---|---|---|---|---|
| 1 | Cardapio registra cadastro preco anterior e novo e disponibilidade | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 2 | Comanda registra ciclo completo sem codigo de acesso nem restricao | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 3 | Contas registram criacao edicao com papel anterior e novo sem nome nem email | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 4 | Login com email inexistente nao guarda o email digitado | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 5 | Login com senha errada registra falha com a conta e o motivo sem a senha | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 6 | Login com sucesso registra ator papel e ip | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 7 | Login com x forwarded for de quem nao e proxy ignora o cabecalho | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 8 | Login de conta inativa registra o motivo | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 9 | Login quinta senha errada registra que bloqueou e depois o motivo conta bloqueada | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 10 | Logoff registra o ator | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 11 | Operacao recusada nao gera registro de sucesso | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 12 | Salao registra cadastro e edicao de praca e mesa | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 13 | Segundo fator registra vinculacao confirmacao e recusa sem codigo nem segredo | Backend .NET | `AuditoriaTests` | 1 | ✅ Passou |
| 14 | Login cinco senhas erradas bloqueia até a senha certa com a mesma mensagem | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 15 | Login com senha errada ou email inexistente retorna 401 com a mesma mensagem | Backend .NET | `AutenticacaoControllerTests` | 2 | ✅ Passou |
| 16 | Login com sucesso zera a contagem | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 17 | Login garcom com credenciais validas recebe token de acesso direto | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 18 | Login gerente nao recebe token de acesso e precisa configurar o autenticador | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 19 | Login sem email e senha retorna 400 | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 20 | Login usuario inativo retorna 401 | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 21 | Logoff invalida o token no servidor | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 22 | Segundo fator codigo certo zera a contagem | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 23 | Segundo fator com codigo errado retorna 401 | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 24 | Segundo fator fluxo completo libera acesso e o segredo nao pode ser gerado de novo | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 25 | Segundo fator logar de novo nao da mais palpites de codigo | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 26 | Segundo fator quinto codigo errado bloqueia com mensagem propria | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 27 | Token do segundo fator nao serve como token de acesso | Backend .NET | `AutenticacaoControllerTests` | 1 | ✅ Passou |
| 28 | Elimina so o vencido respeitando a folga do trigger e registra a eliminacao | Backend .NET | `EliminacaoAuditoriaTests` | 1 | ✅ Passou |
| 29 | Sem nada vencido nao apaga nem registra | Backend .NET | `EliminacaoAuditoriaTests` | 1 | ✅ Passou |
| 30 | Auditoria atras do proxy registra o ip do navegador | Backend .NET | `LimiteDeRequisicoesTests` | 1 | ✅ Passou |
| 31 | Consulta do cliente alem da cota retorna 429 | Backend .NET | `LimiteDeRequisicoesTests` | 1 | ✅ Passou |
| 32 | Cota e por ip outro endereco nao e afetado | Backend .NET | `LimiteDeRequisicoesTests` | 1 | ✅ Passou |
| 33 | Login alem da cota do minuto retorna 429 com mensagem e espera | Backend .NET | `LimiteDeRequisicoesTests` | 1 | ✅ Passou |
| 34 | Login alem da cota responde no idioma pedido | Backend .NET | `LimiteDeRequisicoesTests` | 1 | ✅ Passou |
| 35 | Rotas com login nao entram no limite | Backend .NET | `LimiteDeRequisicoesTests` | 1 | ✅ Passou |
| 36 | Cadastrar com dados invalidos retorna 400 com todos os erros | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 37 | Cadastrar com dados validos retorna 201 e o usuario consegue entrar | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 38 | Cadastrar com email ja usado em outra caixa retorna 422 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 39 | Cadastrar com senha acima do limite do b crypt retorna 400 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 40 | Editar com email de outra conta retorna 422 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 41 | Editar o proprio papel retorna 422 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 42 | Editar trocando o papel derruba o token antigo e novo login vem com o novo papel | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 43 | Gestao de contas como garcom retorna 403 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 44 | Gestao de contas sem login retorna 401 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 45 | Inativar a propria conta retorna 422 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 46 | Inativar bloqueia token e login e reativar devolve o acesso | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 47 | Listar traz contas ativas e inativas | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 48 | Obter inexistente retorna 404 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 49 | Redefinir senha com senha curta retorna 400 e senha antiga continua valendo | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 50 | Redefinir senha da propria conta retorna 422 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 51 | Redefinir senha de conta inexistente retorna 404 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 52 | Redefinir senha e zerar segundo fator como garcom retorna 403 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 53 | Redefinir senha troca a senha e derruba as sessoes abertas | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 54 | Reiniciar segundo fator da propria conta retorna 422 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 55 | Reiniciar segundo fator de quem nao usa segundo fator retorna 422 | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 56 | Reiniciar segundo fator faz o proximo login pedir a configuracao de novo | Backend .NET | `UsuarioControllerTests` | 1 | ✅ Passou |
| 57 | Acesso completo zera a contagem e libera a conta | Backend .NET | `UsuarioTests` | 1 | ✅ Passou |
| 58 | Atualizar dados mantendo papel nao encerra as sessoes | Backend .NET | `UsuarioTests` | 1 | ✅ Passou |
| 59 | Atualizar dados trocando papel encerra as sessoes | Backend .NET | `UsuarioTests` | 1 | ✅ Passou |
| 60 | Criar normaliza email e comeca ativo | Backend .NET | `UsuarioTests` | 1 | ✅ Passou |
| 61 | Inativar encerra as sessoes e reativar devolve o acesso | Backend .NET | `UsuarioTests` | 1 | ✅ Passou |
| 62 | Redefinir senha desfaz o bloqueio | Backend .NET | `UsuarioTests` | 1 | ✅ Passou |
| 63 | Tentativa falha a quinta bloqueia por quinze minutos e avisa que bloqueou | Backend .NET | `UsuarioTests` | 1 | ✅ Passou |
| 64 | Tentativa falha depois do bloqueio exige outros cinco erros | Backend .NET | `UsuarioTests` | 1 | ✅ Passou |
| 65 | Tentativa falha durante o bloqueio nao empurra o fim | Backend .NET | `UsuarioTests` | 1 | ✅ Passou |
| 66 | Tentativa falha quatro seguidas ainda nao bloqueia | Backend .NET | `UsuarioTests` | 1 | ✅ Passou |
| 67 | Exibe o nome do sistema no cabeçalho | Frontend Angular | `App` | 1 | ✅ Passou |
| 68 | O coordenador vê só o cardápio, onde ficam também as promoções | Frontend Angular | `App` | 1 | ✅ Passou |
| 69 | O garçom vê só as telas dele | Frontend Angular | `App` | 1 | ✅ Passou |
| 70 | O gerente vê a gestão, mas não a alocação do metre | Frontend Angular | `App` | 1 | ✅ Passou |
| 71 | O metre vê a alocação e o salão, e não as telas do garçom | Frontend Angular | `App` | 1 | ✅ Passou |
| 72 | Sem login, não mostra menu nem botão de sair | Frontend Angular | `App` | 1 | ✅ Passou |
| 73 | Cadastra a conta com papel e senha inicial | Frontend Angular | `Gestão de usuários (UC04)` | 1 | ✅ Passou |
| 74 | Erro da API aparece dentro do painel de cadastro | Frontend Angular | `Gestão de usuários (UC04)` | 1 | ✅ Passou |
| 75 | Inativar pede confirmação e explica que o acesso cai na hora (RF18) | Frontend Angular | `Gestão de usuários (UC04)` | 1 | ✅ Passou |
| 76 | Lista os ativos por padrão e filtra pela situação | Frontend Angular | `Gestão de usuários (UC04)` | 1 | ✅ Passou |
| 77 | Mostra a verificação em duas etapas só para quem usa (RF16) | Frontend Angular | `Gestão de usuários (UC04)` | 1 | ✅ Passou |
| 78 | Na própria conta não oferece inativar, senha nem zerar as duas etapas | Frontend Angular | `Gestão de usuários (UC04)` | 1 | ✅ Passou |
| 79 | Reativar não pede confirmação: é ação sem perda | Frontend Angular | `Gestão de usuários (UC04)` | 1 | ✅ Passou |
| 80 | Redefine a senha avisando que as sessões caem (#141) | Frontend Angular | `Gestão de usuários (UC04)` | 1 | ✅ Passou |
| 81 | Zerar as duas etapas só aparece para quem já configurou, e explica o efeito (RN07) | Frontend Angular | `Gestão de usuários (UC04)` | 1 | ✅ Passou |
| 82 | Com acesso liberado, vai para a tela inicial do papel | Frontend Angular | `Login` | 1 | ✅ Passou |
| 83 | Mostra a mensagem da API quando a senha está errada | Frontend Angular | `Login` | 1 | ✅ Passou |
| 84 | Não chama a API com o formulário vazio | Frontend Angular | `Login` | 1 | ✅ Passou |
| 85 | Não segue um "voltar" que aponta para fora do sistema | Frontend Angular | `Login` | 1 | ✅ Passou |
| 86 | Avisa que a chave aparece uma única vez (RN07) | Frontend Angular | `Segundo fator (UC02)` | 1 | ✅ Passou |
| 87 | Desenha o QR code da URI otpauth no próprio navegador | Frontend Angular | `Segundo fator (UC02)` | 1 | ✅ Passou |
| 88 | Erro ao gerar a chave aparece na tela em vez de deixar o cartão vazio | Frontend Angular | `Segundo fator (UC02)` | 1 | ✅ Passou |
| 89 | Mantém a chave digitável como contingência de quem não consegue apontar a câmera | Frontend Angular | `Segundo fator (UC02)` | 1 | ✅ Passou |
| 90 | Nos acessos seguintes pede só o código, sem chave nem QR | Frontend Angular | `Segundo fator (UC02)` | 1 | ✅ Passou |
| 91 | Confirmar o segundo fator libera o acesso e descarta o token temporário | Frontend Angular | `SessaoService` | 1 | ✅ Passou |
| 92 | Login do garçom libera o acesso direto e guarda nome, papel e validade | Frontend Angular | `SessaoService` | 1 | ✅ Passou |
| 93 | Login do gerente no primeiro acesso pede a configuração do autenticador, sem liberar acesso | Frontend Angular | `SessaoService` | 1 | ✅ Passou |
| 94 | Lê a validade do JWT em milissegundos e trata token malformado como vencido | Frontend Angular | `SessaoService` | 1 | ✅ Passou |
| 95 | Restaura a sessão guardada ao recarregar a página | Frontend Angular | `SessaoService` | 1 | ✅ Passou |
| 96 | Sair encerra a sessão no navegador mesmo se a API falhar | Frontend Angular | `SessaoService` | 1 | ✅ Passou |
| 97 | Sessão guardada com token vencido é descartada | Frontend Angular | `SessaoService` | 1 | ✅ Passou |
| 98 | 401 do próprio login (senha errada) não mexe na navegação | Frontend Angular | `autenticacaoInterceptor` | 1 | ✅ Passou |
| 99 | 401 fora do login encerra a sessão e leva ao login | Frontend Angular | `autenticacaoInterceptor` | 1 | ✅ Passou |
| 100 | Envia o token nas chamadas à API | Frontend Angular | `autenticacaoInterceptor` | 1 | ✅ Passou |
| 101 | Nunca envia o token para outro endereço | Frontend Angular | `autenticacaoInterceptor` | 1 | ✅ Passou |
| 102 | A raiz leva o Coordenador para /cardapio | Frontend Angular | `guardas de rota` | 1 | ✅ Passou |
| 103 | A raiz leva o Garcom para /comandas | Frontend Angular | `guardas de rota` | 1 | ✅ Passou |
| 104 | A raiz leva o Gerente para /analises | Frontend Angular | `guardas de rota` | 1 | ✅ Passou |
| 105 | A raiz leva o Metre para /alocacao | Frontend Angular | `guardas de rota` | 1 | ✅ Passou |
| 106 | Com o papel certo, deixa entrar | Frontend Angular | `guardas de rota` | 1 | ✅ Passou |
| 107 | Com outro papel, manda para sem permissão | Frontend Angular | `guardas de rota` | 1 | ✅ Passou |
| 108 | Sem login, manda para o login lembrando a tela pedida | Frontend Angular | `guardas de rota` | 1 | ✅ Passou |

## Banco de dados (views, triggers e restrições)

| # | Cenário | Camada | Origem | Casos | Resultado |
|---|---|---|---|---|---|
| 1 | Comanda nao pode ter status e data de fechamento contraditorios | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 2 | Data turno e hora seguem o horario de brasilia | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 3 | Desempenho do garcom conta mesas distintas por turno | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 4 | Eliminacao por prazo apaga so o que passou de 6 meses | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 5 | Faturamento da comanda ignora itens cancelados e comandas abertas | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 6 | Faturamento da praca por turno soma as comandas do turno | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 7 | Faturamento medio da praca e calculado por turno e inclui praca sem movimento | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 8 | Faturamento por item traz a categoria do cardapio | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 9 | Itens por comanda juntam linhas do mesmo item e descartam cancelados | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 10 | Motivo de cancelamento existe so em item cancelado | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 11 | Promocao nao aceita periodo invertido nem percentual de 100 | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 12 | Registro de auditoria aceita insercao mas nunca alteracao | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 13 | Registro de auditoria dentro do prazo de retencao nao pode ser apagado | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |
| 14 | Views nao expoem dado pessoal | Backend .NET | `ObjetosDoBancoTests` | 1 | ✅ Passou |

## Integração backend ↔ Python

| # | Cenário | Camada | Origem | Casos | Resultado |
|---|---|---|---|---|---|
| 1 | Servico real da a praca boa para quem faturou menos | Backend .NET | `ContratoComPythonTests` | 1 | ✅ Passou |
| 2 | Servico real sugere o arroz de coco para quem pediu moqueca | Backend .NET | `ContratoComPythonTests` | 1 | ✅ Passou |
| 3 | Configuracao invalida impede a subida da api | Backend .NET | `RegistroServicoAnaliticoTests` | 2 | ✅ Passou |
| 4 | Usa o cliente http com endereco e tempo limite da configuracao | Backend .NET | `RegistroServicoAnaliticoTests` | 1 | ✅ Passou |
| 5 | Alocacao envia garcons pracas e pesos em snake case e le as designacoes | Backend .NET | `ServicoAnaliticoHttpTests` | 1 | ✅ Passou |
| 6 | Cancelamento pedido por quem chamou nao eh tratado como falha do servico | Backend .NET | `ServicoAnaliticoHttpTests` | 1 | ✅ Passou |
| 7 | Envia o contrato em snake case e le os ids na ordem | Backend .NET | `ServicoAnaliticoHttpTests` | 1 | ✅ Passou |
| 8 | Resposta invalida vira servico indisponivel | Backend .NET | `ServicoAnaliticoHttpTests` | 3 | ✅ Passou |
| 9 | Resposta lenta estoura o tempo limite e vira servico indisponivel | Backend .NET | `ServicoAnaliticoHttpTests` | 1 | ✅ Passou |
| 10 | Servico fora do ar vira servico indisponivel | Backend .NET | `ServicoAnaliticoHttpTests` | 1 | ✅ Passou |
| 11 | Status de erro vira servico indisponivel | Backend .NET | `ServicoAnaliticoHttpTests` | 1 | ✅ Passou |

## Arquitetura e infraestrutura

| # | Cenário | Camada | Origem | Casos | Resultado |
|---|---|---|---|---|---|
| 1 | Application nao depende de infrastructure nem da api | Backend .NET | `ArquiteturaTests` | 1 | ✅ Passou |
| 2 | Camada base nao depende de outra camada | Backend .NET | `ArquiteturaTests` | 3 | ✅ Passou |
| 3 | Infrastructure depende apenas do domain | Backend .NET | `ArquiteturaTests` | 1 | ✅ Passou |
| 4 | Frontend de desenvolvimento esta liberado | Backend .NET | `CorsTests` | 1 | ✅ Passou |
| 5 | Origem desconhecida nao recebe liberacao | Backend .NET | `CorsTests` | 1 | ✅ Passou |
| 6 | Documentacao fora do ambiente de desenvolvimento nao fica exposta | Backend .NET | `DocumentacaoOpenApiTests` | 2 | ✅ Passou |
| 7 | Health responde ok quando a api sobe | Backend .NET | `HealthCheckTests` | 1 | ✅ Passou |
| 8 | Algoritmo nao importa fastapi | Analítica Python | `test_arquitetura` | 2 | ✅ Passou |
| 9 | Health responde healthy | Analítica Python | `test_health` | 1 | ✅ Passou |
