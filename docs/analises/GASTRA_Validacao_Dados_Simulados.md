# GASTRA — Validação dos dados simulados

Confere se o histórico gerado pelo semeador (`backend/tools/GastraSemeador`) faz os algoritmos do TG
funcionarem de verdade, e não só encher tabela. Issue #185, atualizada com a clusterização (#240).

- **Execução conferida:** semente 42, 70 dias, 4.204 comandas fechadas e 13.914 itens, semeados num banco separado
  (`gastra_validacao`) para não mexer no banco de desenvolvimento.
- **Como repetir:** `data-science/scripts/validar_dados_semeados.py`, que lê só as views e imprime as tabelas deste
  documento (seção "Como repetir").

> Os dados são **simulados**. Não representam nenhum restaurante real, nem o estabelecimento entrevistado
> na pesquisa.

## Os dois gabaritos plantados

O semeador e o `simulador.py` plantam, de propósito, padrões que os algoritmos precisam **redescobrir** sem que
ninguém diga quais são:

| Gabarito | O que é | Quem precisa redescobrir |
|---|---|---|
| Combinações | "Moqueca puxa arroz de coco" em 85% das vezes, "moqueca puxa caipirinha" em 60%, "pudim puxa café" em 70% e "porção infantil puxa suco" em 65% | Regras de associação |
| Perfis de consumo | Cada comanda nasce de um de três perfis — **almoço executivo** (salada, risoto, suco, água com gás, café), **frutos do mar** (moqueca, bobó, bolinho de bacalhau, caipirinha, petit gâteau) e **família** (porção infantil, suco, arroz de coco, pudim, sorvete) —, que dá a chance de cada item | Clusterização |

O perfil de cada comanda é sorteado pelo período e pela quantidade de pessoas (no almoço a dois, o executivo; no
jantar, os frutos do mar; com três ou mais pessoas, a família). **Período e pessoas não entram no algoritmo**
(RN05): ele recebe só os itens pedidos. Servem só para conferir, depois, se os grupos encontrados fazem sentido.

---

## 1. Clusterização: os três perfis foram redescobertos

O K-Means agrupou as comandas pelos itens pedidos, testando de 2 a 6 perfis. A quantidade escolhida é a de maior
**silhueta** (o quanto cada comanda está mais perto do próprio grupo do que do vizinho):

| Perfis testados | 2 | **3** | 4 | 5 | 6 |
|---|---|---|---|---|---|
| Silhueta | 0,351 | **0,387** | 0,351 | 0,330 | 0,329 |

✅ **Escolheu 3, a quantidade plantada**, e acima de 0,25, o limite abaixo do qual não há estrutura
substancial. Os grupos:

| Perfil | Comandas | Itens que mais se destacam (presença no perfil; quantas vezes acima do restaurante) | No almoço | Com 3+ pessoas |
|---|---:|---|---:|---:|
| Família | 1.290 (31%) | porção infantil (74%; 3,0×), pudim (46%; 2,8×), sorvete de tapioca (46%; 2,8×), suco (73%; 2,1×) | 53% | **72%** |
| Almoço executivo | 1.080 (26%) | risoto (57%; 3,4×), salada (60%; 3,3×), água com gás (45%; 3,3×), café (45%; 1,8×) | **76%** | 17% |
| Frutos do mar | 1.798 (43%) | moqueca (62%; 2,2×), bolinho de bacalhau (54%; 2,1×), caipirinha (67%; 2,1×), bobó (37%; 2,0×) | 31% | 33% |

✅ **Os itens de cada grupo são os do perfil plantado**, e o período e a composição — que o algoritmo nunca viu —
confirmam a leitura: o grupo executivo é três quartos almoço e quase só mesas de uma ou duas pessoas; o grupo
família é três quartos mesas de três ou mais.

No simulador, em que se sabe o perfil de cada comanda, a concordância entre os grupos e o gabarito foi medida pelo
**índice de Rand ajustado** (1 = perfeita, 0 = o que o acaso daria): entre 0,84 e 0,88 em três sementes, com cada
grupo formado em mais de 90% por um único perfil plantado (teste `test_redescobre_os_perfis_plantados`).

---

## 2. Recomendação: a combinação plantada voltou, e a segmentada acerta mais

### 2.1 Regras de associação

Pedindo sugestão para uma comanda que tem só moqueca:

| Sugestão | Confiança | Lift | Plantado com |
|---|---:|---:|---:|
| Arroz de coco | 0,853 | 2,115 | 0,85 |
| Caipirinha | 0,765 | 2,202 | 0,60 + a presença no perfil |
| Bolinho de bacalhau | 0,525 | 1,900 | — (vem do perfil frutos do mar) |

✅ **A combinação plantada voltou com a confiança plantada** (0,853 contra 0,85). A caipirinha sai acima dos 0,60
da combinação porque ela também é item do perfil frutos do mar, e o bolinho de bacalhau aparece sem combinação
nenhuma: é o perfil se mostrando nas regras gerais.

### 2.2 Recomendação segmentada por perfil

