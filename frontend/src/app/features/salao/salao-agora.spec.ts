import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { API, comanda, itemCardapio, itemDoPedido, mesa, praca } from '../../core/api/testes';
import { ARMAZENAMENTO_DA_SESSAO } from '../../core/configuracao';
import { Papel } from '../../core/sessao/modelos';
import { ArmazenamentoEmMemoria } from '../../core/sessao/testes';
import { provedoresDeLocalizacao } from '../../core/localizacao';
import { SalaoAgora } from './salao-agora';

describe('Salão agora (painel do Metre)', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<SalaoAgora>;

  async function renderizar(papel: Papel = 'Metre'): Promise<HTMLElement> {
    const armazenamento = new ArmazenamentoEmMemoria();
    armazenamento.setItem(
      'gastra.sessao',
      JSON.stringify({ token: 't', nome: 'Helena', papel, expiraEm: Date.now() + 60_000 }),
    );
    await TestBed.configureTestingModule({
      imports: [SalaoAgora],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provedoresDeLocalizacao(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: armazenamento },
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(SalaoAgora);
    await fixture.whenStable();

    http.expectOne(`${API}/api/pracas`).flush([praca(1, 'A'), praca(2, 'B')]);
    http.expectOne(`${API}/api/mesas`).flush([mesa(10, '10', 1), mesa(20, '20', 2)]);
    http.expectOne(`${API}/api/comandas`).flush([
      comanda({
        id: 501,
        mesaId: 10,
        garcomNome: 'Carla Mendes',
        itens: [itemDoPedido({ status: 'Pendente', valor: 40 })],
        restricoes: [{ id: 1, categoria: 'Alergia', observacaoLivre: 'Amendoim' }],
      }),
      comanda({ id: 502, mesaId: 20, garcomNome: 'João Pereira', itens: [itemDoPedido({ status: 'Entregue', valor: 60 })] }),
    ]);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function texto(pagina: HTMLElement): string {
    return (pagina.textContent ?? '').replace(/ /g, ' ');
  }

  afterEach(() => http.verify());

  function botao(pagina: HTMLElement, rotulo: string): HTMLButtonElement | undefined {
    return [...pagina.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.includes(rotulo));
  }

  function preencher(pagina: HTMLElement, seletor: string, valor: string): void {
    const campo = pagina.querySelector<HTMLInputElement>(seletor)!;
    campo.value = valor;
    campo.dispatchEvent(new Event('input'));
  }

  it('o metre lança uma cortesia com motivo e a mesa passa a mostrá-la', async () => {
    const pagina = await renderizar('Metre');
    botao(pagina, 'Ver')!.click();
    await fixture.whenStable();

    expect(botao(pagina, 'Alterar preço')).toBeUndefined();

    botao(pagina, 'Lançar cortesia')!.click();
    http
      .expectOne(`${API}/api/cardapio`)
      .flush([itemCardapio({ id: 7, nome: 'Tortinha de aniversário', preco: 18, disponivel: false })]);
    await fixture.whenStable();

    const lista = pagina.querySelector<HTMLSelectElement>('#cortesia-item')!;
    lista.selectedIndex = 1;
    lista.dispatchEvent(new Event('change'));
    preencher(pagina, '#cortesia-motivo', 'Aniversário');
    await fixture.whenStable();
    botao(pagina, 'Lançar cortesia')!.click();

    const pedido = http.expectOne(`${API}/api/comandas/501/cortesias`);
    expect(pedido.request.body).toEqual({ itemCardapioId: 7, quantidade: 1, motivo: 'Aniversário' });
    pedido.flush(itemDoPedido({ cortesia: true, valor: 0 }));
    http.expectOne(`${API}/api/comandas/501`).flush(
      comanda({
        id: 501,
        mesaId: 10,
        itens: [itemDoPedido({ nome: 'Tortinha de aniversário', cortesia: true, valor: 0, motivoDoAjuste: 'Aniversário' })],
      }),
    );
    await fixture.whenStable();

    expect(texto(pagina.querySelector('app-folha')!)).toContain('Cortesia · Aniversário');
  });

  it('o metre lança um item comum, que vai para o garçom confirmar', async () => {
    const pagina = await renderizar('Metre');
    botao(pagina, 'Ver')!.click();
    await fixture.whenStable();

    botao(pagina, 'Lançar item')!.click();
    http
      .expectOne(`${API}/api/cardapio`)
      .flush([
        itemCardapio({ id: 7, nome: 'Tortinha de aniversário', disponivel: false }),
        itemCardapio({ id: 8, nome: 'Moqueca de peixe', preco: 89 }),
      ]);
    await fixture.whenStable();

    const lista = pagina.querySelector<HTMLSelectElement>('#lancar-item')!;
    expect([...lista.options].map((o) => o.textContent)).not.toContain('Tortinha de aniversário · R$ 0,00');
    lista.selectedIndex = 1;
    lista.dispatchEvent(new Event('change'));
    await fixture.whenStable();
    botao(pagina, 'Lançar para o garçom')!.click();

    const pedido = http.expectOne(`${API}/api/comandas/501/itens`);
    expect(pedido.request.body).toEqual({ itemCardapioId: 8, quantidade: 1 });
    pedido.flush(itemDoPedido({ status: 'AConfirmar', lancadoPor: 'Metre' }));
    http.expectOne(`${API}/api/comandas/501`).flush(
      comanda({ id: 501, mesaId: 10, itens: [itemDoPedido({ nome: 'Moqueca de peixe', status: 'AConfirmar', lancadoPor: 'Metre' })] }),
    );
    await fixture.whenStable();

    expect(texto(pagina.querySelector('app-folha')!)).toContain('A confirmar');
  });

  it('o gerente altera o preço de um item da mesa, com motivo', async () => {
    const pagina = await renderizar('Gerente');
    botao(pagina, 'Ver')!.click();
    await fixture.whenStable();

    botao(pagina, 'Alterar preço')!.click();
    await fixture.whenStable();
    preencher(pagina, '#ajuste-preco', '30');
    preencher(pagina, '#ajuste-motivo', 'Prato demorou');
    await fixture.whenStable();
    botao(pagina, 'Salvar preço')!.click();

    const pedido = http.expectOne(`${API}/api/comandas/501/itens/90/preco`);
    expect(pedido.request.body).toEqual({ preco: 30, motivo: 'Prato demorou' });
    pedido.flush(itemDoPedido({ precoUnitarioNoMomento: 30, precoOriginal: 68 }));
    http.expectOne(`${API}/api/comandas/501`).flush(
      comanda({
        id: 501,
        mesaId: 10,
        itens: [itemDoPedido({ precoUnitarioNoMomento: 30, valor: 30, precoOriginal: 68, motivoDoAjuste: 'Prato demorou' })],
      }),
    );
    await fixture.whenStable();

    expect(texto(pagina.querySelector('app-folha')!)).toContain('R$ 68,00 R$ 30,00 · Prato demorou');
  });

  it('mostra quantos itens da mesa estão atrasados', async () => {
    await TestBed.configureTestingModule({
      imports: [SalaoAgora],
      providers: [provideHttpClient(), provideHttpClientTesting(), provedoresDeLocalizacao()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(SalaoAgora);
    await fixture.whenStable();

    http.expectOne(`${API}/api/pracas`).flush([praca(1, 'A')]);
    http.expectOne(`${API}/api/mesas`).flush([mesa(10, '10', 1)]);
    http.expectOne(`${API}/api/comandas`).flush([
      comanda({
        id: 501,
        mesaId: 10,
        itens: [
          itemDoPedido({ id: 1, dataHoraRegistro: new Date(Date.now() - 20 * 60_000).toISOString() }),
          itemDoPedido({ id: 2 }),
        ],
      }),
    ]);
    await fixture.whenStable();

    const pagina = fixture.nativeElement as HTMLElement;
    expect(pagina.textContent).toContain('1 atrasado');
    expect(pagina.textContent).toContain('2 a entregar');
  });

  it('agrupa as mesas por praça, com garçom, pendências e restrição', async () => {
    const pagina = await renderizar();

    const pracaA = [...pagina.querySelectorAll('section.cartao')].find((s) => s.textContent?.includes('Praça A'))!;
    expect(pracaA.textContent).toContain('Mesa 10');
    expect(pracaA.textContent).toContain('Carla Mendes');
    expect(pracaA.textContent).toContain('restrição');
    expect(pracaA.textContent).toContain('1 a entregar');

    const pracaB = [...pagina.querySelectorAll('section.cartao')].find((s) => s.textContent?.includes('Praça B'))!;
    expect(pracaB.textContent).toContain('João Pereira');
  });

  it('soma o que está em consumo e o que falta entregar', async () => {
    const pagina = await renderizar();

    expect(texto(pagina)).toContain('2');
    expect(texto(pagina)).toContain('R$ 100');
  });

  it('abre a comanda só para leitura, com as restrições (RN04)', async () => {
    const pagina = await renderizar();

    [...pagina.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.trim() === 'Ver')!.click();
    await fixture.whenStable();

    const folha = pagina.querySelector('app-folha')!;
    expect(folha.textContent).toContain('Mesa 10');
    expect(folha.textContent).toContain('Alergia: Amendoim');
    expect(folha.textContent).toContain('A entregar');
    // Painel de leitura: nada de entregar, cancelar ou fechar por aqui.
    expect([...folha.querySelectorAll('button')].some((b) => /Entregue|Cancelar|Fechar/.test(b.textContent ?? ''))).toBe(false);
  });
});
