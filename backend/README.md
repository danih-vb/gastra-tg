# Backend — GASTRA

API em ASP.NET Core (.NET 10), com controllers.

## Pré-requisitos

- .NET SDK 10 — a versão é fixada pelo `global.json` desta pasta. Confira com
  `dotnet --version` dentro de `backend/`.

## Estrutura

Arquitetura em camadas: cada camada é um projeto separado na solução.

```
backend/
├── global.json                    # Fixa o SDK do .NET 10
├── Gastra.slnx                    # Solução
├── src/
│   ├── Gastra.Api/                # Porta de entrada: controllers, middlewares, filters
│   ├── Gastra.Application/        # Casos de uso: orquestram o domínio
│   ├── Gastra.Communication/      # Contratos da API: requests e responses
│   ├── Gastra.Domain/             # Entidades, enums, regras de negócio, interfaces de repositório
│   ├── Gastra.Exceptions/         # Exceções de negócio e mensagens (pt-BR / en)
│   └── Gastra.Infrastructure/     # Acesso a dados: Entity Framework, repositórios, migrations
└── tests/
    └── Gastra.Api.Tests/          # Testes de integração e de arquitetura
```

### Regra de dependência

As dependências apontam **para dentro**: o domínio não depende de nenhuma outra camada.

| Camada | Pode referenciar |
|---|---|
| `Domain` | nenhuma |
| `Communication` | nenhuma |
| `Exceptions` | nenhuma |
| `Infrastructure` | `Domain` |
| `Application` | `Domain`, `Communication`, `Exceptions` |
| `Api` | `Application`, `Communication`, `Exceptions`, `Infrastructure` |

A `Api` referencia a `Infrastructure` apenas para registrar as dependências na inicialização
(*composition root*): os controllers conversam somente com a `Application`.

Essa regra é verificada automaticamente por `tests/Gastra.Api.Tests/ArquiteturaTests.cs` —
se uma camada passar a depender de outra indevidamente, o `dotnet test` falha.

### Injeção de dependência

Cada camada registra os próprios serviços em um método de extensão, e o `Program.cs` apenas os
chama:

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

O nome da camada de exceções está no plural (`Gastra.Exceptions`) de propósito: um namespace
`Gastra.Exception` faria a palavra `Exception` apontar para o namespace, e não para
`System.Exception`, dentro de todo código `Gastra.*`.

## Comandos

Todos executados dentro de `backend/`.

| Comando | O que faz |
|---|---|
| `dotnet build` | Compila a solução |
| `dotnet test` | Compila e roda os testes |
| `dotnet run --project src/Gastra.Api` | Sobe a API em `http://localhost:5019` |

Com a API rodando, `http://localhost:5019/health` deve responder `Healthy`.

### Testes de views e triggers (MySQL real)

Os testes em `tests/Gastra.Api.Tests/BancoDeDados/` aparecem como **ignorados** no `dotnet test`
comum. Para rodá-los, com o contêiner do MySQL de pé, defina `GASTRA_TESTES_MYSQL` com um usuário que
possa criar schemas (no ambiente local, o `root`, com a senha do seu `infra/.env`):

```bash
export GASTRA_TESTES_MYSQL="Server=localhost;Port=3307;User ID=root;Password=<senha do root>"
dotnet test
```

Cada teste cria um schema descartável, aplica as migrations e o apaga no fim. Detalhes em
[`docs/modelagem/GASTRA_Objetos_Banco.md`](../docs/modelagem/GASTRA_Objetos_Banco.md).

### Documentação interativa (Swagger)

Com a API rodando em desenvolvimento, `http://localhost:5019/swagger` lista todos os endpoints,
com as descrições tiradas dos comentários `///` dos controllers, e permite testá-los pelo navegador:

1. Chame `POST /api/autenticacao/login` (e, para Gerente ou Coordenador, `segundo-fator/confirmar`).
2. Copie o `tokenAcesso` da resposta, clique em **Authorize** e cole o token.
3. Os endpoints com cadeado passam a enviar o token automaticamente.

A especificação fica em `http://localhost:5019/openapi/v1.json`, gerada pelo próprio ASP.NET Core
(`Microsoft.AspNetCore.OpenApi`); o pacote `Swashbuckle.AspNetCore.SwaggerUI` só desenha a tela.
Fora do ambiente de desenvolvimento, nenhum dos dois endereços existe.
 O arquivo
