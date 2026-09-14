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

Com a API rodando, `http://localhost:5019/health` deve responder `Healthy`. O arquivo
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
3. Configure a string de conexão em `src/Gastra.Api/appsettings.Development.json` (arquivo
   ignorado pelo Git), com os mesmos valores do `infra/.env`:
   ```json
   {
     "ConnectionStrings": {
       "Gastra": "Server=localhost;Port=3307;Database=gastra_dev;User=gastra_app;Password=SUA_SENHA;"
     }
   }
   ```

### Migrations

Executados dentro de `backend/`:

| Comando | O que faz |
|---|---|
| `dotnet ef migrations add NomeDaMigration --project src/Gastra.Infrastructure --startup-project src/Gastra.Api` | Cria uma migration a partir das mudanças no modelo |
| `dotnet ef database update --project src/Gastra.Infrastructure --startup-project src/Gastra.Api` | Aplica as migrations pendentes no banco |
| `dotnet ef migrations script --project src/Gastra.Infrastructure --startup-project src/Gastra.Api -o ../docs/modelagem/GASTRA_Schema.sql` | Gera o SQL completo do schema (modelo físico para a documentação) |

O `--project` aponta onde ficam o contexto e as migrations; o `--startup-project` é o projeto
que inicializa a aplicação e fornece a configuração.

## Configurações locais

`appsettings.Development.json` está no `.gitignore` de propósito: é onde ficam valores da sua
máquina (como a string de conexão do MySQL). Nunca commite senhas ou strings de conexão reais.
