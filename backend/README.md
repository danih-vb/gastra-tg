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

## Configurações locais

`appsettings.Development.json` está no `.gitignore` de propósito: é onde ficam valores da sua
máquina (como a string de conexão do MySQL). Nunca commite senhas ou strings de conexão reais.