`src/Gastra.Api/Gastra.Api.http` tem as requisições prontas para testar pelo VS Code / Rider /
Visual Studio.

## Banco de dados (Entity Framework Core)

| Pacote | Versão | Projeto |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 9.0.20 | `Gastra.Infrastructure` |
| `Pomelo.EntityFrameworkCore.MySql` | 9.0.0 | `Gastra.Infrastructure` |
| `Microsoft.EntityFrameworkCore.Design` | 9.0.20 | `Gastra.Api` (usado pela ferramenta de migrations) |
| `dotnet-ef` (ferramenta local) | 9.0.20 | `dotnet-tools.json` |

**Por que EF Core 9 com a API em .NET 10:** o Pomelo não tem versão para EF Core 10 (a 9.0.0
aceita apenas EF Core 9.0.x). O EF Core 9 roda normalmente em .NET 10.

- O `GastraDbContext` fica em `Gastra.Infrastructure/DataAccess/`. As entidades ficam no
  `Domain` sem atributos do EF; o mapeamento de cada uma (`IEntityTypeConfiguration`) fica na
  `Infrastructure` e é aplicado automaticamente.
- A versão do MySQL é fixa (8.4) no registro do contexto. `ServerVersion.AutoDetect` não é usado
  porque conecta no banco na inicialização, e a API não subiria com o MySQL desligado.

### Primeira vez

1. Suba o MySQL (`infra/`, ver `infra/README.md`).
2. Restaure a ferramenta de migrations na versão do projeto (dentro de `backend/`):
   ```bash
   dotnet tool restore
   ```
   Dentro de `backend/`, `dotnet ef --version` deve mostrar `9.0.20`, mesmo que haja outra
   versão instalada globalmente.
3. Configure `src/Gastra.Api/appsettings.Development.json` (arquivo ignorado pelo Git):
   ```json
   {
     "ConnectionStrings": {
       "Gastra": "Server=localhost;Port=3307;Database=gastra_dev;User=gastra_app;Password=SUA_SENHA;"
     },
     "Jwt": {
       "ChaveAssinatura": "UMA_CHAVE_ALEATORIA_COM_PELO_MENOS_32_CARACTERES"
     },
     "Administrador": {
       "Nome": "Seu nome",
       "Email": "gerente@gastra.local",
       "Senha": "UMA_SENHA_FORTE"
     }
   }
   ```
   - `ConnectionStrings:Gastra`: mesmos valores do `infra/.env`.
   - `Jwt:ChaveAssinatura`: segredo que assina os tokens. **A API não inicia sem ela.** Para gerar
     uma: `python -c "import secrets; print(secrets.token_urlsafe(48))"`.
   - `Administrador`: cria o **primeiro Gerente** quando o banco não tem nenhum usuário (sem ele,
     ninguém consegue entrar para cadastrar os demais). Depois de criado, a seção é ignorada.

### Migrations

Executados dentro de `backend/`:

| Comando | O que faz |
|---|---|
| `dotnet ef migrations add NomeDaMigration --project src/Gastra.Infrastructure --startup-project src/Gastra.Api` | Cria uma migration a partir das mudanças no modelo |
| `dotnet ef database update --project src/Gastra.Infrastructure --startup-project src/Gastra.Api` | Aplica as migrations pendentes no banco |
| `dotnet ef migrations script --project src/Gastra.Infrastructure --startup-project src/Gastra.Api -o ../docs/modelagem/GASTRA_Schema.sql` | Gera o SQL completo do schema (modelo físico para a documentação) |

O `--project` aponta onde ficam o contexto e as migrations; o `--startup-project` é o projeto
que inicializa a aplicação e fornece a configuração.

## Autenticação e autorização

| Etapa | Endpoint | Quem |
|---|---|---|
| Login com e-mail e senha (UC01) | `POST /api/autenticacao/login` | todos |
| Vincular o app autenticador, uma única vez (RN07) | `POST /api/autenticacao/segundo-fator/configurar` | Gerente, Coordenador |
| Confirmar o código de 6 dígitos (UC02) | `POST /api/autenticacao/segundo-fator/confirmar` | Gerente, Coordenador |
| Encerrar sessão (UC03) | `POST /api/autenticacao/logoff` | usuário logado |

