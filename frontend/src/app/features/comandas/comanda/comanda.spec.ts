import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Comanda as ComandaDaApi, SugestoesDaComanda } from '../../../core/api/modelos';
import { API, comanda, itemCardapio, itemDoPedido, mesa, sessaoDoGarcom } from '../../../core/api/testes';
import { ARMAZENAMENTO_DA_SESSAO } from '../../../core/configuracao';
import { provedoresDeLocalizacao } from '../../../core/localizacao';
import { AvisoService } from '../../../shared/aviso/aviso.service';
import { Comanda } from './comanda';

const SUGESTOES: SugestoesDaComanda = {
  servicoDisponivel: true,
  confirmarRestricaoComCliente: false,
  itens: [{ itemDoCardapioId: 2, nome: 'Caipirinha de limão', categoria: 'Bebida', preco: 24 }],
};

describe('Comanda (UC12, UC13, UC18, UC23)', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<Comanda>;
  let avisos: AvisoService;

  async function renderizar(): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [Comanda],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: sessaoDoGarcom() },
        provedoresDeLocalizacao(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    avisos = TestBed.inject(AvisoService);
    fixture = TestBed.createComponent(Comanda);
    fixture.componentRef.setInput('id', '501');
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  async function responder(dados: Partial<ComandaDaApi> = {}, sugestoes: SugestoesDaComanda | 'falha' = SUGESTOES): Promise<void> {
    http.expectOne(`${API}/api/comandas/501`).flush(comanda({ itens: [itemDoPedido()], ...dados }));
    http.expectOne(`${API}/api/cardapio`).flush([
      itemCardapio({ id: 1, nome: 'Risoto de cogumelos', preco: 68, precoPromocional: 57.8 }),
      itemCardapio({ id: 2, nome: 'Caipirinha de limão', categoria: 'Bebida', preco: 24 }),
      itemCardapio({ id: 3, nome: 'Parmegiana', categoria: 'PratoPrincipal', disponivel: false }),
    ]);
    http.expectOne(`${API}/api/mesas`).flush([mesa(10, '10')]);
    const pedido = http.expectOne(`${API}/api/comandas/501/sugestoes`);
    if (sugestoes === 'falha') {
      pedido.flush({ erros: ['Serviço indisponível.'] }, { status: 503, statusText: 'Service Unavailable' });
    } else {
      pedido.flush(sugestoes);
    }
    await fixture.whenStable();
  }

  /** Depois de cada ação a tela recarrega a comanda e as sugestões: é a API que manda no estado. */
  async function responderRecarga(dados: Partial<ComandaDaApi> = {}): Promise<void> {
    http.expectOne(`${API}/api/comandas/501`).flush(comanda({ itens: [itemDoPedido()], ...dados }));
    http.expectOne(`${API}/api/comandas/501/sugestoes`).flush(SUGESTOES);
    await fixture.whenStable();
  }

  afterEach(() => http.verify());

  /** O formato de moeda usa espaço fixo entre "R$" e o valor; aqui o texto é normalizado para comparar. */
  function texto(pagina: HTMLElement): string {
    return (pagina.textContent ?? '').replace(/ /g, ' ');
  }

  it('mostra a mesa, as pendências e o preço promocional do cardápio', async () => {
    const pagina = await renderizar();
    await responder();

    expect(pagina.querySelector('h1')?.textContent).toContain('Mesa 10');
    expect(pagina.querySelector('#t-pendentes')?.parentElement?.textContent).toContain('1');

    [...pagina.querySelectorAll<HTMLButtonElement>('[role="tab"]')].find((b) => b.textContent?.includes('Pratos'))!.click();
    await fixture.whenStable();

    expect(texto(pagina)).toContain('R$ 57,80');
    expect(texto(pagina)).toContain('Indisponível');
  });

  it('lança o item e oferece desfazer, que cancela por erro de lançamento', async () => {
    const pagina = await renderizar();
    await responder({ itens: [] });

    const bebidas = [...pagina.querySelectorAll<HTMLButtonElement>('[role="tab"]')].find((b) => b.textContent?.includes('Bebidas'))!;
    bebidas.click();
    await fixture.whenStable();

    [...pagina.querySelectorAll<HTMLButtonElement>('.cardapio button')][0].click();
    await fixture.whenStable();
    pagina.querySelector<HTMLButtonElement>('app-folha .rodape-folha .botao')!.click();

    const lancamento = http.expectOne(`${API}/api/comandas/501/itens`);
    expect(lancamento.request.body).toEqual({ itemCardapioId: 2, quantidade: 1 });
    lancamento.flush(itemDoPedido({ id: 91, itemDoCardapioId: 2, nome: 'Caipirinha de limão' }));
    await responderRecarga({ itens: [itemDoPedido({ id: 91, nome: 'Caipirinha de limão' })] });

    expect(avisos.aviso()?.mensagem).toBe('1 × Caipirinha de limão lançado');

    avisos.desfazer();
    const cancelamento = http.expectOne(`${API}/api/comandas/501/itens/91/situacao`);
    expect(cancelamento.request.body).toEqual({ situacao: 'Cancelado', motivoCancelamento: 'ErroDeLancamento' });
    cancelamento.flush(itemDoPedido({ id: 91, status: 'Cancelado', motivoCancelamento: 'ErroDeLancamento' }));
    await responderRecarga({ itens: [] });
  });

  it('entregar não pede confirmação, e o aviso não oferece desfazer (a API não volta atrás)', async () => {
    const pagina = await renderizar();
    await responder();

    [...pagina.querySelectorAll<HTMLButtonElement>('.linha-item .botao')].find((b) => b.textContent?.includes('Entregue'))!.click();
    const requisicao = http.expectOne(`${API}/api/comandas/501/itens/90/situacao`);
    expect(requisicao.request.body).toEqual({ situacao: 'Entregue', motivoCancelamento: null });
    requisicao.flush(itemDoPedido({ status: 'Entregue' }));
    await responderRecarga({ itens: [itemDoPedido({ status: 'Entregue' })] });

    expect(avisos.aviso()?.mensagem).toContain('entregue');
    expect(avisos.aviso()?.acao).toBeUndefined();
  });

  it('cancelar só libera depois de escolher o motivo (RN02)', async () => {
    const pagina = await renderizar();
    await responder();

    [...pagina.querySelectorAll<HTMLButtonElement>('.linha-item .botao')].find((b) => b.textContent?.includes('Cancelar'))!.click();
    await fixture.whenStable();

    const confirmar = [...pagina.querySelectorAll<HTMLButtonElement>('app-folha .rodape-folha .botao')][1];
    expect(confirmar.disabled).toBe(true);

    [...pagina.querySelectorAll<HTMLButtonElement>('app-folha .chip')].find((c) => c.textContent?.includes('Item em falta'))!.click();
    await fixture.whenStable();
    expect(confirmar.disabled).toBe(false);

    confirmar.click();
    const requisicao = http.expectOne(`${API}/api/comandas/501/itens/90/situacao`);
    expect(requisicao.request.body).toEqual({ situacao: 'Cancelado', motivoCancelamento: 'ItemEmFalta' });
    requisicao.flush(itemDoPedido({ status: 'Cancelado', motivoCancelamento: 'ItemEmFalta' }));
    await responderRecarga({ itens: [itemDoPedido({ status: 'Cancelado', motivoCancelamento: 'ItemEmFalta' })] });
  });

  it('registra a restrição com a observação e avisa que ela é apagada no fechamento (RF14)', async () => {
    const pagina = await renderizar();
    await responder();

    pagina.querySelector<HTMLButtonElement>('[aria-label="Registrar restrição alimentar"]')!.click();
    await fixture.whenStable();

    [...pagina.querySelectorAll<HTMLButtonElement>('app-folha .chip')].find((c) => c.textContent?.includes('Alergia'))!.click();
    const campo = pagina.querySelector<HTMLTextAreaElement>('#observacao')!;
    campo.value = 'Camarão';
    campo.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    pagina.querySelector<HTMLButtonElement>('app-folha .rodape-folha .botao')!.click();
    const requisicao = http.expectOne(`${API}/api/comandas/501/restricoes`);
    expect(requisicao.request.body).toEqual({ categoria: 'Alergia', observacaoLivre: 'Camarão' });
    requisicao.flush(comanda());
    await responderRecarga({ restricoes: [{ id: 1, categoria: 'Alergia', observacaoLivre: 'Camarão' }] });

    expect(texto(pagina)).toContain('Alergia: Camarão');
    expect(texto(pagina)).toContain('Apagada no fechamento');
  });

  it('com o serviço de análise fora do ar, avisa e mantém o resto da comanda (D3)', async () => {
    const pagina = await renderizar();
    await responder({}, 'falha');

    expect(texto(pagina)).toContain('Sugestões indisponíveis agora');
    expect(pagina.querySelector('#t-lancar')).not.toBeNull();
  });
});
