import { Papel } from '../core/sessao/modelos';
import {
  CategoriaItemCardapio,
  CategoriaRestricao,
  ComposicaoMesa,
  FlagDietetica,
  MotivoCancelamento,
} from '../core/api/modelos';

/** Nomes em português das listas fechadas da API, num lugar só. */

export const CATEGORIAS: { chave: CategoriaItemCardapio; nome: string }[] = [
  { chave: 'Entrada', nome: 'Entradas' },
  { chave: 'PratoPrincipal', nome: 'Pratos' },
  { chave: 'Sobremesa', nome: 'Sobremesas' },
  { chave: 'Bebida', nome: 'Bebidas' },
];

export const FLAGS: Record<FlagDietetica, string> = {
  Vegano: 'Vegano',
  Vegetariano: 'Vegetariano',
  SemGluten: 'Sem glúten',
  SemLactose: 'Sem lactose',
  OpcaoInfantil: 'Infantil',
};

export const RESTRICOES: Record<CategoriaRestricao, string> = {
  Vegano: 'Vegano',
  Vegetariano: 'Vegetariano',
  SemGluten: 'Sem glúten',
  SemLactose: 'Sem lactose',
  Alergia: 'Alergia',
  Outro: 'Outro',
};

export const MOTIVOS: Record<MotivoCancelamento, string> = {
  ErroDeLancamento: 'Erro de lançamento',
  ClienteDesistiu: 'Cliente desistiu',
  ItemEmFalta: 'Item em falta',
};

export const PAPEIS: Record<Papel, string> = {
  Gerente: 'Gerente',
  Coordenador: 'Coordenador',
  Metre: 'Metre',
  Garcom: 'Garçom',
};

export const COMPOSICOES: Record<ComposicaoMesa, string> = {
  Solo: 'Solo',
  Casal: 'Casal',
  GrupoPequeno: 'Grupo pequeno',
  Familia: 'Família',
  GrupoGrande: 'Grupo grande',
};

/**
 * Mesma classificação da RN01 no domínio (Comanda.Classificar). Aqui ela serve só para *mostrar* a sugestão antes
 * de abrir a mesa; quem decide continua sendo a API.
 */
export function composicaoSugerida(pessoas: number, temItemInfantil = false): ComposicaoMesa {
  if (pessoas <= 1) return 'Solo';
  if (pessoas === 2) return 'Casal';
  if (temItemInfantil) return 'Familia';
  return pessoas <= 4 ? 'GrupoPequeno' : 'GrupoGrande';
}

/** "há 12 min", "agora": tempo desde um instante devolvido pela API. */
export function minutosDesde(dataIso: string, agora = Date.now()): number {
  return Math.max(0, Math.floor((agora - new Date(dataIso).getTime()) / 60000));
}

export function tempoRelativo(dataIso: string, agora = Date.now()): string {
  const minutos = minutosDesde(dataIso, agora);
  if (minutos < 1) return 'agora';
  if (minutos < 60) return `há ${minutos} min`;
  const horas = Math.floor(minutos / 60);
  return horas === 1 ? 'há 1 h' : `há ${horas} h`;
}