- **Garçom e Metre** recebem o token de acesso direto no login. **Gerente e Coordenador** (RF16)
  recebem só um token temporário (5 min), que não abre nenhum endpoint: o token de acesso vem depois
  do código do app autenticador (Google Authenticator, Microsoft Authenticator etc.). No primeiro
  acesso, a resposta do `configurar` traz a URI `otpauth://` (para gerar o QR code) e a chave manual.
- **Senha:** hash BCrypt com fator de custo 12 (RN06). Login com e-mail inexistente ou senha errada
  devolve a mesma mensagem, e o tempo de resposta é o mesmo nos dois casos.
- **Token:** JWT assinado com `Jwt:ChaveAssinatura`, válido por 8 horas. Enviar no cabeçalho
  `Authorization: Bearer <token>`.
- **Logoff e inativação valem na hora:** cada usuário tem uma `chave_sessao`; todo token a carrega,
  e a API confere a cada requisição. O logoff troca a chave, invalidando os tokens emitidos antes.
- **Segredo do TOTP criptografado** no banco com o Data Protection do ASP.NET Core. As chaves do Data
  Protection ficam no perfil do usuário da máquina (`%LOCALAPPDATA%\ASP.NET\DataProtection-Keys`):
  se forem apagadas, os autenticadores já vinculados precisam ser configurados de novo.
- **Permissões:** gestão do cardápio só para Gerente e Coordenador; `GET /api/cardapio/digital` e
  `GET /api/cardapio/{id}` são públicos (o cliente não faz login).

## Gestão de usuários (UC04)

Só o **Gerente** acessa (RF18). O primeiro Gerente vem da seção `Administrador`; os demais são
cadastrados por ele.

| Ação | Endpoint |
|---|---|
| Cadastrar conta (nome, e-mail, senha inicial, papel) | `POST /api/usuarios` |
| Listar contas, ativas e inativas | `GET /api/usuarios` |
| Consultar uma conta | `GET /api/usuarios/{id}` |
| Editar nome, e-mail e papel | `PUT /api/usuarios/{id}` |
| Inativar (soft delete) ou reativar | `PATCH /api/usuarios/{id}/situacao` |

- **Soft delete:** a conta inativada continua no banco (comandas e auditoria apontam para ela),
  mas perde o acesso na hora: o token em uso para de valer e o login é recusado.
- **Trocar o papel também derruba os tokens da pessoa**, porque o papel vai dentro do token: sem
  isso, um Gerente rebaixado a Garçom manteria as permissões de Gerente até o token expirar.
- **O Gerente não pode alterar o próprio papel nem inativar a própria conta**, para o restaurante
  não ficar sem ninguém que consiga gerenciar contas.
- **E-mail único**, sem diferenciar maiúsculas de minúsculas.
- **Senha:** mínimo de 8 caracteres e máximo de 72 bytes. O limite superior existe porque o BCrypt
  ignora o que passa de 72 bytes: uma senha maior seria aceita, mas só o começo dela valeria.
- A resposta nunca traz hash de senha nem segredo do autenticador.

## Salão: praças e mesas (UC24)

Cadastro do **Gerente** (RF23); a leitura vale para todo o salão, porque o garçom precisa da lista de
mesas para abrir comanda.

| Ação | Endpoint |
|---|---|
| Cadastrar praça | `POST /api/pracas` |
| Editar praça | `PUT /api/pracas/{id}` |
| Listar praças | `GET /api/pracas` *(qualquer usuário autenticado)* |
| Cadastrar mesa numa praça | `POST /api/mesas` |
| Editar número e capacidade da mesa | `PUT /api/mesas/{id}` |
| Listar mesas | `GET /api/mesas` *(qualquer usuário autenticado)* |

- **Toda mesa pertence a uma praça** (REL01), e esse vínculo **não muda**: o histórico de faturamento
  por praça, que alimenta a alocação (RN03), perderia o sentido se a mesa trocasse de lugar. Por isso
  a edição da mesa só altera número e capacidade.
