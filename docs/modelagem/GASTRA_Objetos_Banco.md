# Views e triggers do banco — GASTRA

Este documento lista os objetos de banco além das tabelas e explica **por que cada um está no
banco e não na aplicação**. Esse é o critério de aceite da issue #85.

- **Onde são criados:** na migration `CriaViewsETriggersAnaliticos`, em
  `backend/src/Gastra.Infrastructure/DataAccess/Migrations/`.
- **SQL completo:** fica em [`GASTRA_Schema.sql`](GASTRA_Schema.sql).
- **Testes:** `backend/tests/Gastra.Api.Tests/BancoDeDados/`. Rodam contra um MySQL de verdade (veja
  [como rodar](#como-rodar-os-testes)).

---

## 1. Critério usado para decidir o que vai para o banco

| Vai para o banco | Fica na aplicação (C#) |
|---|---|
| Agregações de leitura sobre muitas linhas, que o SGBD faz melhor e que mais de um consumidor usa (BI no backend, algoritmos no Python) | Regras de negócio que mudam o estado: abrir, fechar, calcular taxa de serviço, cancelar item |
| Garantias que precisam valer **mesmo se alguém acessar o banco sem passar pela API** | Qualquer regra que precise saber quem é o usuário ou qual caso de uso está em andamento |
| | Fórmulas com parâmetros que o gerente pode ajustar (pesos, período) |

**Por que não há stored procedures (D6).** A issue sugeria uma procedure de fechamento de comanda
com cálculo da taxa de serviço. Essa regra já existe no domínio (`Comanda.Fechar`,
`Comanda.CalcularTaxaServico`) e é coberta por testes de unidade que rodam sem banco. Uma
procedure faria a mesma regra existir em dois lugares, e os dois poderiam divergir.

---

## 2. Views

As views são **somente leitura** e **não expõem dado pessoal**: só ids, datas e valores. O
backend junta o nome do garçom quando monta a tela. O Python lê apenas as views (decisão D10). Um
teste automatizado verifica que nenhuma coluna de view se chama `nome`, `email`,
`observacao_livre` ou `codigo_acesso_cliente`.

### Regras comuns a todas as views

| Regra | Decisão | Motivo |
|---|---|---|
| Quais comandas entram | Só as **fechadas** | Comanda aberta ainda pode mudar; contar agora e depois daria números diferentes para o mesmo turno |
| Itens cancelados | **Não** entram | Não foram vendidos (RF24) |
| Faturamento | Soma de `quantidade × preço no momento`, **sem a taxa de serviço** | A taxa de serviço pertence aos empregados (Lei 13.419/2017) e não é receita do restaurante. Somar a taxa também inflaria praças onde o cliente não a dispensou |
| Fuso horário | As datas são gravadas em UTC e convertidas para o **horário de Brasília (UTC−3)** | O Brasil não tem horário de verão desde 2019, então o deslocamento fixo é exato e dispensa as tabelas de fuso do MySQL. Sem a conversão, uma comanda das 23h30 cairia no dia seguinte |
| Data e turno da comanda | Pela **abertura**, não pelo fechamento | Uma mesa aberta às 14h50 e fechada às 16h10 é do almoço |
| Almoço × jantar | Abertura **antes das 17h** é almoço; a partir das 17h é jantar | ⚠️ **Horário de corte a confirmar com o restaurante.** Muda num lugar só: a view `vw_comanda_faturamento` |

### Lista

| View | Uma linha por | Colunas | Quem usa | Requisito |
|---|---|---|---|---|
| `vw_comanda_faturamento` | comanda fechada | comanda, garçom, mesa, praça, data, período, dia da semana, hora, pessoas, composição, faturamento | Base das demais; BI por hora do dia e por dia da semana | RF10 |
| `vw_faturamento_praca_turno` | praça × data × período | comandas, pessoas atendidas, faturamento | BI por praça e alocação por turno | RF08, RF10 |
| `vw_faturamento_medio_praca` | praça (inclusive as sem movimento) | turnos com movimento, faturamento total, **faturamento médio por turno** | Programação linear (potencial da praça) | RN03, RF06 |
| `vw_desempenho_garcom_turno` | garçom × data × período | comandas atendidas, **mesas atendidas**, pessoas atendidas, faturamento | BI por garçom e índice de desempenho | RF10, RF11 |
| `vw_faturamento_item_cardapio` | item × data × período | categoria, quantidade vendida, faturamento | BI por item e por categoria | RF10 |
| `vw_itens_por_comanda` | comanda × item | data, quantidade | Regras de associação (Python) | RF09, RN05 |

### Por que cada uma está no banco

- **`vw_comanda_faturamento`.** Evita que cada consulta de BI refaça, do seu jeito, as regras da
  tabela acima (comanda fechada, sem cancelados, sem taxa, fuso, turno). Se o horário de corte
  mudar, muda aqui e todos os relatórios acompanham.
- **`vw_faturamento_medio_praca`.** O MER define o faturamento médio histórico da praça como
  **atributo derivado**: ele não é gravado em coluna, porque ficaria desatualizado a cada comanda
  fechada. A view calcula o valor a cada leitura.
  - A média é **por turno com movimento**, porque a alocação também é feita por turno.
  - Praças sem nenhum turno aparecem com zero, e a coluna `turnos_com_movimento` deixa quem
    consulta decidir como tratar uma praça nova.
- **`vw_faturamento_praca_turno`, `vw_desempenho_garcom_turno` e `vw_faturamento_item_cardapio`.**
  São os agrupamentos pedidos pelos KPIs (`GASTRA_KPIs_Criterios_Analiticos.docx`). Deixá-los no
  banco permite filtrar por período com um `WHERE data BETWEEN ...`, sem trazer comanda por
  comanda para a memória da API.
- **`vw_itens_por_comanda`.** É o formato de "transação" que o algoritmo de regras de associação
  espera. Pela D10, o Python lê o histórico por aqui em vez de receber tudo do backend a cada
  chamada.
  - A view só tem ids de item e de comanda, então não há como o modelo usar atributo pessoal do
    cliente (RN05).

### O índice de desempenho não é uma view

O documento de arquitetura previa uma view com o índice de desempenho. Ela foi trocada pelas
**bases** do índice (`vw_desempenho_garcom_turno`), e a fórmula fica na aplicação:

- o índice combina faturamento e mesas atendidas **com pesos**, e os pesos podem mudar;
- o índice é **relativo ao período escolhido** na tela (semana, mês). Uma view não recebe
  parâmetros, então precisaria fixar um período ou calcular sobre todo o histórico.

---

## 3. Triggers

| Trigger | Quando | Efeito |
|---|---|---|
| `trg_registro_auditoria_impede_update` | antes de `UPDATE` em `registro_auditoria` | Recusa a operação (`SQLSTATE 45000`, "O registro de auditoria não pode ser alterado.") |
| `trg_registro_auditoria_impede_delete` | antes de `DELETE` em `registro_auditoria` | Recusa a operação (`SQLSTATE 45000`, "O registro de auditoria não pode ser apagado.") |

**Por que estão no banco.** A aplicação nunca altera nem apaga auditoria. Mas uma garantia só no
código não impede quem acessa o banco direto (um script, uma ferramenta de administração, um
bug futuro). O trigger vale para qualquer caminho.

**Por que a auditoria em si não é gravada por trigger.** Um trigger não sabe *quem* fez a
alteração nem *em qual* caso de uso, e a aplicação sabe. Por isso a aplicação grava e o banco só
protege o que foi gravado ([`GASTRA_Politica_Log_Auditoria.md`](../arquitetura/GASTRA_Politica_Log_Auditoria.md), seção 8).

**Limite conhecido.** Um usuário com privilégio para apagar o trigger (`DROP TRIGGER`) ainda
consegue contorná-lo. Em produção, o usuário da aplicação não deveria ter esse privilégio. Esse
endurecimento fica registrado como trabalho futuro.

---

## 4. Ajuste no contêiner do MySQL

Com o log binário ligado (padrão do MySQL 8.4), o MySQL só deixa criar trigger quem tem
privilégio `SUPER`, e o usuário da aplicação não tem. O `infra/docker-compose.yml` passou a subir o
MySQL com `--log-bin-trust-function-creators=ON`.

- **Por que é seguro aqui:** a opção existe para proteger a replicação de rotinas não
  determinísticas. Os dois triggers do GASTRA só recusam a operação, então são determinísticos.
- **Para quem já tinha o contêiner:** depois de atualizar a branch, rode `docker compose up -d`
  dentro de `infra/`. O contêiner é recriado e o volume com os dados é mantido.

---

## Como rodar os testes

Os testes de views e triggers ficam **ignorados** no `dotnet test` comum, porque o banco em memória
dos outros testes não tem views nem triggers. Para rodá-los:

1. Deixe o contêiner do MySQL de pé.
2. Defina a variável `GASTRA_TESTES_MYSQL` com um usuário que possa criar schemas. No ambiente
   local, esse usuário é o `root` do contêiner (a senha é a do seu `infra/.env`, e nunca vai para o
   repositório).
3. Rode `dotnet test`.

Cada teste cria um schema descartável (`gastra_teste_...`), aplica todas as migrations, insere
dados com datas controladas e apaga o schema no fim.
