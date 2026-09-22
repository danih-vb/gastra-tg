# GASTRA — Validação dos dados simulados

Confere se o histórico gerado pelo semeador (`backend/tools/GastraSemeador`) faz os algoritmos do TG
funcionarem de verdade, e não só encher tabela. Issue #185.

- **Execução conferida:** semente 42, 70 dias, 4.213 comandas fechadas e 12.753 itens.
- **Ambiente:** MySQL 8.4 local, serviço analítico Python no ar, lendo o banco pelo usuário somente leitura (D10).

> Os dados são **simulados**. Não representam nenhum restaurante real, nem o estabelecimento entrevistado
> na pesquisa.

---

## 1. O Python passou a ler o banco

O serviço analítico usa o histórico real quando encontra movimento suficiente — pelo menos 50 comandas com
2 itens ou mais no último ano. Abaixo disso, cai no histórico simulado em memória e avisa o motivo.

O banco semeado tem **3.128 comandas com 2 ou mais itens**. Pedindo uma recomendação:

```json
{ "regras_consideradas": 59, "origem_do_historico": "banco", "motivo_do_simulado": null }
```

✅ **`origem_do_historico = banco`.** Antes do semeador, a resposta vinha com `"simulado"`.

---

## 2. A combinação plantada foi reencontrada

O semeador embute as mesmas combinações do `simulador.py` — entre elas "moqueca puxa arroz de coco", com 85%
de chance. O teste é o gabarito da validação: o algoritmo de regras de associação precisa **redescobrir** a
regra a partir dos dados, sem que ninguém diga qual é.

Pedindo sugestão para uma comanda que tem só moqueca (item 5):

| Sugestão | Confiança | Lift | Plantado com |
|---|---|---|---|
| Arroz de coco | 0,871 | 1,552 | 0,85 |
| Caipirinha | 0,749 | 1,294 | 0,60 |

✅ **As duas regras voltaram, na ordem certa e com confiança próxima da plantada.** O lift acima de 1 mostra
que a associação é maior do que o acaso explicaria.

---

## 3. A alocação entrega a praça de alto potencial a quem faturou menos

É o comportamento que a RN03 existe para produzir. Com os números que as views devolvem (últimos 30 dias) e
as praças semeadas:

| Garçom | Faturamento por turno | Praça designada | Potencial da praça |
|---|---|---|---|
| 5 | R$ 386,85 — o menor | **1** | R$ 1.876/turno — alto |
| 7 | R$ 516,75 | **1** | alto |
| 6 | R$ 532,06 | 2 | R$ 1.244/turno |
| 8 | R$ 626,90 | 2 | médio |
| 9 | R$ 664,81 | 2 | médio |
| 10 | R$ 729,85 | 3 | R$ 615/turno — baixo |
| 11 | R$ 816,42 — o maior | 3 | baixo |

✅ **A ordem saiu exatamente invertida:** quem fatura menos pega a praça melhor, quem fatura mais pega a
mais fraca. É o efeito do peso de 0,6 sobre o faturamento médio por turno.

---

## 4. As telas de análise não ficam vazias

| View | O que devolve | Situação |
|---|---|---|
| `vw_faturamento_medio_praca` | 3 praças, todas com movimento em ~140 turnos | ✅ |
| `vw_desempenho_garcom_turno` | 7 garçons, 41 a 55 turnos cada nos últimos 30 dias | ✅ |
| `vw_comanda_faturamento` | 4.213 comandas, almoço e jantar equilibrados | ✅ |
| `vw_faturamento_item_cardapio` | os 14 itens do cardápio vendem, de 431 a 2.557 unidades | ✅ |
| `vw_itens_por_comanda` | 3.128 comandas com 2 itens ou mais | ✅ |

O movimento por hora tem a cara de um restaurante, com dois picos:

| Hora | 11 | 12 | 13 | 14 | 15 | 18 | 19 | 20 | 21 |
|---|---|---|---|---|---|---|---|---|---|
| Comandas | 498 | 461 | 456 | 469 | 211 | 339 | 693 | 702 | 384 |

> **Ressalva.** As views foram consultadas direto no banco. As telas em Angular leem exatamente esses
> números por `/api/indicadores/*`, mas **não foram exercitadas contra a API real** — entrar no sistema pede
> senha, e essa continua sendo a lacuna de verificação registrada em todas as PRs do frontend.

---

## 5. Divergência documentada: o Gini do histórico é maior que o da calibração

A issue pedia para comparar a desigualdade do dado semeado com a de
[`GASTRA_Calibracao_Pesos_RN03.md`](GASTRA_Calibracao_Pesos_RN03.md), e registrar a divergência como achado.

| Medida | Gini do faturamento médio por turno |
|---|---|
| Calibração, política escolhida (w1 = 0,6) | 0,0048 a 0,0072 |
| Calibração, sem rodízio (w1 = 0,0) | 0,0473 |
| **Histórico semeado** | **0,1235** |

**Divergiu, e era para divergir.** As duas medidas não medem a mesma coisa:

- A calibração mede a desigualdade **depois** de 120 turnos rodando a política de alocação, com garçons que
  só diferem pelo histórico. É o *resultado* da RN03.
- O histórico semeado é o **estado inicial**: 70 dias de rodízio neutro, com garçons que têm fatores de
  venda diferentes de propósito (de 0,70 a 1,42). É o *problema* que a RN03 existe para corrigir.

Se o semeador entregasse Gini de 0,005, a alocação não teria nada para consertar: todo mundo já estaria
igual, a sugestão sairia arbitrária e a demonstração não mostraria a regra funcionando. A desigualdade
inicial de 0,1235 — o garçom que mais fatura ganha 2,1 vezes o que menos fatura — é o que dá contraste à
seção 3 acima.

**Achado a acompanhar:** ninguém mediu ainda o Gini *depois* de N turnos usando a alocação do sistema sobre
esta base. Seria a prova mais forte da RN03 — a curva caindo de 0,12 em direção ao valor da calibração — e
fica como sugestão de experimento para o documento de pesquisa.

---

## Como repetir

1. Suba o MySQL e o serviço analítico, e rode o semeador (ver `backend/tools/GastraSemeador/README.md`).
2. Recomendação: `POST http://localhost:8000/recomendacao/combinacoes` com `{"itens":[<id da moqueca>]}`.
3. Alocação: `POST http://localhost:8000/alocacao/sugestao` com os garçons e praças que as views devolvem.
4. Views: consultas diretas, como as deste documento.

Com a mesma semente, os números se repetem.