- **Código da praça e número da mesa são únicos.**
- **`quantidadeGarcons`** é quantos garçons a praça comporta por turno: é entrada da programação
  linear da alocação.

## Núcleo de comandas (UC10–UC14, UC20, UC23)

Endpoints do **Garçom**, exceto onde indicado:

| Ação | Endpoint |
|---|---|
| UC10 — Abrir comanda (mesa + nº de pessoas) | `POST /api/comandas` |
| UC11 — Confirmar ou ajustar a composição | `PATCH /api/comandas/{id}/composicao` |
| UC12 — Registrar item do pedido | `POST /api/comandas/{id}/itens` |
| UC23 — Entregar ou cancelar item | `PATCH /api/comandas/{id}/itens/{itemId}/situacao` |
| UC13 — Registrar restrição alimentar | `POST /api/comandas/{id}/restricoes` |
| RF04 — Remover a taxa de serviço | `DELETE /api/comandas/{id}/taxa-servico` |
| UC14 — Fechar a comanda | `POST /api/comandas/{id}/fechamento` |
| Consultar comanda / painel do salão *(garçom, metre, coordenador, gerente)* | `GET /api/comandas/{id}` · `GET /api/comandas` |
| UC20 — Consulta do cliente por QR code *(sem login)* | `GET /api/comandas/consulta/{codigoAcesso}` |
| UC18 — Sugestões de itens para oferecer na mesa | `GET /api/comandas/{id}/sugestoes` |

- **Composição da mesa (RN01):** o garçom informa quantas pessoas estão na mesa e o sistema sugere
  Solo, Casal, Grupo pequeno, Família ou Grupo grande. Um item infantil numa mesa de 3 ou mais
  pessoas muda para Família — mas, se o garçom já tiver ajustado a composição na mão, a escolha dele
  prevalece.
- **Preço congelado (RF03):** o valor vai para o item do pedido no momento do lançamento; mudar o
  preço no cardápio depois não altera comanda aberta.
- **Fechamento (RN02):** só fecha sem itens pendentes. Cancelar exige motivo da lista fechada, e o
  item cancelado sai da conta. O total é subtotal + 10% de taxa, removível a pedido do cliente.
- **LGPD:** ao fechar, a observação livre da restrição é apagada e fica só a categoria. A consulta do
  cliente mostra itens e valores, nunca a restrição ou quem é o garçom (RN04).
- **Quem vê a restrição alimentar (RN04):** só Garçom e Metre, e só com a comanda aberta. Para
  Coordenador e Gerente, e depois do fechamento, `restricoes` vem vazia nas consultas e no painel.
  A regra fica no domínio (`Comanda.RestricoesVisiveisPara`), e o papel vem do token, sem consulta
  ao banco.
- **Código de acesso:** cada comanda recebe um código único, usado no QR code da mesa. Ele funciona
  como senha da conta, por isso nunca aparece em log.

## Sugestão de pratos (UC18, RF09)

`GET /api/comandas/{id}/sugestoes` devolve até 3 itens para o garçom oferecer, calculados pela camada
analítica em Python (regras de associação). O caminho é
`SugerirCombinacoesUseCase` → `IServicoAnalitico` (domínio) → `ServicoAnaliticoHttp` (infraestrutura) →
`POST /recomendacao/combinacoes` no FastAPI.

- **O backend decide o que pode ser oferecido; o Python só ordena.** Antes de chamar o serviço, o
  backend monta a lista de itens permitidos: disponíveis hoje (RF21), fora da comanda e compatíveis
  com a restrição registrada. Na volta, descarta qualquer id fora dessa lista.
- **Restrição alimentar:** vegano, vegetariano, sem glúten e sem lactose são conferidos pelas flags do
  cardápio. Na dúvida, o item não é sugerido: um suco sem a flag "vegano" não vai para uma mesa vegana.
  Alergia e "outro" só existem no texto livre, então não filtram nada e a resposta vem com
  `confirmarRestricaoComCliente = true`.
- **Python fora do ar não para o salão (D3, RNF05):** se o serviço não responde em 2 s, dá erro ou
  devolve algo inválido, a resposta é `200` com a lista vazia e `servicoDisponivel = false`. O motivo
  vai para o log técnico, sem dados da comanda.
- **Privacidade (RN05):** só os ids dos itens pedidos vão para o Python. Nenhum dado do cliente.

