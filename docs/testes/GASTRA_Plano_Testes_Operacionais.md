# Plano de testes operacionais

Issues #208 (roteiro ponta a ponta), #211 (plano) e #177 (máquina limpa). O resultado alimenta a seção 5.6 do
documento e a medição dos RNF01 e RNF02, que os testes automatizados não alcançam.

Diferente dos testes automatizados, aqui não há dublê: o navegador fala com a API, a API com o MySQL e com o serviço
analítico, todos em contêineres, como na entrega.

## Como executar

1. **Preparar:** dentro de `infra/`, `./resetar-ambiente.sh`. Toda rodada começa do mesmo estado: histórico de
   70 dias, turno de hoje montado e **não confirmado**, 6 mesas abertas.
2. **Contas:** as do semeador (`roberta.gerente@`, `marcos.coordenador@`, `helena.metre@`, `carla@`, `joao@`...
   `@gastra.local`), todas com a senha de `GASTRA_SEMEADOR_SENHA`. Não anote senha neste arquivo.
3. **Aparelhos:** garçom e cliente no celular, pela mesma rede (`http://IP-DA-MÁQUINA:4200`); Metre e Gerente
   no computador.
4. **Registrar:** em cada passo, **P** (passou) ou **F** (falhou). Toda falha vira uma linha na tabela de
   defeitos, com a issue aberta para ela.
5. **Quem:** um executa, o outro anota e cronometra. Trocar de papel na rodada seguinte.

## A. Máquina limpa (#177)

Num computador que **não** tenha .NET, Node nem Python instalados, só o Docker. Quem executa é quem não montou o
compose, seguindo apenas o `README.md` e o `infra/README.md`, sem ajuda.

| Nº | Passo | Resultado esperado | P/F | Observação |
|---|---|---|---|---|
| A1 | Clonar o repositório | Clona sem erro | | |
| A2 | Criar o `infra/.env` a partir do `.env.example` | A documentação diz o que pôr em cada variável | | |
| A3 | `docker compose --profile app up -d --build` | Sobe sem erro | | Tempo do primeiro build: |
| A4 | `docker compose --profile app ps` | Quatro serviços `healthy` | | |
| A5 | Abrir `http://localhost:4200` e entrar com `ADMIN_EMAIL` | Tela do segundo fator, com o QR code | | |
| A6 | Abrir pelo IP da máquina num celular da mesma rede | A tela de login abre no celular | | |

Cada tropeço (porta ocupada, WSL2, passo faltando na documentação) vira correção na documentação.

## B. Roteiro ponta a ponta, por papel (#208)

### Gerente

| Nº | Passo | Resultado esperado | P/F | Observação |
|---|---|---|---|---|
| G1 | Entrar com `roberta.gerente@` | Pede o segundo fator; sem ele, não entra | | |
| G2 | Vincular o autenticador pelo QR e digitar o código | Entra e cai na tela de análises | | |
| G3 | Abrir Análises | Faturamento, itens mais vendidos e perfis de consumo, sem nome de cliente | | |
| G4 | Abrir Cardápio, criar um item vegano e uma promoção | O item aparece no cardápio do cliente com a marcação | | |
| G5 | Abrir Salão, conferir praças e mesas | 3 praças com vagas 3/3/2 e 20 mesas | | |
| G6 | Em Usuários, criar um garçom, entrar com ele em outra aba | A conta nova entra | | |
| G7 | Trocar o papel dessa conta | A aba aberta com o papel antigo perde o acesso | | |
| G8 | Inativar a conta | Ela não entra mais | | |
| G9 | Sair | Voltar no navegador não reabre a tela | | |

### Metre

| Nº | Passo | Resultado esperado | P/F | Observação |
|---|---|---|---|---|
| M1 | Entrar com `helena.metre@` | Cai na alocação do turno | | |
| M2 | Pedir a sugestão de alocação | Cada garçom presente em uma praça, sem exceder as vagas, com o motivo de cada designação | | |
| M3 | Trocar um garçom de praça | A troca é aceita; o garçom aparece como "Ajustado", com a praça sugerida ao lado | | |
| M4 | Confirmar a alocação | O turno fica confirmado; os garçons veem a sua praça | | |
| M5 | Abrir Salão agora | Mesas abertas por praça, em tempo real | | |

