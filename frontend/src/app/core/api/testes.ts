import { Comanda, ItemCardapio, ItemDoPedido, Mesa, Praca } from './modelos';

/** Objetos prontos para os testes das telas, com os mesmos campos que a API devolve. */

export const API = 'http://localhost:5019';

export function praca(id: number, codigo: string): Praca {
  return { id, codigo, quantidadeGarcons: 2 };
}

export function mesa(id: number, numero: string, pracaId = 1, capacidade = 4): Mesa {
  return { id, numero, capacidade, pracaId, pracaCodigo: pracaId === 1 ? 'A' : 'B' };
}

export function itemCardapio(dados: Partial<ItemCardapio> = {}): ItemCardapio {
  return {
    id: 1,
    nome: 'Risoto de cogumelos',
    categoria: 'PratoPrincipal',
    preco: 68,
    precoPromocional: null,
    descricao: 'Arbóreo com shiitake',
    flagsDieteticas: ['Vegetariano'],
    disponivel: true,
    imagem: null,
    ...dados,
  };
}

export function itemDoPedido(dados: Partial<ItemDoPedido> = {}): ItemDoPedido {
  return {
    id: 90,
    itemDoCardapioId: 1,
    nome: 'Risoto de cogumelos',
    quantidade: 1,
    precoUnitarioNoMomento: 68,
    valor: 68,
    status: 'Pendente',
    motivoCancelamento: null,
    dataHoraRegistro: new Date().toISOString(),
    ...dados,
  };
}

export function comanda(dados: Partial<Comanda> = {}): Comanda {
  const itens = dados.itens ?? [];
  const subtotal = itens.filter((i) => i.status !== 'Cancelado').reduce((total, i) => total + i.valor, 0);
  const taxa = dados.taxaServicoRemovida ? 0 : Math.round(subtotal * 10) / 100;
  return {
    id: 501,
    mesaId: 10,
    garcomId: 11,
    garcomNome: 'Carla Mendes',
    status: 'Aberta',
    quantidadePessoas: 2,
    composicao: 'Casal',
    composicaoAjustadaManualmente: false,
    taxaServicoRemovida: false,
    codigoAcessoCliente: 'K7P2QX',
    dataHoraAbertura: new Date().toISOString(),
    dataHoraFechamento: null,
    itens,
    restricoes: [],
    subtotal,
    taxaServico: taxa,
    total: subtotal + taxa,
    ...dados,
  };
}

/** Sessão de um garçom já logado, do jeito que o SessaoService guarda. */
export function sessaoDoGarcom(id = 11, nome = 'Carla Mendes'): Storage {
  const dados = new Map<string, string>([
    ['gastra.sessao', JSON.stringify({ token: 'token-de-teste', id, nome, papel: 'Garcom', expiraEm: Date.now() + 3_600_000 })],
  ]);
  return {
    get length() {
      return dados.size;
    },
    clear: () => dados.clear(),
    getItem: (chave: string) => dados.get(chave) ?? null,
    key: (indice: number) => [...dados.keys()][indice] ?? null,
    removeItem: (chave: string) => void dados.delete(chave),
    setItem: (chave: string, valor: string) => void dados.set(chave, valor),
  };
}