A recomendação segmentada associa a comanda em andamento ao perfil mais parecido e sugere, nesta ordem, pelas
regras do perfil, pelos itens característicos do perfil que a mesa ainda não pediu e pelas regras gerais.

Para medir se isso ajuda o garçom, cada comanda de teste teve um item escondido por vez, e conferiu-se se ele
voltava entre as três sugestões feitas a partir dos demais (70% das comandas para treinar, 30% para testar):

| Recomendação | Acertou o item escondido |
|---|---:|
| Só regras de associação (gerais) | 76,2% |
| **Segmentada por perfil (clusterização + regras)** | **83,2%** |

✅ **Sete pontos a mais.** O exemplo que resume a diferença: numa mesa com salada e suco, as regras gerais sugerem
a porção infantil, porque o suco aparece muito com ela nas famílias; a segmentada reconhece o almoço executivo e
sugere risoto, café e água com gás (teste `test_segmentada_sugere_o_que_mesas_do_mesmo_perfil_pedem`).

---

## 3. A alocação combina os dois fatores da RN03

Com o faturamento por turno dos últimos 30 dias devolvido pelas views e o histórico de alocações confirmadas:

| Garçom | Faturamento por turno | Turnos sem praça de alto potencial | Praça designada | Potencial da praça |
|---|---:|---:|---|---:|
| 4 | R$ 493,96 — o menor | 0 | 2 | R$ 1.518/turno — médio |
| 5 | R$ 583,87 | **3** | **1** | R$ 2.254/turno — alto |
| 6 | R$ 639,59 | 1 | 2 | médio |
| 7 | R$ 775,27 | **3** | **1** | alto |
| 8 | R$ 826,39 | 0 | 2 | médio |
| 10 | R$ 1.084,02 | 0 | 3 | R$ 787/turno — baixo |
| 9 | R$ 1.096,17 — o maior | 0 | 3 | baixo |

✅ **As duas pontas saíram como a regra pede:** os dois que mais faturaram ficaram na praça mais fraca, e a praça de
alto potencial foi para dois dos que menos faturaram.

**O garçom que menos faturou não ficou com a praça alta, e isso é a RN03 funcionando.** Ele tinha acabado de sair
dela (zero turnos de espera), enquanto os garçons 5 e 7 faturavam pouco *e* esperavam havia três turnos. Com peso
de 0,4 para a espera, a regra dá a vez a quem espera, em vez de devolver a praça boa sempre à mesma pessoa. É o
rodízio que o orientador pediu, e que a calibração (`GASTRA_Calibracao_Pesos_RN03.md`) mediu.

---

## 4. As telas de análise não ficam vazias

As views devolvem movimento nos 70 dias, nas três praças e nos sete garçons — é delas que saem as tabelas acima.
O semeador distribui as comandas entre 11h e 15h30 no almoço e entre 18h30 e 21h30 no jantar, o que dá ao mapa de
calor por hora os dois picos de um restaurante.

> **Ressalva.** As views foram consultadas direto no banco. As telas em Angular leem esses números por
> `/api/indicadores/*`, mas **não foram exercitadas contra a API real** nesta validação — entrar no sistema pede
> senha. Os testes operacionais (Sprint 5) cobrem essa lacuna.

---

## 5. Divergência documentada: o Gini do histórico é maior que o da calibração

| Medida | Gini do faturamento médio por turno |
|---|---|
| Calibração, política escolhida (w1 = 0,6) | 0,0048 a 0,0072 |
| Calibração, sem regra | 0,0456 |
| **Histórico semeado** | **0,1555** |

**Divergiu, e era para divergir.** As duas medidas não medem a mesma coisa:

- A calibração mede a desigualdade **depois** de 120 turnos aplicando a RN03, com garçons que só diferem pelo
  histórico. É o *resultado* da regra.
- O histórico semeado é o **estado inicial**: 70 dias de rodízio neutro, com garçons de fator de venda diferente de
  propósito (de 0,70 a 1,42) — o que mais fatura ganha 2,2 vezes o que menos fatura. É o *problema* que a RN03
  existe para corrigir; se o semeador já entregasse Gini perto de zero, a alocação não teria o que mostrar.

**Achado a acompanhar:** ainda não se mediu o Gini *depois* de sucessivos turnos alocados pelo próprio sistema
sobre esta base (#221). Seria a prova mais forte da RN03: a curva caindo de 0,16 em direção ao valor da calibração.

---

## Como repetir

1. Crie um banco separado e aplique as migrations:
   `dotnet ef database update -p src/Gastra.Infrastructure -s src/Gastra.Api --connection "...Database=gastra_validacao..."`
   (dentro de `backend/`).
2. Rode o semeador apontando para ele (`backend/tools/GastraSemeador/README.md`), com a semente 42.
3. Rode a validação, dentro de `data-science/`:
   ```bash
   GASTRA_VALIDACAO_URL="mysql+pymysql://<usuario>:<senha>@localhost:3307/gastra_validacao" \
       python scripts/validar_dados_semeados.py
   ```

Com a mesma semente, os números se repetem.