Configuração, em `appsettings.json` (sem segredo, por isso versionada):

```json
"ServicoAnalitico": {
  "UrlBase": "http://localhost:8000",
  "TempoLimiteMilissegundos": 2000
}
```

Para ver a sugestão de verdade, suba o serviço Python (`data-science/README.md`). Sem ele, a API
funciona normalmente e só a sugestão fica vazia.

**Teste de contrato com o Python.** `ContratoComPythonTests` aparece como ignorado no `dotnet test`
comum. Com o serviço Python no ar, rode:

```bash
GASTRA_TESTES_ANALITICA=http://localhost:8000 dotnet test
```

O teste chama o FastAPI de verdade pelo `ServicoAnaliticoHttp` e confere que os dois lados usam o
mesmo formato de requisição e de resposta.

## Alocação de garçons (UC15, UC21, UC22)

Endpoints do **Metre**, exceto a consulta:

| Ação | Endpoint |
|---|---|
| UC15 — Gerar a sugestão do turno | `POST /api/alocacoes/sugestao` com `data`, `periodo` (`Almoco` ou `Jantar`) e `garcomIds` |
| UC22 — Pôr um garçom numa praça (ajuste ou alocação manual) | `PUT /api/alocacoes/{data}/{periodo}/garcons/{garcomId}` com `pracaId` |
| UC21 — Confirmar o turno | `POST /api/alocacoes/{data}/{periodo}/confirmacao` |
| Quem está em qual praça *(qualquer usuário logado)* | `GET /api/alocacoes/{data}/{periodo}` |

- **Fatores da RN03 (`RegraDeDistribuicao`, no domínio):**
  - **faturamento acumulado do garçom:** soma dos 30 dias anteriores ao turno (`vw_desempenho_garcom_turno`);
  - **potencial da praça:** faturamento médio por turno (`vw_faturamento_medio_praca`);
  - **praça de alto potencial:** a que fatura acima da média das praças que já tiveram movimento;
  - **espera:** quantos turnos confirmados o garçom trabalhou desde a última vez numa praça de alto potencial.
- **Pesos:** w1 = 0,6 (desequilíbrio) e w2 = 0,4 (espera), até a calibração da #55.
- **Quem calcula:** o Python (`POST /alocacao/sugestao`). O backend confere a resposta antes de gravar:
  todos os garçons, praças existentes e vagas respeitadas. Resposta incoerente é tratada como serviço fora do ar.
- **Python fora do ar (D3):** a sugestão responde `200` com `servicoDisponivel = false` e não grava nada. O Metre
  aloca garçom por garçom pelo `PUT`.
- **Sugestão gerada de novo:** substitui a anterior, desde que o turno não tenha sido confirmado.
- **Depois de confirmado:** nenhum ajuste nem sugestão nova (`422`). O turno confirmado entra no histórico da RN03.
- **Auditoria (política de log, 4.5):** sugestão gerada (com os pesos), ajuste (praça sugerida → escolhida) e
  confirmação.

## Auditoria (política de log, #120)

Os casos de uso chamam `IRegistradorAuditoria.Registrar(evento, ...)`. O registro entra no **mesmo
Commit** da operação auditada: se a operação não é gravada, a auditoria também não é.

- **Quem agiu:** vem do token. No login, que ainda não tem token, o caso de uso informa a conta.
- **IP:** só em eventos de autenticação.
- **O que nunca vai para a auditoria:** senha, código ou segredo do autenticador, e-mail digitado sem conta
  correspondente, nome e e-mail de funcionário, código de acesso da comanda, categoria e observação da
  restrição alimentar. Os testes em `AuditoriaTests` conferem cada um.
- **Criações** (conta, item, praça, mesa, comanda) gravam em dois Commits, porque o id do registro só existe
  depois do primeiro.
- **Eliminação por prazo:** um serviço em segundo plano roda ao subir a API e uma vez por dia. Desligue com
  `"Auditoria": { "EliminacaoAutomatica": false }`.

## Configurações locais

`appsettings.Development.json` está no `.gitignore` de propósito: é onde ficam valores da sua
máquina (como a string de conexão do MySQL). Nunca commite senhas ou strings de conexão reais.
