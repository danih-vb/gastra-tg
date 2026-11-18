/*
 * GASTRA — cenas do protótipo para gerar as imagens da documentação (docs/ux-ui/telas).
 * Uso: <pagina>.html?captura&cena=<nome>. Cada cena é uma sequência de passos sobre o DOM, como um usuário faria.
 * O script docs/ux-ui/gerar_telas.py abre cada cena num navegador sem janela e salva a imagem.
 */
(function () {
  'use strict';

  const CENAS = {
    garcom: {
      mesas: [['rota', '/mesas']],
      'abrir-mesa': [['rota', '/mesas'], ['clicar', '[data-abrir-mesa="11"]']],
      comanda: [['rota', '/comanda/502']],
      'lancar-item': [['rota', '/comanda/502'], ['clicar', '[data-categoria="Bebida"]'], ['clicar', '[data-lancar="16"]'], ['clicar', '.folha [data-mais]']],
      restricao: [['rota', '/comanda/501'], ['clicar', '[data-acao="restricao"]'], ['clicar', '[data-cat="Alergia"]'], ['valor', '#obs', 'Camarão']],
      cancelar: [['rota', '/comanda/501'], ['clicar', '[data-cancelar]'], ['clicar', '[data-motivo="ClienteDesistiu"]']],
      'fechar-bloqueado': [['rota', '/comanda/502/fechar']],
      fechar: [['rota', '/comanda/505/fechar']],
      pendencias: [['rota', '/pendencias']],
      desempenho: [['rota', '/desempenho']],
      'sugestoes-indisponiveis': [['estado', 'servicoIndisponivel'], ['rota', '/comanda/501']],
    },
    metre: {
      presenca: [['rota', '/alocacao']],
      sugestao: [['rota', '/alocacao'], ['clicar', '[data-gerar]']],
      troca: [['rota', '/alocacao'], ['clicar', '[data-gerar]'], ['clicar', '[data-mudar="11"]'], ['clicar', '[data-destino="1"]'], ['clicar', '[data-com="18"]']],
      confirmada: [['rota', '/alocacao'], ['clicar', '[data-periodo="Almoco"]']],
      'servico-indisponivel': [['estado', 'servicoIndisponivel'], ['rota', '/alocacao'], ['clicar', '[data-gerar]']],
      salao: [['rota', '/salao']],
    },
    gerente: {
      analises: [['rota', '/analises']],
      cardapio: [['rota', '/cardapio']],
      preco: [['rota', '/cardapio'], ['clicar', '[data-preco="5"]']],
      promocoes: [['rota', '/cardapio'], ['clicar', '[data-aba="promocoes"]']],
      'nova-promocao': [['rota', '/cardapio'], ['clicar', '[data-aba="promocoes"]'], ['clicar', '[data-nova-promo]'], ['valor', '#p-desc', 'Petiscos da happy hour'],
        ['valor', '#p-valor', '20'], ['marcar', '.grade-checks input[value="2"]'], ['marcar', '.grade-checks input[value="1"]'], ['valor', '#p-fim', '2026-09-30']],
      salao: [['rota', '/salao']],
      usuarios: [['rota', '/usuarios']],
      'erro-formulario': [['rota', '/usuarios'], ['clicar', '[data-novo-usuario]'], ['valor', '#u-email', 'carla@restaurante.com.br'], ['valor', '#u-senha', '1234'], ['enviar', '.folha form']],
    },
    cliente: {
      inicio: [['rota', '/']],
      cardapio: [['rota', '/cardapio']],
      filtro: [['rota', '/cardapio'], ['clicar', '[data-flag="SemGluten"]'], ['clicar', '[data-flag="Vegetariano"]']],
      conta: [['rota', '/conta/M3R8TD']],
    },
    acesso: {
      login: [['rota', '/login']],
      'login-erro': [['rota', '/login'], ['valor', '#email', 'carla@restaurante.com.br'], ['valor', '#senha', 'errada'], ['enviar', 'form']],
      'configurar-2fa': [['rota', '/login'], ['clicar', '[data-perfil="marcos@restaurante.com.br"]'], ['enviar', 'form']],
      codigo: [['rota', '/login'], ['clicar', '[data-perfil="roberta@restaurante.com.br"]'], ['enviar', 'form']],
    },
  };

  const esperar = (ms) => new Promise((r) => setTimeout(r, ms));

  async function executar(passos) {
    for (const [acao, alvo, valor] of passos) {
      if (acao === 'rota') G.navegar(alvo);
      else if (acao === 'estado') {
        const caixa = document.querySelector(`[data-estado="${alvo}"]`);
        caixa.checked = true;
        caixa.dispatchEvent(new Event('change'));
      } else {
        const el = document.querySelector(alvo);
        if (!el) throw new Error(`Cena: elemento não encontrado: ${alvo}`);
        if (acao === 'clicar') el.click();
        else if (acao === 'valor') { el.value = valor; el.dispatchEvent(new Event('input', { bubbles: true })); }
        else if (acao === 'marcar') { el.checked = true; el.dispatchEvent(new Event('change', { bubbles: true })); }
        else if (acao === 'enviar') el.requestSubmit();
      }
      await esperar(120);
    }
    document.querySelectorAll('.aviso-flutuante').forEach((a) => a.remove());
    document.activeElement?.blur();
    document.body.dataset.cenaPronta = 'sim';
  }

  window.addEventListener('load', () => {
    const nome = new URLSearchParams(location.search).get('cena');
    if (!nome) return;
    const pagina = location.pathname.split('/').pop().replace('.html', '');
    const passos = CENAS[pagina]?.[nome];
    if (passos) setTimeout(() => executar(passos).catch((e) => { document.title = `ERRO ${e.message}`; }), 100);
  });

  window.CENAS_PROTOTIPO = CENAS;
})();
