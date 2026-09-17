import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Comanda } from '../../../core/api/modelos';
import { API, comanda, itemDoPedido, mesa, sessaoDoGarcom } from '../../../core/api/testes';
import { ARMAZENAMENTO_DA_SESSAO } from '../../../core/configuracao';
import { provedoresDeLocalizacao } from '../../../core/localizacao';
import { FecharConta } from './fechar-conta';

describe('FecharConta (UC14)', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<FecharConta>;

  async function renderizar(dados: Partial<Comanda>): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [FecharConta],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: sessaoDoGarcom() },
        provedoresDeLocalizacao(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(FecharConta);
    fixture.componentRef.setInput('id', '501');
    await fixture.whenStable();

    http.expectOne(`${API}/api/comandas/501`).flush(comanda(dados));
    http.expectOne(`${API}/api/mesas`).flush([mesa(10, '10')]);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function texto(pagina: HTMLElement): string {
    return (pagina.textContent ?? '').replace(/ /g, ' ');
  }

  afterEach(() => http.verify());

  it('com item pendente, explica o bloqueio e não deixa fechar (RN02)', async () => {
    const pagina = await renderizar({ itens: [itemDoPedido({ status: 'Pendente', nome: 'Pudim de leite' })] });

    expect(texto(pagina)).toContain('Ainda não dá para fechar');
    expect(texto(pagina)).toContain('Pudim de leite');
    expect(pagina.querySelector<HTMLButtonElement>('.rodape-fixo .botao')!.disabled).toBe(true);
  });

  it('mostra subtotal, taxa de 10% e total, e cobra a taxa por padrão (RF04)', async () => {
    const pagina = await renderizar({ itens: [itemDoPedido({ status: 'Entregue', valor: 100, quantidade: 1 })] });

    expect(texto(pagina)).toContain('R$ 100,00');
    expect(texto(pagina)).toContain('R$ 10,00');
    expect(texto(pagina)).toContain('R$ 110,00');
    expect(pagina.querySelector<HTMLInputElement>('.interruptor input')!.checked).toBe(true);
  });

  it('tira a taxa a pedido do cliente e não deixa recolocar', async () => {
    const pagina = await renderizar({ itens: [itemDoPedido({ status: 'Entregue', valor: 100 })] });

    pagina.querySelector<HTMLInputElement>('.interruptor input')!.click();
    http.expectOne({ method: 'DELETE', url: `${API}/api/comandas/501/taxa-servico` }).flush(
      comanda({ itens: [itemDoPedido({ status: 'Entregue', valor: 100 })], taxaServicoRemovida: true }),
    );
    await fixture.whenStable();

    const interruptor = pagina.querySelector<HTMLInputElement>('.interruptor input')!;
    expect(interruptor.checked).toBe(false);
    expect(interruptor.disabled).toBe(true);
    expect(texto(pagina)).toContain('R$ 100,00');
  });

  it('fecha depois de confirmar e avisa que as restrições foram apagadas', async () => {
    const pagina = await renderizar({ itens: [itemDoPedido({ status: 'Entregue', valor: 100 })] });

    pagina.querySelector<HTMLButtonElement>('.rodape-fixo .botao')!.click();
    await fixture.whenStable();
    expect(texto(pagina)).toContain('Depois de fechada, a conta não aceita novos itens');

    [...pagina.querySelectorAll<HTMLButtonElement>('app-folha .rodape-folha .botao')][1].click();
    http.expectOne({ method: 'POST', url: `${API}/api/comandas/501/fechamento` }).flush(
      comanda({ status: 'Fechada', itens: [itemDoPedido({ status: 'Entregue', valor: 100 })], dataHoraFechamento: new Date().toISOString() }),
    );
    await fixture.whenStable();

    expect(texto(pagina)).toContain('Mesa 10 liberada');
    expect(texto(pagina)).toContain('restrições alimentares e observações desta mesa foram apagadas');
  });

  it('erro da API no fechamento aparece na tela', async () => {
    const pagina = await renderizar({ itens: [itemDoPedido({ status: 'Entregue', valor: 100 })] });

    pagina.querySelector<HTMLButtonElement>('.rodape-fixo .botao')!.click();
    await fixture.whenStable();
    [...pagina.querySelectorAll<HTMLButtonElement>('app-folha .rodape-folha .botao')][1].click();
    http.expectOne({ method: 'POST', url: `${API}/api/comandas/501/fechamento` }).flush(
      { erros: ['Há itens pendentes: entregue ou cancele cada um antes de fechar.'] },
      { status: 422, statusText: 'Unprocessable Entity' },
    );
    await fixture.whenStable();

    expect(pagina.querySelector('[role="alert"]')?.textContent).toContain('Há itens pendentes');
  });
});
