# Backend — GASTRA

API em ASP.NET Core (.NET 10), com controllers.

## Pré-requisitos

- .NET SDK 10 — a versão é fixada pelo `global.json` desta pasta. Confira com
  `dotnet --version` dentro de `backend/`.

## Estrutura

```
backend/
├── global.json                 # Fixa o SDK do .NET 10
├── Gastra.slnx                 # Solução
├── src/
│   └── Gastra.Api/             # API (controllers, Program.cs, configurações)
└── tests/
    └── Gastra.Api.Tests/       # Testes (xUnit + WebApplicationFactory)
```

A organização em camadas (domínio, serviços, repositórios) será definida junto com a
arquitetura do sistema (issue #46). Por enquanto, um único projeto de API.

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
