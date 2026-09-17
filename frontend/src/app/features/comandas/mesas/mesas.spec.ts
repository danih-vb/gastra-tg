import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { API, comanda, itemDoPedido, mesa, praca, sessaoDoGarcom } from '../../../core/api/testes';
import { ARMAZENAMENTO_DA_SESSAO } from '../../../core/configuracao';
import { dataDeHoje, periodoDoTurno } from '../turno';
import { Mesas } from './mesas';

describe('Mesas (UC10)', () => {
  let http: HttpTestingController;
  let router: Router;
  let fixture: ComponentFixture<Mesas>;

  async function renderizar(): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [Mesas],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: sessaoDoGarcom() },
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(Mesas);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function responder(opcoes: { alocado?: boolean } = {}): void {
    http.expectOne(`${API}/api/pracas`).flush([praca(1, 'A'), praca(2, 'B')]);
    http.expectOne(`${API}/api/mesas`).flush([mesa(1, '01', 1), mesa(2, '02', 1), mesa(9, '09', 2)]);
    http.expectOne(`${API}/api/comandas`).flush([
      comanda({ id: 501, mesaId: 1, garcomId: 11, itens: [itemDoPedido({ status: 'Pendente' })] }),
      comanda({ id: 502, mesaId: 2, garcomId: 12, garcomNome: 'João Pereira' }),
    ]);
    const alocacao = http.expectOne(`${API}/api/alocacoes/${dataDeHoje()}/${periodoDoTurno()}`);
    if (opcoes.alocado === false) {
      alocacao.flush({ erros: ['Não há alocação para este turno.'] }, { status: 404, statusText: 'Not Found' });
    } else {
      alocacao.flush({
        data: dataDeHoje(),
        periodo: periodoDoTurno(),
        confirmada: true,
        servicoDisponivel: null,
        designacoes: [{ garcomId: 11, garcomNome: 'Carla Mendes', pracaId: 2, pracaCodigo: 'B' }],
      });
    }
  }

  afterEach(() => http.verify());

  function textoDasMesas(pagina: HTMLElement): string[] {
    return [...pagina.querySelectorAll('.mesa')].map((m) => (m.textContent ?? '').replace(/\s+/g, ' ').trim());
  }

  it('abre na praça do garçom e separa as mesas dele das dos colegas', async () => {
    const pagina = await renderizar();
    responder();
    await fixture.whenStable();

    expect(pagina.querySelector('.sub')?.textContent).toContain('sua praça: B');
    expect(pagina.querySelectorAll('.mesa').length).toBe(1); // só a praça B

    pagina.querySelectorAll<HTMLButtonElement>('.chip')[1].click(); // "Todas as praças"
    await fixture.whenStable();

    const mesas = textoDasMesas(pagina);
    expect(mesas.some((m) => m.includes('01') && m.includes('1 a entregar'))).toBe(true);
    expect(mesas.some((m) => m.includes('02') && m.includes('João'))).toBe(true);
    expect(mesas.some((m) => m.includes('09') && m.includes('Livre'))).toBe(true);
  });

  it('sem alocação confirmada, mostra todas as praças e avisa o motivo', async () => {
    const pagina = await renderizar();
    responder({ alocado: false });
    await fixture.whenStable();

    expect(pagina.textContent).toContain('alocação deste turno ainda não foi confirmada');
    expect(pagina.querySelectorAll('.mesa').length).toBe(3);
  });

  it('abre a mesa em dois toques: a mesa livre e a quantidade de pessoas (RNF02)', async () => {
    const pagina = await renderizar();
    responder({ alocado: false });
    await fixture.whenStable();

    const livre = [...pagina.querySelectorAll<HTMLButtonElement>('.mesa')].find((m) => m.textContent?.includes('09'))!;
    livre.click(); // 1º toque
    await fixture.whenStable();

    const opcoes = [...pagina.querySelectorAll<HTMLButtonElement>('.pessoas button')];
    expect(opcoes.map((b) => (b.textContent ?? '').replace(/\s+/g, ' ').trim())).toContain('4Grupo pequeno');

    opcoes[3].click(); // 2º toque: 4 pessoas
    const requisicao = http.expectOne(`${API}/api/comandas`);
    expect(requisicao.request.body).toEqual({ mesaId: 9, quantidadePessoas: 4 });
    requisicao.flush(comanda({ id: 777, mesaId: 9, quantidadePessoas: 4, composicao: 'GrupoPequeno' }));
    await fixture.whenStable();

    expect(router.navigate).toHaveBeenCalledWith(['/comandas', 777]);
  });

  it('mesa de colega não abre comanda: avisa de quem é', async () => {
    const pagina = await renderizar();
    responder({ alocado: false });
    await fixture.whenStable();

    [...pagina.querySelectorAll<HTMLButtonElement>('.mesa')].find((m) => m.textContent?.includes('02'))!.click();
    await fixture.whenStable();

    expect(pagina.querySelector('.pessoas')).toBeNull();
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
