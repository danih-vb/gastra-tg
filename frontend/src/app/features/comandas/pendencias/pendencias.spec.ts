import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { API, comanda, itemDoPedido, mesa, sessaoDoGarcom } from '../../../core/api/testes';
import { ARMAZENAMENTO_DA_SESSAO } from '../../../core/configuracao';
import { provedoresDeLocalizacao } from '../../../core/localizacao';
import { MINUTOS_PARA_ATRASO } from '../../../shared/rotulos';
import { Pendencias } from './pendencias';

const MINUTO = 60_000;

describe('A entregar (RF03)', () => {
  let http: HttpTestingController;

  afterEach(() => http.verify());

  it('mostra a mesa que chamou e limpa ao atender', async () => {
    await TestBed.configureTestingModule({
      imports: [Pendencias],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: sessaoDoGarcom() },
        provedoresDeLocalizacao(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(Pendencias);
    await fixture.whenStable();
    http.expectOne(`${API}/api/mesas`).flush([mesa(10, '10')]);
    http.expectOne(`${API}/api/comandas`).flush([comanda({ id: 501, atendimentoChamadoEm: new Date().toISOString() })]);
    await fixture.whenStable();

    const pagina = fixture.nativeElement as HTMLElement;
    expect(pagina.querySelector('.chamado')!.textContent).toContain('Mesa 10 chamando');

    pagina.querySelector<HTMLButtonElement>('.chamado button')!.click();
    http.expectOne({ method: 'DELETE', url: `${API}/api/comandas/501/chamado` }).flush(null, { status: 204, statusText: 'No Content' });
    http.expectOne(`${API}/api/mesas`).flush([mesa(10, '10')]);
    http.expectOne(`${API}/api/comandas`).flush([comanda({ id: 501 })]);
    await fixture.whenStable();

    expect(pagina.querySelector('.chamado')).toBeNull();
  });

  it('marca como atrasado só o item que passou do limite, e ele vem primeiro', async () => {
    await TestBed.configureTestingModule({
      imports: [Pendencias],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: sessaoDoGarcom() },
        provedoresDeLocalizacao(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(Pendencias);
    await fixture.whenStable();

    const haMinutos = (minutos: number) => new Date(Date.now() - minutos * MINUTO).toISOString();
    http.expectOne(`${API}/api/mesas`).flush([mesa(10, '10')]);
    http.expectOne(`${API}/api/comandas`).flush([
      comanda({
        itens: [
          itemDoPedido({ id: 1, nome: 'Café coado', dataHoraRegistro: haMinutos(MINUTOS_PARA_ATRASO - 1) }),
          itemDoPedido({ id: 2, nome: 'Moqueca', dataHoraRegistro: haMinutos(MINUTOS_PARA_ATRASO) }),
        ],
      }),
    ]);
    await fixture.whenStable();

    const linhas = [...(fixture.nativeElement as HTMLElement).querySelectorAll('.lista li')];
    expect(linhas[0].textContent).toContain('Moqueca');
    expect(linhas[0].textContent).toContain('Atrasado');
    expect(linhas[1].textContent).toContain('Café coado');
    expect(linhas[1].textContent).not.toContain('Atrasado');
  });
});
