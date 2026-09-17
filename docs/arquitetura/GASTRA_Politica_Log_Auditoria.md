# GASTRA — Política de Log e Auditoria

Define o que o sistema registra em log, com que nível de detalhe, por quanto tempo e quem acessa —
e, principalmente, o que **nunca** é registrado (issue #86).

Requisitos relacionados: **RNF03** (minimização e acesso restrito por papel), **RNF04** (permissões
distintas por papel), **RN04** (dado pessoal do cliente só no contexto da mesa ativa), **RN05**
(sem captura de atributos sensíveis), **RN06** (senha só em hash), **RN07** (segredo TOTP nunca
reexibido).

> **Status:** proposta da dupla. Os prazos de retenção e os itens da seção 9 dependem de validação
> com o orientador.

---

## 1. Princípio

**Registrar a ação, não o dado.**

A LGPD (Lei nº 13.709/2018) cobra ao mesmo tempo:

- **prestação de contas** (art. 6º, X) e **registro das operações de tratamento** (art. 37) — é
  preciso conseguir responder *quem fez o quê e quando*;
- **necessidade** (art. 6º, III) e **segurança** (art. 6º, VII; art. 46) — tratar apenas o mínimo e
  proteger o que é guardado.

Um log que copia os dados manipulados vira um segundo banco de dados pessoais, sem os mesmos
controles do banco principal. Por isso o log de auditoria guarda **identificadores e o tipo da
ação**, e não o conteúdo dos dados pessoais envolvidos.

---

## 2. Titulares de dados no GASTRA

| Titular | Dados pessoais tratados | Onde |
|---|---|---|
| **Funcionário** (Gerente, Coordenador, Metre, Garçom) | nome, e-mail, papel, senha (hash), segredo TOTP, alocações, desempenho | `Usuario`, `Alocacao`, índice de desempenho (RF11) |
| **Cliente** | **não é identificado** (sem nome, CPF ou e-mail). Dados vinculados à comanda: código de acesso, restrição alimentar e observação livre | `Comanda`, `RestricaoAlimentar` |

**Ponto de atenção — dado sensível:** a categoria `alergia` e a `observacao_livre` de
`RestricaoAlimentar` podem conter **dado referente à saúde**, que é dado pessoal sensível
(art. 5º, II). Como a restrição fica ligada a uma comanda (com código de acesso, mesa, horário e
garçom), o dado é pseudonimizado, e não anonimizado. Esta política trata esses campos como
sensíveis: **nunca aparecem em nenhum log**. A decisão de modelagem correspondente está na issue #40.

---

## 3. Dois tipos de log

| | **Log técnico** | **Log de auditoria** |
|---|---|---|
| Pergunta que responde | *O sistema está funcionando? Por que falhou?* | *Quem fez o quê, quando?* |
| Público | equipe de desenvolvimento | Gerente (RNF04) |
| Onde | saída do ASP.NET Core (`ILogger`) e do FastAPI | tabela própria no MySQL |
| Conteúdo | erros, tempos de resposta, falhas de integração | eventos de negócio da seção 4 |
| Retenção proposta | 30 dias | 6 meses (seção 7) |

---

## 4. Eventos de auditoria

Legenda da coluna "Registrar": **ator** = `id_usuario` e papel de quem executou; **alvo** = tipo e
identificador do registro afetado.

### 4.1 Autenticação e contas

| Evento | Caso de uso | Registrar | Nunca registrar |
|---|---|---|---|
| Login com sucesso | UC01 | ator, data/hora, IP | senha, token de sessão |
| Login com falha | UC01 | `id_usuario` **se a conta existir**; motivo (senha incorreta / conta inativa); data/hora; IP | senha digitada; **e-mail digitado quando a conta não existe** |
| Autenticador vinculado | RN07 | ator, data/hora, IP | segredo TOTP, chave manual |
| Segundo fator confirmado / recusado | UC02 | ator, resultado, data/hora | código TOTP digitado, segredo TOTP |
| Logoff | UC03 | ator, data/hora | token de sessão |
| Conta criada / editada / inativada | UC04 | ator, alvo, **nomes** dos campos alterados; papel anterior → novo | senha, hash, segredo TOTP, valores de nome e e-mail |
| Senha redefinida pelo Gerente | UC04 | ator, alvo, data/hora | senha nova, hash |
| Segundo fator zerado pelo Gerente | UC04 | ator, alvo, data/hora | segredo TOTP antigo ou novo |

Justificativas:

- **Falha de login com e-mail inexistente:** o texto digitado pode ser o e-mail de uma pessoa sem
  relação com o sistema. Registrar só "conta não encontrada" já permite detectar tentativas
  repetidas.
- **Mudança de papel com valor antigo e novo:** é a informação que prova quem concedeu acesso
  administrativo a quem — o objetivo central da auditoria. Não é dado sensível.
- **Redefinição de senha e do segundo fator (#141):** as duas ações dão acesso à conta de outra pessoa, então
  precisam de dono. Nenhuma pode ser feita na própria conta, e as duas derrubam as sessões abertas daquela conta.
  Quem redefine nunca fica sabendo a senha anterior: só o hash é guardado (RN06).

### 4.2 Cardápio

| Evento | Caso de uso | Registrar | Nunca registrar |
|---|---|---|---|
| Item cadastrado | UC05 | ator, alvo | — |
| Preço alterado | UC06 | ator, alvo, preço anterior → novo | — |
| Disponibilidade alterada | UC07 | ator, alvo, novo estado | — |
| Foto do item alterada | RF05 | ator, alvo, se o item ficou com ou sem foto | — |
| Promoção criada / removida | UC08, UC09 | ator, alvo, itens vinculados (implementado no #124) | — |
| Praça cadastrada / editada | UC24 | ator, alvo, código e quantidade de garçons | — |
| Mesa cadastrada / editada | UC24 | ator, alvo, número, capacidade e praça | — |

Dados do cardápio e do salão não são pessoais: podem ser registrados com os valores.

### 4.3 Comandas

| Evento | Caso de uso | Registrar | Nunca registrar |
|---|---|---|---|
| Comanda aberta | UC10 | ator (garçom), `id_comanda`, `id_mesa` | código de acesso do cliente |
| Composição da mesa ajustada | UC11 | ator, `id_comanda`, composição sugerida → confirmada | — |
| Item registrado | UC12 | ator, `id_comanda`, `id_item_cardapio`, quantidade | — |
| Item cancelado | UC12 | ator, `id_item_pedido`, motivo (lista fechada) | — |
| **Restrição alimentar registrada** | UC13 | **apenas** ator, `id_comanda` e o fato de que houve registro | **categoria e observação livre** |
| Taxa de serviço removida | UC14 | ator, `id_comanda` | — |
| Comanda fechada | UC14 | ator, `id_comanda`, valor total | — |

### 4.4 Consulta do cliente

| Evento | Caso de uso | Registrar | Nunca registrar |
|---|---|---|---|
| Comanda consultada por QR code | UC20 | somente em log **técnico**: data/hora e resultado (encontrada / não encontrada) | **código de acesso completo**, IP do cliente |

O código de acesso funciona como uma senha da comanda: quem tem o código vê a conta. Registrá-lo
por inteiro permitiria que quem lê o log consultasse comandas de terceiros. Se for necessário
correlacionar tentativas, registrar apenas os 4 últimos caracteres.

### 4.5 Alocação de garçons e análises

| Evento | Caso de uso | Registrar | Nunca registrar |
|---|---|---|---|
| Sugestão de alocação gerada | UC15 | ator (metre), data, período, pesos `w1`/`w2` usados | — |
| Alocação confirmada | UC21 | ator, data, período | — |
| Alocação ajustada manualmente | UC22 | ator, garçom, praça sugerida → praça escolhida | — |
| Relatório de BI consultado | UC16 | ator, relatório, filtros de período | — |
| Índice de desempenho consultado | UC17 | ator, garçom consultado | — |

Justificativas:

- **Ajuste manual da alocação:** a RN03 existe para reduzir a percepção de injustiça na
  distribuição das praças. Sem registro de quem alterou a sugestão e para onde, não há como
  demonstrar que a regra foi seguida.
- **Consulta do índice de desempenho:** o índice é dado pessoal do funcionário (avaliação de
  desempenho). Registrar quem consultou o índice de quem é prestação de contas sobre esse
  tratamento.

---

## 5. Nunca registrar (em nenhum tipo de log)

| Dado | Motivo |
|---|---|
| Senha (texto ou hash) | RN06 |
| Segredo TOTP e códigos TOTP digitados | RN07 |
| Tokens de sessão / autenticação | permitem se passar pelo usuário |
| Código de acesso do cliente completo | permite consultar a comanda (seção 4.4) |
| Categoria e observação livre da restrição alimentar | possível dado de saúde (seção 2), RN05 |
| Nome e e-mail de funcionários | o `id_usuario` já identifica o ator |
| Corpo completo de requisições e respostas | carrega todos os dados acima de uma vez |
| String de conexão e variáveis de ambiente | segredos de infraestrutura |

---

## 6. Estrutura do registro de auditoria

| Campo | Exemplo | Observação |
|---|---|---|
| `data_hora_utc` | `2026-09-14T19:42:10Z` | sempre em UTC |
| `evento` | `ALOCACAO_AJUSTADA` | código fixo, um por linha da seção 4 |
| `resultado` | `SUCESSO` / `FALHA` | |
| `id_usuario` | `12` | ator; nulo apenas em falha de login sem conta |
| `papel` | `Metre` | papel no momento da ação |
| `entidade` / `id_entidade` | `Alocacao` / `87` | alvo |
| `detalhes` | `{"praca_sugerida": 2, "praca_escolhida": 4}` | JSON **somente com os campos permitidos na seção 4** |
| `ip` | `192.168.0.15` | **somente em eventos de autenticação** (4.1) |
| `id_correlacao` | `0HN6...` | liga o evento ao log técnico da mesma requisição |

---

## 7. Retenção e eliminação

> **Implementado (#120).** A API roda a eliminação ao subir e depois uma vez por dia
> (`EliminarAuditoriaVencidaUseCase`). O corte usa 6 meses e mais um dia de folga, para o relógio da API
> nunca pedir a exclusão de um registro que o trigger do banco ainda protege. A própria eliminação vira um
> registro `AUDITORIA_ELIMINADA_POR_PRAZO`, com a quantidade apagada e sem o conteúdo.

| Tipo | Retenção proposta | Após o prazo |
|---|---|---|
| Log técnico | 30 dias | eliminação |
| Log de auditoria | 6 meses | eliminação |

- **6 meses para auditoria:** acompanha o prazo mínimo de guarda de registros de acesso a
  aplicações do Marco Civil da Internet (Lei nº 12.965/2014, art. 15), usado aqui como referência.
  O prazo final depende de validação com o orientador (seção 9).
- **Eliminação ao fim do prazo:** a LGPD prevê a eliminação dos dados após o término do tratamento
  (art. 16).
- O log técnico não tem valor de prestação de contas; guardá-lo por mais tempo só aumentaria o
  risco.

---

## 8. Diretrizes de implementação

> **Situação:** todos os eventos das seções 4.1 a 4.3 e os do salão estão implementados (#120), com testes
> que conferem o que é registrado e o que nunca pode aparecer (`AuditoriaTests`). Da seção 4.5, os de
> alocação entraram com a #122 e os de BI com a #123: toda consulta de relatório registra qual relatório e o período, e a
> consulta do índice registra de quem foi o índice consultado ("todos", para o Gerente, ou o próprio id, para o Garçom). Os códigos dos eventos ficam em
> `Gastra.Domain/Auditoria/EventoAuditoria.cs`.

1. **A auditoria é gravada pela aplicação, não por trigger do banco.** Um trigger não sabe *qual
   usuário* executou a ação — essa informação existe na API (usuário autenticado), não no MySQL.
   Os casos de uso da camada `Application` chamam o registro de auditoria.
2. **Tabela somente de inserção.** A aplicação só insere registros de auditoria; nunca altera nem
   apaga. Um trigger que bloqueia `UPDATE` e `DELETE` nessa tabela é um uso justificado de trigger
   no banco (ver #85): protege o log inclusive contra erro da própria aplicação. A exceção é a
   rotina de eliminação por prazo (seção 7).
3. **`EnableSensitiveDataLogging` do Entity Framework nunca ativo fora do ambiente local.** Essa
   opção inclui os valores dos parâmetros das consultas SQL no log técnico — senha, restrição
   alimentar, tudo.
4. **Middleware de exceções sem corpo da requisição.** O tratamento global de erros registra tipo
   da exceção, rota e `id_correlacao`, e não o conteúdo enviado.
5. **Consulta por código de acesso via corpo ou cabeçalho, não por query string.** Query strings
   aparecem por padrão nos logs de requisição.
6. **Camada analítica (Python):** recebe apenas dados agregados ou identificadores. Não registra em
   log os dados recebidos para cálculo.
7. **Acesso ao log de auditoria restrito ao Gerente** (RNF04); o log técnico, à equipe de
   desenvolvimento.

---

## 9. Pontos para validação com o orientador

- [ ] Retenção de 6 meses para auditoria e 30 dias para log técnico.
- [ ] Registro de IP apenas em eventos de autenticação.
- [ ] Registro de consultas a relatórios de BI e ao índice de desempenho (seção 4.5).
- [ ] Tratamento da restrição alimentar como dado sensível — depende da decisão da issue #40.
