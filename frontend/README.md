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

## Sessão e comunicação com a API

A API roda em `http://localhost:5019` (valor padrão do token `URL_DA_API`, em `core/configuracao.ts`).
Ela só aceita chamadas do navegador vindas das origens listadas em `Cors:OrigensPermitidas` no
`appsettings.json` do backend (por padrão, `http://localhost:4200`).

| Arquivo | Responsabilidade |
|---|---|
| `core/sessao/sessao.service.ts` | Login, segundo fator (configurar e confirmar), logoff e o usuário logado (nome, papel, validade do token) |
| `core/sessao/autenticacao.interceptor.ts` | Envia o token **só** para a API; num 401 fora do login, encerra a sessão e volta ao login |
| `core/sessao/guardas.ts` | `exigePapel(...)` em cada rota e a página inicial de cada papel |
| `core/navegacao.ts` | Menu: quais telas cada papel vê |

### Fluxo de login (UC01–UC03)

1. `/acesso/login` envia e-mail e senha.
2. Garçom e Metre recebem o token e vão para a página inicial do papel.
3. Gerente e Coordenador (RF16) recebem um token temporário e vão para `/acesso/segundo-fator`:
   no primeiro acesso a tela mostra a chave para cadastrar no aplicativo autenticador
   (exibida uma única vez); nos seguintes, pede só o código de 6 dígitos.
4. **Sair** chama `POST /api/autenticacao/logoff` (a API invalida o token) e limpa a sessão no
   navegador mesmo que a chamada falhe.

### Decisões

- **`sessionStorage`, não `localStorage`:** a sessão acaba ao fechar a aba, o que combina com
  tablets e computadores compartilhados do salão. O token também não vai para cookie, e o
  interceptador nunca o anexa a outros endereços.
- **O menu esconde, a API bloqueia:** as guardas e o menu só evitam telas inúteis para o papel.
  Quem garante a permissão é a API (`[Authorize(Roles = ...)]`); o frontend nunca é a barreira de
  segurança.
- **Parâmetro `voltar` do login:** só aceita caminhos internos (começando com `/` e não com `//`),
  para o login não servir de redirecionamento para sites externos.
- As telas dos módulos ainda são marcadores (`shared/em-construcao`) e serão feitas a partir do
  protótipo (#47).

## Telas do Garçom (#145)

Primeiro módulo de telas, feito a partir do protótipo (`docs/ux-ui/prototipo/garcom.html`).

| Rota | Tela | Casos de uso |
|---|---|---|
| `/comandas` | Mesas do turno, por praça | UC10 |
| `/comandas/:id` | Comanda: pendências, sugestões, lançamento, restrição, cancelamento | UC11–UC13, UC18, UC23 |
| `/comandas/:id/fechar` | Fechamento com taxa de serviço | UC14 |
| `/a-entregar` | Pendências de todas as mesas do garçom | RF03 |
| `/desempenho` | Posição e índice do próprio garçom | UC17 |

- **`core/api/`:** `GastraApiService` concentra as chamadas, e `modelos.ts` traz os tipos das respostas, com os mesmos nomes do backend.
- **`shared/`:** `app-folha` (painel que sobe de baixo), `AvisoService` + `app-avisos` (aviso curto com "Desfazer"), `app-icone` e `rotulos.ts` (nomes em português das listas fechadas).
- **Estado:** cada ação chama a API e recarrega a comanda. A tela não recalcula total, taxa nem composição: quem decide é o servidor.

### Decisões

- **Desfazer em vez de confirmar** nas ações do dia a dia. Lançar um item mostra "Desfazer", que **cancela o item com o motivo "erro de lançamento"** — é o caminho que a API oferece. Entregar não tem volta na API, então o aviso aparece sem "Desfazer". Confirmação em painel fica só para fechar a conta.
- **Dois toques para abrir a mesa (RNF02):** tocar no número de pessoas já confirma a composição sugerida (RN01). Lançar um item leva outros três: categoria, item e "Lançar".
- **Minha praça** vem da alocação confirmada do turno (`GET /api/alocacoes/{data}/{periodo}`). Sem alocação, a tela mostra todas as praças e explica o motivo, em vez de ficar vazia.
- **Falha do serviço de análise (D3):** as sugestões somem com um aviso e o resto da comanda continua.

### Rodar contra a API

```bash
npm start
```

Com a API em `http://localhost:5019` (`dotnet run --project backend/src/Gastra.Api`) e um Gerente que já tenha cadastrado praças, mesas, cardápio e o garçom.

## Telas do Metre (#149)

| Rota | Tela | Casos de uso |
|---|---|---|
| `/alocacao` | Presença do turno → sugestão por praça → confirmação | UC15, UC21, UC22 |
| `/salao-agora` | Comandas abertas por praça, somente leitura | RN04 |

- **Três etapas visíveis:** quem está no turno, revisar e confirmada. A tela guarda a sugestão que veio da API para marcar o que o Metre mudou depois ("Ajustado", com a praça sugerida).
- **Troca de praça (#140):** quando a praça de destino está cheia — o caso comum, com um garçom para cada vaga — a tela pede com quem trocar e manda `trocarComGarcomId`.
- **Sem o serviço de análise (D3):** a API responde `servicoDisponivel: false` com a lista vazia; todos aparecem em "Sem praça" e o Metre distribui à mão.
- **Sem números de faturamento:** a tela explica o critério da RN03 em palavras. Os valores por praça e por garçom são do Gerente (RNF04), e o Metre não tem acesso a eles.
- **Menu:** em tela larga ele sobe para junto da barra; no celular fica no rodapé.

## Telas do Cliente (#151)

Públicas: o cliente chega pelo QR code da mesa e **não faz login**.

| Rota | Tela | Casos de uso |
|---|---|---|
| `/cliente` | Início: cardápio ou conta pelo código da mesa | — |
| `/cliente/cardapio` | Cardápio digital com filtros dietéticos | UC19, RF05 |
| `/cliente/conta/:codigo` | Conta da mesa em tempo real | UC20, RF13 |

- **Rotas sem guarda e sem token:** o interceptador só manda o token quando existe sessão, então estas telas funcionam com o navegador limpo.
- **Filtros combinam entre si:** "vegetariano e sem glúten" mostra só o que atende aos dois, com estado vazio explicando o que fazer.
- **A conta se atualiza sozinha** a cada 15 segundos enquanto a mesa está aberta, e para quando ela fecha. Também dá para atualizar na mão.
- **Código inexistente não é erro de sistema:** a tela diz para conferir o código na mesa ou chamar o garçom.
- **Sem dado pessoal:** a resposta do cliente já não traz garçom nem restrição alimentar (RN04), e a tela diz isso ao cliente.