### Garçom (no celular)

| Nº | Passo | Resultado esperado | P/F | Observação |
|---|---|---|---|---|
| W1 | Entrar com `carla@` | Cai nas mesas da sua praça | | |
| W2 | Abrir uma mesa livre | Abre em **2 toques** (RNF02) | | Toques: |
| W3 | Lançar um item | Lança em **3 toques** (RNF02) | | Toques: |
| W4 | Lançar a moqueca e olhar a sugestão | Sugere arroz de coco entre as primeiras | | |
| W5 | Registrar uma restrição alimentar na mesa | A restrição aparece para o garçom e não aparece para o Gerente | | |
| W6 | Abrir A entregar e marcar um item como entregue | O item sai da lista | | |
| W7 | Fechar a conta, retirando a taxa de serviço | Total sem os 10%, com o motivo registrado | | |
| W8 | Abrir Seu desempenho | O índice dele e a faixa, sem os valores dos colegas | | |
| W9 | Digitar `/analises` na barra de endereço | Tela de sem permissão | | |

### Cliente (no celular, sem login)

| Nº | Passo | Resultado esperado | P/F | Observação |
|---|---|---|---|---|
| C1 | Na comanda do garçom, abrir **Conta do cliente** e ler o QR code com o celular do cliente | Abre a conta da mesa, sem login | | |
| C2 | Abrir `/cliente`, ir ao cardápio e filtrar por sem glúten | Só itens marcados como sem glúten | | |
| C3 | Com a conta aberta, o garçom lança um item | O item aparece em até **2 segundos** (RNF01) | | Tempo: |
| C4 | Depois do fechamento, avaliar o atendimento | A avaliação é aceita uma única vez | | |
| C5 | Abrir `/cliente/conta/` com um código inventado | "Não encontramos essa conta", sem dizer se a mesa existe | | |

### Segurança e falhas

| Nº | Passo | Resultado esperado | P/F | Observação |
|---|---|---|---|---|
| S1 | Errar a senha de um garçom 5 vezes | A conta é bloqueada por tempo (RN09); a mensagem não diz se o e-mail existe | | |
| S2 | `docker stop gastra-analitica` e lançar um pedido | A comanda continua aceitando pedidos; só a sugestão some (RNF05) | | |
| S3 | Com o serviço parado, o Metre aloca manualmente | A alocação manual funciona | | |
| S4 | `docker start gastra-analitica` | A sugestão volta sem reiniciar a API | | |

## C. Medição dos RNF

**RNF01 (até 2 s para o pedido aparecer na conta do cliente):** dez repetições do passo C3. Gravar a tela dos
dois celulares lado a lado (ou um cronômetro filmado junto) e medir do toque de confirmação do garçom até o item
aparecer no celular do cliente.

| Repetição | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | Média | Máximo |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Tempo (s) | | | | | | | | | | | | |

**RNF02 (abrir mesa em 2 toques e lançar item em 3):** contar os toques dos passos W2 e W3 em três mesas
diferentes, inclusive uma mesa com restrição registrada.

| Mesa | Toques para abrir | Toques para lançar |
|---|---|---|
| | | |
| | | |
| | | |

## D. Defeitos encontrados

| Nº | Passo | O que aconteceu | Severidade (baixa/média/alta) | Issue | Corrigido em |
|---|---|---|---|---|---|
| | | | | | |

## E. Resumo para a seção 5.6

Preencher no fim, com a quantidade de passos executados e aprovados por papel, os tempos do RNF01, os toques do
RNF02, os defeitos com a correção de cada um e o resultado da máquina limpa.

| Papel | Passos | Passou | Falhou |
|---|---|---|---|
| Máquina limpa | 6 | | |
| Gerente | 9 | | |
| Metre | 5 | | |
| Garçom | 9 | | |
| Cliente | 5 | | |
| Segurança e falhas | 4 | | |
| **Total** | **38** | | |
