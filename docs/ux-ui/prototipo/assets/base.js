/*
 * GASTRA — utilidades do protótipo navegável.
 * Não é código de produção: serve para navegar pelos fluxos, contar toques (RNF02) e mostrar estados
 * alternativos (serviço analítico fora do ar, lista vazia) durante a avaliação de Nielsen.
 */
(function () {
  'use strict';

  // Ícones em traço, 24×24, desenhados para o protótipo.
  const CAMINHOS = {
    mesa: '<path d="M4 9h16M6 9v10M18 9v10M3 6h18"/>',
    pessoas: '<circle cx="9" cy="8" r="3"/><path d="M3 20c0-3.3 2.7-6 6-6s6 2.7 6 6"/><circle cx="17" cy="9" r="2.5"/><path d="M16 14.2c2.9.4 5 2.8 5 5.8"/>',
    voltar: '<path d="M15 5l-7 7 7 7"/>',
    avancar: '<path d="M9 5l7 7-7 7"/>',
    mais: '<path d="M12 5v14M5 12h14"/>',
    menos: '<path d="M5 12h14"/>',
    check: '<path d="M5 12.5l4.5 4.5L19 7"/>',
    x: '<path d="M6 6l12 12M18 6L6 18"/>',
    alerta: '<path d="M12 3l9.5 17h-19z"/><path d="M12 10v4M12 17.5v.01"/>',
    info: '<circle cx="12" cy="12" r="9"/><path d="M12 11v6M12 7.5v.01"/>',
    relogio: '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',
    grafico: '<path d="M4 20V10M10 20V4M16 20v-7M22 20H2"/>',
    trofeu: '<path d="M8 4h8v5a4 4 0 01-8 0z"/><path d="M8 6H5a3 3 0 003 4M16 6h3a3 3 0 01-3 4M12 13v4M8 20h8"/>',
    sair: '<path d="M14 4h5v16h-5M10 8l-4 4 4 4M6 12h10"/>',
    cardapio: '<path d="M6 3h12v18H6z"/><path d="M9 8h6M9 12h6M9 16h4"/>',
    usuarios: '<circle cx="9" cy="8" r="3.5"/><path d="M2.5 20c.5-3.5 3.2-6 6.5-6s6 2.5 6.5 6"/><path d="M16 4.5a3.5 3.5 0 010 7M18 14.5c2 .8 3.3 2.9 3.5 5.5"/>',
    salao: '<rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/>',
    alocacao: '<circle cx="6" cy="6" r="2.5"/><circle cx="18" cy="6" r="2.5"/><path d="M6 8.5V12h12V8.5M12 12v4"/><rect x="8" y="16" width="8" height="5" rx="1"/>',
    comanda: '<path d="M6 2h12v20l-3-2-3 2-3-2-3 2z"/><path d="M9 7h6M9 11h6M9 15h3"/>',
    folha: '<path d="M5 19c0-8 5-14 15-14 0 10-6 15-14 15"/><path d="M5 19l8-8"/>',
    lapis: '<path d="M4 20h4L19 9l-4-4L4 16z"/><path d="M13 7l4 4"/>',
    lixeira: '<path d="M4 7h16M9 7V4h6v3M6 7l1 13h10l1-13"/>',
    qr: '<rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/><path d="M14 14h3v3h-3zM20 14v.01M14 20h.01M17 20h4v-3"/>',
    estrela: '<path d="M12 3l2.7 5.6 6.1.9-4.4 4.3 1 6.1L12 17l-5.4 2.9 1-6.1-4.4-4.3 6.1-.9z"/>',
    faisca: '<path d="M12 3v4M12 17v4M3 12h4M17 12h4M6 6l2.5 2.5M15.5 15.5L18 18M6 18l2.5-2.5M15.5 8.5L18 6"/>',
    cadeado: '<rect x="5" y="11" width="14" height="10" rx="2"/><path d="M8 11V7a4 4 0 018 0v4"/>',
    celular: '<rect x="7" y="2" width="10" height="20" rx="2"/><path d="M11 18h2"/>',
    atualizar: '<path d="M20 12a8 8 0 11-2.3-5.7M20 4v5h-5"/>',
    etiqueta: '<path d="M3 12V3h9l9 9-9 9z"/><circle cx="7.5" cy="7.5" r="1.5"/>',
    filtro: '<path d="M3 5h18l-7 8v6l-4 2v-8z"/>',
    calendario: '<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M3 10h18M8 3v4M16 3v4"/>',
    olho: '<path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12z"/><circle cx="12" cy="12" r="3"/>',
    escudo: '<path d="M12 3l8 3v6c0 5-3.5 8-8 9-4.5-1-8-4-8-9V6z"/>',
    prato: '<path d="M3 18h18M5 18a7 7 0 0114 0M12 8.5V11M10 8.5h4"/>',
    copo: '<path d="M6 3h12l-1.6 17H7.6z"/><path d="M6.6 9h10.8"/>',
    mao: '<path d="M8 13V5a1.5 1.5 0 013 0v6M11 11V4a1.5 1.5 0 013 0v7M14 11V5.5a1.5 1.5 0 013 0V14c0 4-2.5 7-6.5 7S5 18.5 4 16l-1.5-3.5a1.5 1.5 0 012.6-1.4L8 14"/>',
  };

  function icone(nome, rotulo) {
    const acessivel = rotulo ? `role="img" aria-label="${rotulo}"` : 'aria-hidden="true"';
    return `<svg class="icone" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" ${acessivel}>${CAMINHOS[nome] || ''}</svg>`;
  }

  const formatoMoeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });
  const moeda = (valor) => formatoMoeda.format(valor);
  const numero = (valor, casas = 0) =>
    new Intl.NumberFormat('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas }).format(valor);
  const escapar = (texto) =>
    String(texto).replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[c]);

  // ---------- Rotas por hash (#/mesas, #/comanda/12) ----------
  const rotas = [];
  let aoMudarRota = null;

  function rota(padrao, render) {
    const nomes = [];
    const regex = new RegExp('^' + padrao.replace(/:(\w+)/g, (_, n) => { nomes.push(n); return '([^/]+)'; }) + '$');
    rotas.push({ padrao, regex, nomes, render });
  }

  function navegar(caminho) {
    if (location.hash === '#' + caminho) resolver();
    else location.hash = caminho;
  }

  function resolver() {
    const [caminho, consulta = ''] = (location.hash.replace(/^#/, '') || '/').split('?');
    for (const r of rotas) {
      const m = caminho.match(r.regex);
      if (m) {
        const params = Object.fromEntries(r.nomes.map((n, i) => [n, decodeURIComponent(m[i + 1])]));
        params.consulta = new URLSearchParams(consulta);
        fecharFolha();
        r.render(params);
        if (aoMudarRota) aoMudarRota(caminho);
        window.scrollTo(0, 0);
        return;
      }
    }
    if (rotas.length) navegar(rotas[0].padrao);
  }

  function iniciar(opcoes = {}) {
    aoMudarRota = opcoes.aoMudarRota || null;
    window.addEventListener('hashchange', resolver);
    if (new URLSearchParams(location.search).has('captura')) document.body.classList.add('captura');
    montarInstrumento(opcoes.instrumento || {});
    resolver();
  }

  // ---------- Folha inferior / diálogo ----------
  let folhaAtual = null;

  function abrirFolha(html, { dialogo = false, rotulo = 'Painel', aoMontar } = {}) {
    fecharFolha();
    const fundo = document.createElement('div');
    fundo.className = 'fundo-folha' + (dialogo ? ' dialogo' : '');
    fundo.innerHTML = `<section class="folha" role="dialog" aria-modal="true" aria-label="${escapar(rotulo)}">${html}</section>`;
    fundo.addEventListener('click', (e) => { if (e.target === fundo) fecharFolha(); });
    document.body.appendChild(fundo);
    folhaAtual = { fundo, anterior: document.activeElement };
    const folha = fundo.querySelector('.folha');
    if (aoMontar) aoMontar(folha);
    const foco = folha.querySelector('[autofocus], button, input, select, textarea');
    if (foco) foco.focus({ preventScroll: true });
    return folha;
  }

  function fecharFolha() {
    if (!folhaAtual) return;
    folhaAtual.fundo.remove();
    if (folhaAtual.anterior && folhaAtual.anterior.isConnected) folhaAtual.anterior.focus({ preventScroll: true });
    folhaAtual = null;
  }

  document.addEventListener('keydown', (e) => { if (e.key === 'Escape') fecharFolha(); });

  // ---------- Aviso flutuante (com "Desfazer" opcional) ----------
  let temporizador = null;
  function avisar(mensagem, { acao, aoAgir, icone: nomeIcone = 'check' } = {}) {
    document.querySelectorAll('.aviso-flutuante').forEach((el) => el.remove());
    clearTimeout(temporizador);
    const el = document.createElement('div');
    el.className = 'aviso-flutuante';
    el.setAttribute('role', 'status');
    el.innerHTML = `${icone(nomeIcone)}<span>${escapar(mensagem)}</span>${acao ? `<button type="button">${escapar(acao)}</button>` : ''}`;
    if (acao) el.querySelector('button').addEventListener('click', () => { el.remove(); aoAgir(); });
    document.body.appendChild(el);
    temporizador = setTimeout(() => el.remove(), acao ? 6000 : 3000);
  }

  // ---------- Instrumento: contador de toques e estados alternativos ----------
  const medicao = { toques: 0, etapas: null, etapaAtual: 0, resultados: [] };
  const estados = {};
  let instrumento = null;

  function montarInstrumento({ medicoes = null, alternativas = [] }) {
    alternativas.forEach((a) => { estados[a.chave] = !!a.inicial; });
    instrumento = document.createElement('details');
    instrumento.className = 'instrumento';
    instrumento.innerHTML = `
      <summary>${icone('mao')}<span>Toques</span><span class="contagem num" aria-live="polite">0</span></summary>
      <div class="corpo-instrumento">
        <p>Conta cada ação do usuário nas telas (toque em botão, chip, mesa, item).</p>
        <button type="button" data-instr="zerar">Zerar contagem</button>
        ${medicoes ? `<button type="button" data-instr="medir">Medir ${escapar(medicoes.nome)}</button><div class="resultado oculto" aria-live="polite"></div>` : ''}
        ${alternativas.map((a) => `<label><input type="checkbox" data-estado="${a.chave}" ${a.inicial ? 'checked' : ''}> ${escapar(a.rotulo)}</label>`).join('')}
        <a href="index.html">Voltar ao índice do protótipo</a>
      </div>`;
    document.body.appendChild(instrumento);
    instrumento.querySelector('[data-instr="zerar"]').addEventListener('click', () => { medicao.toques = 0; medicao.etapas = null; atualizarInstrumento(); });
    const medir = instrumento.querySelector('[data-instr="medir"]');
    if (medir) medir.addEventListener('click', () => {
      medicao.toques = 0;
      medicao.etapas = medicoes.etapas;
      medicao.etapaAtual = 0;
      medicao.resultados = [];
      medicao.limite = medicoes.limite;
      if (medicoes.inicio) navegar(medicoes.inicio);
      instrumento.open = false;
      atualizarInstrumento();
      avisar(`Medição iniciada: ${medicoes.etapas[0].nome}`, { icone: 'mao' });
    });
    instrumento.querySelectorAll('[data-estado]').forEach((caixa) => caixa.addEventListener('change', () => {
      estados[caixa.dataset.estado] = caixa.checked;
      resolver();
    }));
    document.addEventListener('click', (e) => {
      if (instrumento.contains(e.target)) return;
      if (e.target.closest('[data-toque]')) { medicao.toques++; atualizarInstrumento(); }
    }, true);
  }

  function atualizarInstrumento() {
    if (!instrumento) return;
    instrumento.querySelector('.contagem').textContent = medicao.toques;
    const resultado = instrumento.querySelector('.resultado');
    if (!resultado) return;
    if (!medicao.etapas) { resultado.classList.add('oculto'); return; }
    resultado.classList.remove('oculto');
    const linhas = medicao.resultados.map((r) => `${escapar(r.nome)}: <strong>${r.toques}</strong> (meta ${r.meta})`);
    if (medicao.etapaAtual < medicao.etapas.length) {
      linhas.push(`${escapar(medicao.etapas[medicao.etapaAtual].nome)}: medindo…`);
      resultado.classList.remove('ok');
    } else {
      const total = medicao.resultados.reduce((s, r) => s + r.toques, 0);
      const ok = total <= medicao.limite;
      linhas.push(`<strong>Total ${total} ${ok ? '≤' : '>'} ${medicao.limite} ${ok ? '✔ atende' : '✘ não atende'}</strong>`);
      resultado.classList.toggle('ok', ok);
    }
    resultado.innerHTML = linhas.join('<br>');
  }

  /** Marca o fim de uma etapa da medição (ex.: "mesa aberta"). */
  function marco(chave) {
    if (!medicao.etapas || medicao.etapaAtual >= medicao.etapas.length) return;
    const etapa = medicao.etapas[medicao.etapaAtual];
    if (etapa.chave !== chave) return;
    const anteriores = medicao.resultados.reduce((s, r) => s + r.toques, 0);
    medicao.resultados.push({ nome: etapa.nome, toques: medicao.toques - anteriores, meta: etapa.meta });
    medicao.etapaAtual++;
    atualizarInstrumento();
    if (medicao.etapaAtual >= medicao.etapas.length) instrumento.open = true;
  }

  window.G = { icone, moeda, numero, escapar, rota, navegar, iniciar, abrirFolha, fecharFolha, avisar, marco, estados };
})();
