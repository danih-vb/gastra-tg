import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RankingDeDesempenho } from '../../../core/api/modelos';
import { API, sessaoDoGarcom } from '../../../core/api/testes';
import { ARMAZENAMENTO_DA_SESSAO } from '../../../core/configuracao';
import { provedoresDeLocalizacao } from '../../../core/localizacao';
import { Desempenho } from './desempenho';

const RANKING: RankingDeDesempenho = {
  periodo: { inicio: '2026-08-18', fim: '2026-09-16' },
  pesoFaturamento: 0.5,
  pesoMesasAtendidas: 0.5,
  totalNoRanking: 8,
  posicoes: [
    { posicao: 3, garcomId: 11, nome: 'Carla Mendes', indice: 95, faturamentoPorTurno: 1184.76, mesasPorTurno: 9.3, turnos: 21 },
  ],
};

describe('Desempenho (UC17)', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<Desempenho>;

  async function renderizar(resposta: RankingDeDesempenho): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [Desempenho],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: sessaoDoGarcom() },
        provedoresDeLocalizacao(),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(Desempenho);
    await fixture.whenStable();
    http.expectOne(`${API}/api/indicadores/desempenho`).flush(resposta);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function texto(pagina: HTMLElement): string {
    return (pagina.textContent ?? '').replace(/ /g, ' ');
  }

  afterEach(() => http.verify());

  it('mostra a posição do próprio garçom e o que entra no índice', async () => {
    const pagina = await renderizar(RANKING);

    expect(texto(pagina)).toContain('3º');
    expect(texto(pagina)).toContain('de 8 garçons');
    expect(texto(pagina)).toContain('95,0 / 100');
    expect(texto(pagina)).toContain('R$ 1.184,76');
    expect(texto(pagina)).toContain('vale 50% do índice');
  });

  it('sem turnos no período, explica em vez de mostrar tela vazia', async () => {
    const pagina = await renderizar({ ...RANKING, totalNoRanking: 0, posicoes: [] });

    expect(texto(pagina)).toContain('Ainda sem turnos no período');
  });
});
