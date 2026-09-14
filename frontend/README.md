# Frontend — GASTRA

Aplicação web em Angular 22 (componentes standalone, SCSS, testes com Vitest).

## Pré-requisitos

- Node.js 24 e npm

Use sempre o Angular CLI **do projeto** (`npx ng ...` ou os scripts `npm run ...`), e não um
`ng` instalado globalmente: versões antigas do CLI não suportam o Node 24.

## Primeira vez

Dentro de `frontend/`:

```bash
npm install
```

Isso cria a pasta `node_modules/`, que não é versionada.

## Comandos

Todos executados dentro de `frontend/`.

| Comando | O que faz |
|---|---|
| `npm start` | Servidor de desenvolvimento em `http://localhost:4200`, com recarga automática ao salvar |
| `npm test -- --watch=false` | Roda os testes uma vez (sem `--watch=false`, fica vigiando os arquivos) |
| `npm run build` | Gera a versão otimizada em `dist/` |

## Estrutura

```
frontend/
├── public/            # Arquivos estáticos (favicon)
└── src/
    ├── index.html     # Página base
    ├── styles.scss    # Estilos globais
    └── app/
        ├── app.ts         # Componente principal (cabeçalho + área das telas)
        ├── app.html
        ├── app.scss
        ├── app.spec.ts    # Testes do componente principal
        ├── app.routes.ts  # Rotas das telas
        └── app.config.ts  # Configuração da aplicação
```
