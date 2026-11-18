/*
 * GASTRA — dados fictícios do protótipo. Os nomes dos campos e as listas fechadas seguem os contratos
 * da API (backend/src/Gastra.Communication), para a implementação em Angular reaproveitar o desenho.
 */
(function () {
  'use strict';

  const CATEGORIAS = [
    { chave: 'Entrada', nome: 'Entradas' },
    { chave: 'PratoPrincipal', nome: 'Pratos' },
    { chave: 'Sobremesa', nome: 'Sobremesas' },
    { chave: 'Bebida', nome: 'Bebidas' },
  ];

  const FLAGS = {
    Vegano: 'Vegano',
    Vegetariano: 'Vegetariano',
    SemGluten: 'Sem glúten',
    SemLactose: 'Sem lactose',
    OpcaoInfantil: 'Infantil',
  };

  // RF14: lista fechada + observação livre.
  const RESTRICOES = {
    Vegano: 'Vegano',
    Vegetariano: 'Vegetariano',
    SemGluten: 'Sem glúten',
    SemLactose: 'Sem lactose',
    Alergia: 'Alergia',
    Outro: 'Outro',
  };

  // RN02: cancelamento só com justificativa da lista fechada.
  const MOTIVOS_CANCELAMENTO = {
    ErroDeLancamento: 'Erro de lançamento',
    ClienteDesistiu: 'Cliente desistiu',
    ItemEmFalta: 'Item em falta',
  };

  // RN01
  const COMPOSICOES = {
    Solo: 'Solo',
    Casal: 'Casal',
    GrupoPequeno: 'Grupo pequeno',
    Familia: 'Família',
    GrupoGrande: 'Grupo grande',
  };

  /** Mesma regra de Comanda.Classificar no domínio (RN01). */
  function classificar(pessoas, temItemInfantil) {
    if (pessoas === 1) return 'Solo';
    if (pessoas === 2) return 'Casal';
    if (pessoas >= 3 && temItemInfantil) return 'Familia';
    if (pessoas <= 4) return 'GrupoPequeno';
    return 'GrupoGrande';
  }

  const PAPEIS = { Gerente: 'Gerente', Coordenador: 'Coordenador', Metre: 'Metre', Garcom: 'Garçom' };

  const cardapio = [
    { id: 1, nome: 'Bolinho de bacalhau (6 un.)', categoria: 'Entrada', preco: 42.0, descricao: 'Bacalhau desfiado com batata, servido com limão.', flags: ['SemLactose'], disponivel: true },
    { id: 2, nome: 'Pão de alho da casa', categoria: 'Entrada', preco: 18.9, descricao: 'Pão italiano com manteiga de alho e ervas.', flags: ['Vegetariano'], disponivel: true },
    { id: 3, nome: 'Salada caprese', categoria: 'Entrada', preco: 34.5, descricao: 'Tomate, muçarela de búfala, manjericão e azeite.', flags: ['Vegetariano', 'SemGluten'], disponivel: true },
    { id: 4, nome: 'Picanha na chapa', categoria: 'PratoPrincipal', preco: 119.0, descricao: 'Serve 2. Acompanha arroz, farofa e vinagrete.', flags: ['SemGluten', 'SemLactose'], disponivel: true },
    { id: 5, nome: 'Risoto de cogumelos', categoria: 'PratoPrincipal', preco: 68.0, descricao: 'Arbóreo, shiitake, shimeji e parmesão.', flags: ['Vegetariano', 'SemGluten'], disponivel: true },
    { id: 6, nome: 'Filé de tilápia grelhado', categoria: 'PratoPrincipal', preco: 59.9, descricao: 'Com legumes salteados e purê de mandioquinha.', flags: ['SemGluten'], disponivel: true },
    { id: 7, nome: 'Parmegiana de frango', categoria: 'PratoPrincipal', preco: 64.0, descricao: 'Serve 1. Arroz e batata frita.', flags: [], disponivel: false },
    { id: 8, nome: 'Moqueca de palmito', categoria: 'PratoPrincipal', preco: 62.0, descricao: 'Palmito, leite de coco e dendê. Acompanha arroz.', flags: ['Vegano', 'Vegetariano', 'SemGluten', 'SemLactose'], disponivel: true },
    { id: 9, nome: 'Kids: mini hambúrguer com fritas', categoria: 'PratoPrincipal', preco: 36.0, descricao: 'Pão, carne 90 g, queijo e batata frita.', flags: ['OpcaoInfantil'], disponivel: true },
    { id: 10, nome: 'Kids: frango com arroz e purê', categoria: 'PratoPrincipal', preco: 32.0, descricao: 'Porção infantil.', flags: ['OpcaoInfantil', 'SemGluten'], disponivel: true },
    { id: 11, nome: 'Pudim de leite', categoria: 'Sobremesa', preco: 19.0, descricao: 'Receita da casa.', flags: ['Vegetariano', 'SemGluten'], disponivel: true },
    { id: 12, nome: 'Petit gâteau', categoria: 'Sobremesa', preco: 29.0, descricao: 'Com sorvete de creme.', flags: ['Vegetariano'], disponivel: true },
    { id: 13, nome: 'Salada de frutas', categoria: 'Sobremesa', preco: 16.0, descricao: 'Frutas da estação.', flags: ['Vegano', 'Vegetariano', 'SemGluten', 'SemLactose'], disponivel: true },
    { id: 14, nome: 'Água sem gás 500 ml', categoria: 'Bebida', preco: 6.5, descricao: '', flags: ['Vegano', 'SemGluten', 'SemLactose'], disponivel: true },
    { id: 15, nome: 'Refrigerante lata', categoria: 'Bebida', preco: 8.0, descricao: '', flags: ['Vegano', 'SemGluten', 'SemLactose'], disponivel: true },
    { id: 16, nome: 'Suco de laranja 400 ml', categoria: 'Bebida', preco: 13.0, descricao: 'Natural, sem açúcar.', flags: ['Vegano', 'SemGluten', 'SemLactose'], disponivel: true },
    { id: 17, nome: 'Caipirinha de limão', categoria: 'Bebida', preco: 24.0, descricao: 'Cachaça, limão e açúcar.', flags: ['Vegano', 'SemGluten', 'SemLactose'], disponivel: true },
  ];

  // UC08: a melhor promoção vigente define o preço promocional (mínimo R$ 0,01).
  const promocoes = [
    { id: 1, descricao: 'Quinta do risoto', tipo: 'Percentual', valor: 15, inicio: '2026-09-01', fim: '2026-09-30', itens: [5], ativa: true },
    { id: 2, descricao: 'Sobremesa da semana', tipo: 'ValorFixo', valor: 5, inicio: '2026-09-14', fim: '2026-09-20', itens: [12], ativa: true },
    { id: 3, descricao: 'Festival do bacalhau', tipo: 'Percentual', valor: 10, inicio: '2026-08-01', fim: '2026-08-31', itens: [1], ativa: true },
  ];

  const HOJE = '2026-09-17';

  function vigente(p) { return p.ativa && p.inicio <= HOJE && HOJE <= p.fim; }

  function precoComDesconto(item, p) {
    const valor = p.tipo === 'Percentual' ? item.preco * (1 - p.valor / 100) : item.preco - p.valor;
    return Math.max(0.01, Math.round(valor * 100) / 100);
  }

  function precoPromocional(item) {
    const precos = promocoes.filter((p) => vigente(p) && p.itens.includes(item.id)).map((p) => precoComDesconto(item, p));
    return precos.length ? Math.min(...precos) : null;
  }

  function precoAtual(item) {
    const promo = precoPromocional(item);
    return promo ?? item.preco;
  }

  const pracas = [
    { id: 1, codigo: 'A', garcons: 3 },
    { id: 2, codigo: 'B', garcons: 2 },
    { id: 3, codigo: 'C', garcons: 2 },
    { id: 4, codigo: 'D', garcons: 1 },
  ];

  const mesas = [];
  [[1, 1, 8], [2, 9, 14], [3, 15, 20], [4, 21, 23]].forEach(([praca, de, ate]) => {
    for (let n = de; n <= ate; n++) mesas.push({ id: n, numero: String(n).padStart(2, '0'), capacidade: n % 5 === 0 ? 6 : n % 3 === 0 ? 2 : 4, pracaId: praca });
  });

  const usuarios = [
    { id: 1, nome: 'Roberta Lima', email: 'roberta@restaurante.com.br', papel: 'Gerente', ativo: true, segundoFator: true },
    { id: 2, nome: 'Marcos Tavares', email: 'marcos@restaurante.com.br', papel: 'Coordenador', ativo: true, segundoFator: false },
    { id: 3, nome: 'Sérgio Nogueira', email: 'sergio@restaurante.com.br', papel: 'Metre', ativo: true, segundoFator: false },
    { id: 11, nome: 'Carla Mendes', email: 'carla@restaurante.com.br', papel: 'Garcom', ativo: true, segundoFator: false },
    { id: 12, nome: 'João Pereira', email: 'joao@restaurante.com.br', papel: 'Garcom', ativo: true, segundoFator: false },
    { id: 13, nome: 'Ana Souza', email: 'ana@restaurante.com.br', papel: 'Garcom', ativo: true, segundoFator: false },
    { id: 14, nome: 'Rafael Costa', email: 'rafael@restaurante.com.br', papel: 'Garcom', ativo: true, segundoFator: false },
    { id: 15, nome: 'Beatriz Alves', email: 'beatriz@restaurante.com.br', papel: 'Garcom', ativo: true, segundoFator: false },
    { id: 16, nome: 'Lucas Martins', email: 'lucas@restaurante.com.br', papel: 'Garcom', ativo: true, segundoFator: false },
    { id: 17, nome: 'Fernanda Rocha', email: 'fernanda@restaurante.com.br', papel: 'Garcom', ativo: true, segundoFator: false },
    { id: 18, nome: 'Diego Ramos', email: 'diego@restaurante.com.br', papel: 'Garcom', ativo: true, segundoFator: false },
    { id: 19, nome: 'Paulo Henrique', email: 'paulo@restaurante.com.br', papel: 'Garcom', ativo: false, segundoFator: false },
  ];

  const garcons = () => usuarios.filter((u) => u.papel === 'Garcom' && u.ativo);

  // Comandas abertas no turno (mesas ocupadas).
  let proximoItem = 100;
  function item(itemCardapioId, quantidade, status, minutosAtras, motivo) {
    const c = cardapio.find((i) => i.id === itemCardapioId);
    return { id: proximoItem++, itemCardapioId, nome: c.nome, quantidade, precoUnitario: precoAtual(c), status, motivo: motivo || null, minutosAtras };
  }

  const comandas = [
    { id: 501, mesaId: 10, garcomId: 11, pessoas: 2, composicao: 'Casal', ajustada: false, taxaRemovida: false, codigo: 'K7P2QX', abertaHa: 48,
      itens: [item(14, 2, 'Entregue', 45), item(5, 1, 'Entregue', 35), item(4, 1, 'Entregue', 35), item(12, 2, 'Pendente', 4)], restricoes: [] },
    { id: 502, mesaId: 12, garcomId: 11, pessoas: 4, composicao: 'Familia', ajustada: false, taxaRemovida: false, codigo: 'M3R8TD', abertaHa: 22,
      itens: [item(15, 3, 'Entregue', 20), item(9, 2, 'Pendente', 12), item(8, 1, 'Pendente', 12), item(14, 1, 'Cancelado', 18, 'ClienteDesistiu')],
      restricoes: [{ id: 1, categoria: 'SemLactose', observacao: '' }, { id: 2, categoria: 'Alergia', observacao: 'Amendoim (criança de 7 anos)' }] },
    { id: 503, mesaId: 13, garcomId: 17, pessoas: 5, composicao: 'GrupoGrande', ajustada: false, taxaRemovida: false, codigo: 'Z9W4LB', abertaHa: 65,
      itens: [item(1, 2, 'Entregue', 60), item(17, 5, 'Entregue', 55), item(4, 2, 'Entregue', 40)], restricoes: [] },
    { id: 504, mesaId: 3, garcomId: 14, pessoas: 3, composicao: 'GrupoPequeno', ajustada: true, taxaRemovida: false, codigo: 'H2N6VC', abertaHa: 15,
      itens: [item(16, 3, 'Pendente', 14)], restricoes: [{ id: 3, categoria: 'Vegano', observacao: '' }] },
    { id: 505, mesaId: 16, garcomId: 13, pessoas: 2, composicao: 'Casal', ajustada: false, taxaRemovida: false, codigo: 'Q5D1FJ', abertaHa: 90,
      itens: [item(3, 1, 'Entregue', 85), item(6, 2, 'Entregue', 70), item(11, 2, 'Entregue', 30)], restricoes: [] },
  ];

  function totais(comanda) {
    const subtotal = comanda.itens.filter((i) => i.status !== 'Cancelado').reduce((s, i) => s + i.precoUnitario * i.quantidade, 0);
    const taxa = comanda.taxaRemovida ? 0 : Math.round(subtotal * 10) / 100;
    return { subtotal, taxa, total: subtotal + taxa };
  }

  // UC15/UC21: alocação confirmada do turno atual (Carla na praça B).
  const alocacaoAtual = { data: HOJE, periodo: 'Jantar', confirmada: true, designacoes: { 11: 2, 12: 4, 13: 3, 14: 1, 15: 3, 16: 1, 17: 2, 18: 1 } };

  // Histórico que alimenta a RN03 e os relatórios (UC16/UC17), últimos 30 dias.
  const indicadoresGarcons = [
    { garcomId: 13, nome: 'Ana Souza', faturamento: 28940, comandas: 212, turnos: 22, mesasAtendidas: 205, tempoMedio: 58 },
    { garcomId: 12, nome: 'João Pereira', faturamento: 26310, comandas: 187, turnos: 20, mesasAtendidas: 181, tempoMedio: 62 },
    { garcomId: 11, nome: 'Carla Mendes', faturamento: 24880, comandas: 201, turnos: 21, mesasAtendidas: 196, tempoMedio: 54 },
    { garcomId: 15, nome: 'Beatriz Alves', faturamento: 21150, comandas: 168, turnos: 19, mesasAtendidas: 160, tempoMedio: 57 },
    { garcomId: 14, nome: 'Rafael Costa', faturamento: 22760, comandas: 150, turnos: 21, mesasAtendidas: 146, tempoMedio: 71 },
    { garcomId: 17, nome: 'Fernanda Rocha', faturamento: 15320, comandas: 131, turnos: 15, mesasAtendidas: 128, tempoMedio: 55 },
    { garcomId: 16, nome: 'Lucas Martins', faturamento: 17400, comandas: 139, turnos: 20, mesasAtendidas: 133, tempoMedio: 60 },
    { garcomId: 18, nome: 'Diego Ramos', faturamento: 9870, comandas: 88, turnos: 12, mesasAtendidas: 85, tempoMedio: 66 },
  ].map((g) => ({ ...g, faturamentoPorTurno: g.faturamento / g.turnos, ticketMedio: g.faturamento / g.comandas, mesasPorTurno: g.mesasAtendidas / g.turnos }));

  /** Índice de desempenho (UC17): 50% faturamento por turno + 50% mesas por turno, normalizados pelo maior. */
  function ranking() {
    const maxFat = Math.max(...indicadoresGarcons.map((g) => g.faturamentoPorTurno));
    const maxMesas = Math.max(...indicadoresGarcons.map((g) => g.mesasPorTurno));
    return indicadoresGarcons
      .map((g) => ({ ...g, indice: Math.round((50 * g.faturamentoPorTurno / maxFat + 50 * g.mesasPorTurno / maxMesas) * 10) / 10 }))
      .sort((a, b) => b.indice - a.indice)
      .map((g, i) => ({ ...g, posicao: i + 1 }));
  }

  const indicadoresPracas = [
    { pracaId: 1, codigo: 'A', faturamento: 71240, comandas: 540, turnosComMovimento: 52 },
    { pracaId: 2, codigo: 'B', faturamento: 48910, comandas: 395, turnosComMovimento: 50 },
    { pracaId: 3, codigo: 'C', faturamento: 29830, comandas: 214, turnosComMovimento: 38 },
    { pracaId: 4, codigo: 'D', faturamento: 16650, comandas: 127, turnosComMovimento: 24 },
  ].map((p) => ({ ...p, faturamentoPorTurno: p.faturamento / p.turnosComMovimento, ticketMedio: p.faturamento / p.comandas }));

  const indicadoresCardapio = [
    { nome: 'Picanha na chapa', categoria: 'PratoPrincipal', quantidade: 418, faturamento: 49742 },
    { nome: 'Caipirinha de limão', categoria: 'Bebida', quantidade: 1102, faturamento: 26448 },
    { nome: 'Risoto de cogumelos', categoria: 'PratoPrincipal', quantidade: 362, faturamento: 21968 },
    { nome: 'Bolinho de bacalhau (6 un.)', categoria: 'Entrada', quantidade: 455, faturamento: 19110 },
    { nome: 'Filé de tilápia grelhado', categoria: 'PratoPrincipal', quantidade: 247, faturamento: 14795 },
    { nome: 'Refrigerante lata', categoria: 'Bebida', quantidade: 1540, faturamento: 12320 },
    { nome: 'Petit gâteau', categoria: 'Sobremesa', quantidade: 388, faturamento: 10864 },
    { nome: 'Pudim de leite', categoria: 'Sobremesa', quantidade: 402, faturamento: 7638 },
  ];

  // Faturamento por hora (18h–23h) e praça, para o mapa de calor.
  const porHora = {
    A: [2100, 5400, 9800, 11200, 7300, 2600],
    B: [1500, 3900, 7100, 7900, 4700, 1500],
    C: [300, 1900, 4400, 5100, 3000, 900],
    D: [200, 1200, 2600, 3100, 1800, 500],
  };
  const HORAS = [18, 19, 20, 21, 22, 23];

  // UC18: sugestões por associação (o que costuma sair junto com o que já está na mesa).
  const associacoes = { 4: [17, 1, 11], 5: [16, 13, 3], 12: [14], 8: [13, 16], 9: [15, 13], 3: [5, 16], 14: [2] };

  function sugestoes(comanda) {
    const naMesa = new Set(comanda.itens.map((i) => i.itemCardapioId));
    const ids = [];
    comanda.itens.forEach((i) => (associacoes[i.itemCardapioId] || []).forEach((id) => { if (!naMesa.has(id) && !ids.includes(id)) ids.push(id); }));
    if (!ids.length) ids.push(1, 14, 17);
    const restricoes = comanda.restricoes.map((r) => r.categoria);
    const atende = (c) => restricoes.every((r) => !['Vegano', 'Vegetariano', 'SemGluten', 'SemLactose'].includes(r) || c.flags.includes(r));
    const itens = ids.map((id) => cardapio.find((c) => c.id === id)).filter((c) => c.disponivel && atende(c)).slice(0, 3);
    return { itens, confirmarComCliente: restricoes.some((r) => r === 'Alergia' || r === 'Outro') };
  }

  window.DADOS = {
    CATEGORIAS, FLAGS, RESTRICOES, MOTIVOS_CANCELAMENTO, COMPOSICOES, PAPEIS, HOJE, HORAS,
    cardapio, promocoes, pracas, mesas, usuarios, comandas, alocacaoAtual,
    indicadoresGarcons, indicadoresPracas, indicadoresCardapio, porHora,
    classificar, garcons, totais, ranking, sugestoes, vigente, precoPromocional, precoAtual, precoComDesconto,
    novoIdItem: () => proximoItem++,
  };
})();
