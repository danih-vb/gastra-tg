import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Mesa, Praca } from '../../core/api/modelos';
import { API, sessaoDoGarcom } from '../../core/api/testes';
import { ARMAZENAMENTO_DA_SESSAO } from '../../core/configuracao';
import { QrDasMesas } from './qr-das-mesas';

const PRACAS: Praca[] = [
  { id: 1, codigo: 'A', quantidadeGarcons: 2 },
  { id: 2, codigo: 'B', quantidadeGarcons: 2 },
];
const MESAS: Mesa[] = [
  { id: 12, numero: '12', capacidade: 4, pracaId: 2, pracaCodigo: 'B' },
  { id: 3, numero: '03', capacidade: 2, pracaId: 1, pracaCodigo: 'A' },
  { id: 10, numero: '10', capacidade: 6, pracaId: 1, pracaCodigo: 'A' },
];

describe('QR codes das mesas (#219)', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<QrDasMesas>;

  async function renderizar(): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [QrDasMesas],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: sessaoDoGarcom() },
      ],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(QrDasMesas);
    await fixture.whenStable();
    http.expectOne(`${API}/api/pracas`).flush(PRACAS);
    http.expectOne(`${API}/api/mesas`).flush(MESAS);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  const etiquetas = (pagina: HTMLElement) => [...pagina.querySelectorAll<HTMLElement>('.etiqueta-qr')];

  afterEach(() => http.verify());

  it('uma etiqueta por mesa, em ordem de praça e número, com o QR do cardápio daquela mesa', async () => {
    const pagina = await renderizar();

    expect(etiquetas(pagina).map((e) => e.querySelector('.numero')?.textContent?.trim())).toEqual([
      'Mesa 03',
      'Mesa 10',
      'Mesa 12',
    ]);
    expect(etiquetas(pagina)[0].dataset['link']).toBe(`${window.location.origin}/cliente/cardapio?mesa=03`);
    await new Promise((r) => setTimeout(r, 50));
    fixture.detectChanges();
    expect(etiquetas(pagina)[0].querySelector('.qr svg')).not.toBeNull();
  });

  it('filtra pela praça e usa o endereço digitado no link', async () => {
    const pagina = await renderizar();

    [...pagina.querySelectorAll<HTMLButtonElement>('.chip')].find((b) => b.textContent?.includes('Praça B'))!.click();
    const campo = pagina.querySelector<HTMLInputElement>('#qr-endereco')!;
    campo.value = 'http://192.168.1.3:4200/';
    campo.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    expect(etiquetas(pagina).map((e) => e.dataset['link'])).toEqual(['http://192.168.1.3:4200/cliente/cardapio?mesa=12']);
  });
});
