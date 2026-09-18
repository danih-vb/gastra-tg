/**
 * Tipos das respostas e requisições da API (backend/src/Gastra.Communication). Os nomes seguem os do backend, para
 * a comparação com os contratos ser direta.
 */

export type CategoriaItemCardapio = 'Entrada' | 'PratoPrincipal' | 'Sobremesa' | 'Bebida';
export type FlagDietetica = 'Vegano' | 'Vegetariano' | 'SemGluten' | 'SemLactose' | 'OpcaoInfantil';
export type CategoriaRestricao = 'Vegano' | 'Vegetariano' | 'SemGluten' | 'SemLactose' | 'Alergia' | 'Outro';
export type MotivoCancelamento = 'ErroDeLancamento' | 'ClienteDesistiu' | 'ItemEmFalta';
export type ComposicaoMesa = 'Solo' | 'Casal' | 'GrupoPequeno' | 'Familia' | 'GrupoGrande';
export type StatusComanda = 'Aberta' | 'Fechada';
export type StatusItemPedido = 'Pendente' | 'Entregue' | 'Cancelado';
export type PeriodoAlocacao = 'Almoco' | 'Jantar';

export interface ItemCardapio {
  id: number;
  nome: string;
  categoria: CategoriaItemCardapio;
  preco: number;
  /** Preço com a melhor promoção vigente; nulo quando o item não está em promoção. */
  precoPromocional: number | null;
  descricao: string;
  flagsDieteticas: FlagDietetica[];
  disponivel: boolean;
  imagem: string | null;
}

export interface Praca {
  id: number;
  codigo: string;
  quantidadeGarcons: number;
}

export interface Mesa {
  id: number;
  numero: string;
  capacidade: number;
  pracaId: number;
  pracaCodigo: string;
}

export interface ItemDoPedido {
  id: number;
  itemDoCardapioId: number;
  nome: string;
  quantidade: number;
  precoUnitarioNoMomento: number;
  valor: number;
  status: StatusItemPedido;
  motivoCancelamento: MotivoCancelamento | null;
  dataHoraRegistro: string;
}

export interface RestricaoAlimentar {
  id: number;
  categoria: CategoriaRestricao;
  observacaoLivre: string | null;
}

export interface Comanda {
  id: number;
  mesaId: number;
  garcomId: number;
  garcomNome: string;
  status: StatusComanda;
  quantidadePessoas: number;
  composicao: ComposicaoMesa;
  composicaoAjustadaManualmente: boolean;
  taxaServicoRemovida: boolean;
  codigoAcessoCliente: string;
  dataHoraAbertura: string;
  dataHoraFechamento: string | null;
  itens: ItemDoPedido[];
  /** Só chega para Garçom e Metre, com a mesa aberta (RN04). */
  restricoes: RestricaoAlimentar[];
  subtotal: number;
  taxaServico: number;
  total: number;
}

export interface ItemSugerido {
  itemDoCardapioId: number;
  nome: string;
  categoria: CategoriaItemCardapio;
  preco: number;
}

export interface SugestoesDaComanda {
  servicoDisponivel: boolean;
  /** A mesa tem alergia ou "outro": o cardápio não confere sozinho, o garçom confirma com o cliente. */
  confirmarRestricaoComCliente: boolean;
  itens: ItemSugerido[];
}

export interface DesignacaoDoTurno {
  garcomId: number;
  garcomNome: string;
  pracaId: number;
  pracaCodigo: string;
}

export interface AlocacaoDoTurno {
  data: string;
  periodo: PeriodoAlocacao;
  confirmada: boolean;
  servicoDisponivel: boolean | null;
  designacoes: DesignacaoDoTurno[];
}

/** UC15 — garçom que pode entrar no turno (só id e nome, RNF03). */
export interface GarcomDoTurno {
  id: number;
  nome: string;
}

export interface PosicaoNoRanking {
  posicao: number;
  garcomId: number;
  nome: string;
  indice: number;
  faturamentoPorTurno: number;
  mesasPorTurno: number;
  turnos: number;
}

export interface RankingDeDesempenho {
  periodo: { inicio: string; fim: string };
  pesoFaturamento: number;
  pesoMesasAtendidas: number;
  totalNoRanking: number;
  posicoes: PosicaoNoRanking[];
}
