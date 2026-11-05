import { Routes } from '@angular/router';
import { exigePapel, paraPaginaInicial } from './core/sessao/guardas';

const emConstrucao = () => import('./shared/em-construcao/em-construcao').then((m) => m.EmConstrucao);

/**
 * Cada módulo é carregado sob demanda e protegido pelos papéis do menu (core/navegacao.ts). As telas dos módulos
 * ainda são marcadores: entram conforme o protótipo (#47).
 */
export const routes: Routes = [
  { path: '', pathMatch: 'full', canActivate: [paraPaginaInicial], children: [] },

  // UC01–UC03
  { path: 'acesso/login', title: 'Entrar — GASTRA', loadComponent: () => import('./features/acesso/login/login').then((m) => m.Login) },
  {
    path: 'acesso/segundo-fator',
    title: 'Verificação em duas etapas — GASTRA',
    loadComponent: () => import('./features/acesso/segundo-fator/segundo-fator').then((m) => m.SegundoFator),
  },

  // UC10–UC14, UC18, UC23 — telas do Garçom.
  {
    path: 'comandas',
    title: 'Mesas — GASTRA',
    canActivate: [exigePapel('Garcom')],
    loadComponent: () => import('./features/comandas/mesas/mesas').then((m) => m.Mesas),
  },
  {
    path: 'comandas/:id',
    title: 'Comanda — GASTRA',
    canActivate: [exigePapel('Garcom')],
    loadComponent: () => import('./features/comandas/comanda/comanda').then((m) => m.Comanda),
  },
  {
    path: 'comandas/:id/fechar',
    title: 'Fechar conta — GASTRA',
    canActivate: [exigePapel('Garcom')],
    loadComponent: () => import('./features/comandas/fechar/fechar-conta').then((m) => m.FecharConta),
  },
  {
    path: 'a-entregar',
    title: 'A entregar — GASTRA',
    canActivate: [exigePapel('Garcom')],
    loadComponent: () => import('./features/comandas/pendencias/pendencias').then((m) => m.Pendencias),
  },
  {
    path: 'desempenho',
    title: 'Seu desempenho — GASTRA',
    canActivate: [exigePapel('Garcom')],
    loadComponent: () => import('./features/analises/desempenho/desempenho').then((m) => m.Desempenho),
  },
  // UC15, UC21, UC22 — telas do Metre.
  {
    path: 'alocacao',
    title: 'Alocação — GASTRA',
    canActivate: [exigePapel('Metre')],
    loadComponent: () => import('./features/alocacao/alocacao').then((m) => m.Alocacao),
  },
  {
    path: 'salao-agora',
    title: 'Salão agora — GASTRA',
    canActivate: [exigePapel('Metre')],
    loadComponent: () => import('./features/salao/salao-agora').then((m) => m.SalaoAgora),
  },
  // UC05–UC09 — cardápio e promoções (Gerente e Coordenador).
  {
    path: 'cardapio',
    title: 'Cardápio — GASTRA',
    canActivate: [exigePapel('Gerente', 'Coordenador')],
    loadComponent: () => import('./features/cardapio/gestao-cardapio').then((m) => m.GestaoCardapio),
  },
  // UC16, UC17 — relatórios de BI.
  {
    path: 'analises',
    title: 'Análises — GASTRA',
    canActivate: [exigePapel('Gerente')],
    loadComponent: () => import('./features/analises/painel/analises').then((m) => m.Analises),
  },
  // UC24 e UC04 — salão e contas (Gerente).
  {
    path: 'salao',
    title: 'Salão — GASTRA',
    canActivate: [exigePapel('Gerente')],
    loadComponent: () => import('./features/salao/gestao-salao').then((m) => m.GestaoSalao),
  },
  {
    path: 'usuarios',
    title: 'Usuários — GASTRA',
    canActivate: [exigePapel('Gerente')],
    loadComponent: () => import('./features/usuarios/gestao-usuarios').then((m) => m.GestaoUsuarios),
  },

  // UC19, UC20 — telas públicas: o cliente chega pelo QR code da mesa e não faz login.
  {
    path: 'cliente',
    title: 'GASTRA',
    loadComponent: () => import('./features/cliente/inicio/inicio-do-cliente').then((m) => m.InicioDoCliente),
  },
  {
    path: 'cliente/cardapio',
    title: 'Cardápio — GASTRA',
    loadComponent: () => import('./features/cliente/cardapio-digital/cardapio-digital').then((m) => m.CardapioDigital),
  },
  {
    path: 'cliente/conta/:codigo',
    title: 'Sua conta — GASTRA',
    loadComponent: () => import('./features/cliente/conta/conta-do-cliente').then((m) => m.ContaDoCliente),
  },
  { path: 'sem-permissao', title: 'Sem permissão — GASTRA', loadComponent: () => import('./shared/acesso-negado/acesso-negado').then((m) => m.AcessoNegado) },
  { path: '**', redirectTo: '' },
];
