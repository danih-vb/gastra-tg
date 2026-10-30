# Avaliação heurística de Nielsen

Issues #101 (telas do Gerente e do Cliente) e #102 (telas do Garçom e do Metre). O resultado vai para a seção 4.4.6
do documento, cujo método já está escrito: este arquivo é onde a avaliação é feita e registrada.

## Como avaliar

- **Avaliação cruzada:** quem desenhou o protótipo não avalia as próprias telas. Pedro avalia as do Gerente e do
  Cliente (protótipo do Daniel, #101); Daniel avalia as do Garçom e do Metre (protótipo do Pedro, #102).
- **O que se avalia:** as telas em Angular, no sistema rodando (`infra/resetar-ambiente.sh`), e não o protótipo.
  Garçom e Cliente no celular; Gerente e Metre no computador.
- **Cada um sozinho primeiro.** Nielsen recomenda que os avaliadores não conversem até terminar, para um não
  influenciar o outro. Depois, juntar as duas listas, tirar repetidos e discutir a severidade.
- **Duas passadas por tela:** a primeira para entender o fluxo, fazendo a tarefa indicada; a segunda percorrendo
  as dez heurísticas uma a uma.
- **Um problema por linha**, mesmo que apareça em várias telas. Se viola duas heurísticas, escolher a principal.

## As dez heurísticas

| Nº | Heurística | Pergunta para fazer em cada tela |
|---|---|---|
| H1 | Visibilidade do estado do sistema | Depois de cada toque, fica claro o que aconteceu (salvou, carregando, erro)? |
| H2 | Correspondência entre o sistema e o mundo real | Os termos são os do salão (mesa, praça, comanda, conta), e não os do sistema? |
| H3 | Controle e liberdade do usuário | Dá para desfazer ou sair de um passo errado sem perder o que foi feito? |
| H4 | Consistência e padrões | O mesmo botão, cor e posição significam a mesma coisa em todas as telas? |
| H5 | Prevenção de erros | O sistema evita o erro antes (confirmação, desabilitar o que não pode), em vez de só avisar depois? |
| H6 | Reconhecimento em vez de memorização | A informação necessária está na tela, ou o usuário precisa lembrar de outra tela? |
| H7 | Flexibilidade e eficiência de uso | Quem usa todo dia tem atalhos (sugestão, itens frequentes), sem atrapalhar quem é novo? |
| H8 | Estética e design minimalista | Tem informação na tela que não serve para a tarefa e compete com a que serve? |
| H9 | Ajuda para reconhecer, diagnosticar e corrigir erros | A mensagem de erro diz, em português claro, o que houve e o que fazer? |
| H10 | Ajuda e documentação | Quando a tela não é óbvia, há uma explicação curta no lugar certo? |

## Severidade

| Nota | Significado | O que fazer |
|---|---|---|
| 0 | Não é problema de usabilidade | Não registrar |
| 1 | Só estético | Corrigir se sobrar tempo |
| 2 | Menor: atrapalha, mas o usuário contorna | Corrigir ou justificar |
| 3 | Grave: o usuário erra ou demora muito | Corrigir antes da entrega |
| 4 | Catastrófico: impede a tarefa | Corrigir antes da entrega |

A nota considera três coisas: quantos usuários vão encontrar o problema, se conseguem contorná-lo e se ele se
repete todo dia (um problema na tela de lançar item pesa mais que um na tela de usuários).

## Telas e tarefas

### #101: Gerente e Cliente (avaliador: Pedro)

| Tela | Rota | Tarefa para fazer durante a avaliação |
|---|---|---|
| Login | `/acesso/login` | Entrar como Gerente; errar a senha uma vez |
| Segundo fator | `/acesso/segundo-fator` | Vincular o autenticador e entrar |
| Análises | `/analises` | Descobrir o prato mais vendido no jantar e o perfil de consumo mais comum |
| Cardápio | `/cardapio` | Criar um prato vegano com promoção e depois desativá-lo |
| Salão | `/salao` | Criar uma mesa e mudar a quantidade de vagas de uma praça |
| Usuários | `/usuarios` | Criar um garçom, trocar o papel dele e inativá-lo |
| Início do cliente | `/cliente` | Chegar ao cardápio |
| Cardápio digital | `/cliente/cardapio` | Achar uma sobremesa sem lactose |
| Conta do cliente | `/cliente/conta/:codigo` | Entrar pelo QR code da comanda, conferir o total e avaliar o atendimento depois do fechamento |

### #102: Garçom e Metre (avaliador: Daniel)

| Tela | Rota | Tarefa para fazer durante a avaliação |
|---|---|---|
| Mesas | `/comandas` | Abrir uma mesa para quatro pessoas com uma criança |
| Comanda | `/comandas/:id` | Lançar três itens, usar uma sugestão e cancelar um item |
| A entregar | `/a-entregar` | Marcar dois itens como entregues |
| Fechar conta | `/comandas/:id/fechar` | Fechar a conta retirando a taxa de serviço |
| Seu desempenho | `/desempenho` | Entender por que o índice está naquele valor |
| Alocação | `/alocacao` | Pedir a sugestão, trocar um garçom e confirmar |
| Salão agora | `/salao-agora` | Descobrir qual praça está mais cheia |

## Registro dos problemas

| Nº | Avaliador | Tela | Heurística | Problema | Severidade | Correção ou justificativa | Issue/PR |
|---|---|---|---|---|---|---|---|
| 1 | | | | | | | |

## Cobertura por tela

Marcar, para cada tela e heurística: **ok**, o **número do problema** encontrado, ou **n/a** quando a heurística
não se aplica. Serve para mostrar que todas as heurísticas foram olhadas em todas as telas, e não só as que deram
problema.

| Tela | H1 | H2 | H3 | H4 | H5 | H6 | H7 | H8 | H9 | H10 |
|---|---|---|---|---|---|---|---|---|---|---|
| Login | | | | | | | | | | |
| Segundo fator | | | | | | | | | | |
| Análises | | | | | | | | | | |
| Cardápio | | | | | | | | | | |
| Salão | | | | | | | | | | |
| Usuários | | | | | | | | | | |
| Início do cliente | | | | | | | | | | |
| Cardápio digital | | | | | | | | | | |
| Conta do cliente | | | | | | | | | | |
| Mesas | | | | | | | | | | |
| Comanda | | | | | | | | | | |
| A entregar | | | | | | | | | | |
| Fechar conta | | | | | | | | | | |
| Seu desempenho | | | | | | | | | | |
| Alocação | | | | | | | | | | |
| Salão agora | | | | | | | | | | |

## Resumo para a seção 4.4.6

| Severidade | Encontrados | Corrigidos | Aceitos com justificativa |
|---|---|---|---|
| 4 | | | |
| 3 | | | |
| 2 | | | |
| 1 | | | |
| **Total** | | | |

Separar também por heurística (quais concentraram mais problemas) e escolher dois ou três exemplos com a correção
feita, de preferência com o antes e o depois da tela.
