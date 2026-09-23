import { Papel } from '../sessao/modelos';

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

/** RF25 — avaliação do atendimento. Anônima: não existe campo de identificação, e é de propósito (RN08). */
export interface NovaAvaliacao {
  nota: number;
  comentario?: string;
}

/** RF25 — o que o Gerente vê das avaliações: só agregado, sem comentário e sem garçom. */
export interface RelatorioAvaliacoes {
  periodo: Periodo;
  quantidade: number;
  media: number;
  distribuicao: { nota: number; quantidade: number; percentual: number }[];
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

/** UC20 — o que o cliente vê da própria conta: nada de garçom, restrição ou outra mesa (RN04). */
export interface ComandaDoCliente {
  mesa: string;
  dataHoraAbertura: string;
  fechada: boolean;
  itens: { nome: string; quantidade: number; valor: number; situacao: StatusItemPedido }[];
  subtotal: number;
  taxaServico: number;
  total: number;
  /** RF25: a tela só oferece avaliar com a conta fechada, sem avaliação e dentro do prazo. */
  podeAvaliar: boolean;
  avaliacaoEnviada: boolean;
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
  /** Média bayesiana das avaliações (1 a 5); nula abaixo do mínimo de avaliações (RN08). */
  notaConsiderada: number | null;
}

export interface RankingDeDesempenho {
  periodo: { inicio: string; fim: string };
  pesoFaturamento: number;
  pesoMesasAtendidas: number;
  /** Zero quando ninguém teve o mínimo de avaliações no período. */
  pesoAvaliacao: number;
  minimoDeAvaliacoes: number;
  totalNoRanking: number;
  posicoes: PosicaoNoRanking[];
}

// --- Gerente: relatórios de BI (UC16) ---

export interface Periodo {
  inicio: string;
  fim: string;
}

export interface IndicadorGarcom {
  garcomId: number;
  nome: string;
  faturamento: number;
  comandas: number;
  turnos: number;
  faturamentoPorTurno: number;
  mesasAtendidas: number;
  ticketMedio: number;
  tempoMedioAtendimentoMinutos: number;
}

export interface RelatorioGarcons {
  periodo: Periodo;
  garcons: IndicadorGarcom[];
  totais: { faturamento: number; comandas: number; ticketMedio: number; tempoMedioAtendimentoMinutos: number };
}

export interface IndicadorPraca {
  pracaId: number;
  codigo: string;
  faturamento: number;
  comandas: number;
  turnosComMovimento: number;
  faturamentoPorTurno: number;
  ticketMedio: number;
  faturamentoMedioHistorico: number;
}

export interface RelatorioPracas {
  periodo: Periodo;
  pracas: IndicadorPraca[];
}

export interface IndicadorItem {
  itemCardapioId: number;
  nome: string;
  categoria: string;
  quantidade: number;
  faturamento: number;
  participacaoPercentual: number;
}

export interface RelatorioCardapio {
  periodo: Periodo;
  itens: IndicadorItem[];
  categorias: { categoria: string; quantidade: number; faturamento: number; participacaoPercentual: number }[];
}

export interface RelatorioHorarios {
  periodo: Periodo;
  porHora: { pracaId: number; pracaCodigo: string; hora: number; faturamento: number; comandas: number }[];
  porDiaDaSemana: { pracaId: number; pracaCodigo: string; diaDaSemana: number; nomeDoDia: string; faturamento: number; comandas: number }[];
}

// --- Gerente e Coordenador: cardápio e promoções (UC05–UC09) ---

export type TipoDesconto = 'Percentual' | 'ValorFixo';

export interface NovoItemDoCardapio {
  nome: string;
  categoria: CategoriaItemCardapio;
  preco: number;
  descricao: string;
  flagsDieteticas: FlagDietetica[];
  imagem: string | null;
}

export interface Promocao {
  id: number;
  descricao: string;
  tipoDesconto: TipoDesconto;
  valorDesconto: number;
  dataInicio: string;
  dataFim: string;
  ativa: boolean;
  vigenteHoje: boolean;
  itens: { itemCardapioId: number; nome: string; precoOriginal: number; precoComDesconto: number }[];
}

export interface NovaPromocao {
  descricao: string;
  tipoDesconto: TipoDesconto;
  valorDesconto: number;
  dataInicio: string;
  dataFim: string;
  itemCardapioIds: number[];
}

// --- Gerente: salão e contas (UC24, UC04) ---

export interface NovaPraca {
  codigo: string;
  quantidadeGarcons: number;
}

export interface NovaMesa {
  numero: string;
  capacidade: number;
  pracaId: number;
}

export interface Usuario {
  id: number;
  nome: string;
  email: string;
  papel: Papel;
  ativo: boolean;
  segundoFatorConfigurado: boolean;
}

export interface NovoUsuario {
  nome: string;
  email: string;
  senha: string;
  papel: Papel;
}
