import { Papel } from './sessao/modelos';

export interface ItemDeMenu {
  rotulo: string;
  rota: string;
  /** Nome do ícone em shared/icone. */
  icone: string;
  papeis: Papel[];
}

/**
 * Menu da casca da aplicação. Cada item aparece só para os papéis que podem usar a tela (RNF04); as mesmas listas
 * protegem as rotas em app.routes.ts, e a API confere de novo em cada endpoint.
 */
export const MENU: ItemDeMenu[] = [
  { rotulo: 'Comandas', icone: 'mesa', rota: '/comandas', papeis: ['Garcom'] },
  { rotulo: 'A entregar', icone: 'relogio', rota: '/a-entregar', papeis: ['Garcom'] },
  { rotulo: 'Desempenho', icone: 'trofeu', rota: '/desempenho', papeis: ['Garcom'] },
  { rotulo: 'Alocação', icone: 'pessoas', rota: '/alocacao', papeis: ['Metre'] },
  { rotulo: 'Salão agora', icone: 'mesa', rota: '/salao-agora', papeis: ['Metre'] },
  { rotulo: 'Análises', icone: 'faisca', rota: '/analises', papeis: ['Gerente'] },
  { rotulo: 'Cardápio', icone: 'etiqueta', rota: '/cardapio', papeis: ['Gerente', 'Coordenador'] },
  { rotulo: 'Salão', icone: 'mesa', rota: '/salao', papeis: ['Gerente'] },
  { rotulo: 'Usuários', icone: 'pessoas', rota: '/usuarios', papeis: ['Gerente'] },
];

export const NOME_DO_PAPEL: Record<Papel, string> = {
  Garcom: 'Garçom',
  Metre: 'Metre',
  Coordenador: 'Coordenador',
  Gerente: 'Gerente',
};
