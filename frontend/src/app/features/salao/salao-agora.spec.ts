import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { API, comanda, itemDoPedido, mesa, praca } from '../../core/api/testes';
import { provedoresDeLocalizacao } from '../../core/localizacao';
import { SalaoAgora } from './salao-agora';

describe('Salão agora (painel do Metre)', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<SalaoAgora>;

  async function renderizar(): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [SalaoAgora],
      providers: [provideHttpClient(), provideHttpClientTesting(), provedoresDeLocalizacao()],
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

  it('abre a comanda só para leitura, com as restrições e o aviso de privacidade (RN04)', async () => {
    const pagina = await renderizar();

    [...pagina.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.trim() === 'Ver')!.click();
    await fixture.whenStable();

    const folha = pagina.querySelector('app-folha')!;
    expect(folha.textContent).toContain('Mesa 10');
    expect(folha.textContent).toContain('Alergia: Amendoim');
    expect(folha.textContent).toContain('A entregar');
    expect(folha.textContent).toContain('só enquanto a mesa está aberta');
    // Painel de leitura: nada de entregar, cancelar ou fechar por aqui.
    expect([...folha.querySelectorAll('button')].some((b) => /Entregue|Cancelar|Fechar/.test(b.textContent ?? ''))).toBe(false);
  });
});
