import { Papel } from './sessao/modelos';

export interface ItemDeMenu {
  rotulo: string;
  rota: string;
  papeis: Papel[];
}

/**
 * Menu da casca da aplicação. Cada item aparece só para os papéis que podem usar a tela (RNF04); as mesmas listas
 * protegem as rotas em app.routes.ts, e a API confere de novo em cada endpoint.
 */
export const MENU: ItemDeMenu[] = [
  { rotulo: 'Comandas', rota: '/comandas', papeis: ['Garcom', 'Metre', 'Coordenador', 'Gerente'] },
  { rotulo: 'Alocação', rota: '/alocacao', papeis: ['Metre'] },
  { rotulo: 'Cardápio', rota: '/cardapio', papeis: ['Gerente', 'Coordenador'] },
  { rotulo: 'Análises', rota: '/analises', papeis: ['Gerente', 'Garcom'] },
  { rotulo: 'Salão', rota: '/salao', papeis: ['Gerente'] },
  { rotulo: 'Usuários', rota: '/usuarios', papeis: ['Gerente'] },
];

export const NOME_DO_PAPEL: Record<Papel, string> = {
  Garcom: 'Garçom',
  Metre: 'Metre',
  Coordenador: 'Coordenador',
  Gerente: 'Gerente',
};
