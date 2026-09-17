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

  {
    path: 'comandas',
    title: 'Comandas — GASTRA',
    canActivate: [exigePapel('Garcom', 'Metre', 'Coordenador', 'Gerente')],
    loadComponent: emConstrucao,
    data: { titulo: 'Comandas', casosDeUso: 'UC10–UC14, UC18, UC23' },
  },
  {
    path: 'alocacao',
    title: 'Alocação — GASTRA',
    canActivate: [exigePapel('Metre')],
    loadComponent: emConstrucao,
    data: { titulo: 'Alocação de garçons', casosDeUso: 'UC15, UC21, UC22' },
  },
  {
    path: 'cardapio',
    title: 'Cardápio — GASTRA',
    canActivate: [exigePapel('Gerente', 'Coordenador')],
    loadComponent: emConstrucao,
    data: { titulo: 'Cardápio e promoções', casosDeUso: 'UC05–UC09' },
  },
  {
    path: 'analises',
    title: 'Análises — GASTRA',
    canActivate: [exigePapel('Gerente', 'Garcom')],
    loadComponent: emConstrucao,
    data: { titulo: 'Relatórios e desempenho', casosDeUso: 'UC16, UC17' },
  },
  {
    path: 'salao',
    title: 'Salão — GASTRA',
    canActivate: [exigePapel('Gerente')],
    loadComponent: emConstrucao,
    data: { titulo: 'Praças e mesas', casosDeUso: 'UC24' },
  },
  {
    path: 'usuarios',
    title: 'Usuários — GASTRA',
    canActivate: [exigePapel('Gerente')],
    loadComponent: emConstrucao,
    data: { titulo: 'Contas de usuário', casosDeUso: 'UC04' },
  },

  // UC19, UC20 — o cliente não faz login.
  {
    path: 'cliente',
    title: 'Cardápio — GASTRA',
    loadComponent: emConstrucao,
    data: { titulo: 'Cardápio digital e conta da mesa', casosDeUso: 'UC19, UC20' },
  },

  { path: 'sem-permissao', title: 'Sem permissão — GASTRA', loadComponent: () => import('./shared/acesso-negado/acesso-negado').then((m) => m.AcessoNegado) },
  { path: '**', redirectTo: '' },
];
