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

Organizada **por módulo funcional** (issue #82): cada pasta de `features/` corresponde a um grupo de
casos de uso, e as telas de um módulo ficam juntas com seus serviços e testes.

```
frontend/
├── public/                # Arquivos estáticos (favicon)
└── src/
    ├── index.html         # Página base
    ├── styles.scss        # Estilos globais (tema)
    └── app/
        ├── app.ts             # Componente principal (cabeçalho + área das telas)
        ├── app.html
        ├── app.scss
        ├── app.spec.ts        # Testes do componente principal
        ├── app.routes.ts      # Rotas: cada módulo é carregado sob demanda
        ├── app.config.ts      # Configuração da aplicação
        ├── core/              # Serviços únicos da aplicação: sessão, interceptador do token, guardas por papel
        ├── shared/            # Componentes, pipes e modelos reutilizados por mais de um módulo
        └── features/
            ├── acesso/        # UC01–UC04: login, segundo fator, logoff, gestão de usuários
            ├── cardapio/      # UC05–UC09: itens do cardápio e promoções (Gerente, Coordenador)
            ├── comandas/      # UC10–UC14: abrir mesa, pedidos, restrição, fechamento (Garçom)
            ├── alocacao/      # UC15, UC21, UC22: sugestão e confirmação da alocação (Metre)
            ├── analises/      # UC16–UC18: relatórios de BI, índice de desempenho, sugestão de pratos
            └── cliente/       # UC19–UC20: cardápio digital e consulta da comanda (sem login)
```

### Regras de dependência

| Pasta | Pode importar de |
|---|---|
| `features/<módulo>` | `core`, `shared` e o próprio módulo — **nunca outro módulo** |
| `shared` | nada da aplicação (só Angular e bibliotecas) |
| `core` | `shared` |

Um módulo não importa outro para que cada grupo de telas possa mudar sem quebrar os demais. O que
dois módulos precisam em comum sobe para `shared` (componente visual) ou `core` (serviço único, como
a sessão do usuário).

**Por que `cliente` é separado de `cardapio`:** as telas do cliente são públicas e pensadas para
celular; as do cardápio são de gestão e exigem login de Gerente ou Coordenador. Separar evita que a
área pública carregue código e rotas da área administrativa.
