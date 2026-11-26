import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MedalhasDoGarcom, RankingDeDesempenho } from '../../../core/api/modelos';
import { API, sessaoDoGarcom } from '../../../core/api/testes';
import { ARMAZENAMENTO_DA_SESSAO } from '../../../core/configuracao';
import { provedoresDeLocalizacao } from '../../../core/localizacao';
import { Desempenho } from './desempenho';

const RANKING: RankingDeDesempenho = {
  periodo: { inicio: '2026-08-18', fim: '2026-09-16' },
  pesoFaturamento: 0.5,
  pesoMesasAtendidas: 0.5,
  pesoAvaliacao: 0,
  minimoDeAvaliacoes: 5,
  totalNoRanking: 8,
  posicoes: [
    { posicao: 3, garcomId: 11, nome: 'Carla Mendes', indice: 95, faturamentoPorTurno: 1184.76, mesasPorTurno: 9.3, turnos: 21, notaConsiderada: null },
  ],
};

const MEDALHAS: MedalhasDoGarcom = {
  garcomId: 11,
  nome: 'Carla Mendes',
  mes: '2026-11-01',
  medalhas: [
    { codigo: 'top-3', conquistada: true, progresso: 2, alvo: 3 },
    { codigo: 'mesa-cheia', conquistada: false, progresso: 64, alvo: 100 },
    { codigo: 'maratonista', conquistada: true, progresso: 20, alvo: 20 },
    { codigo: 'bem-avaliado', conquistada: false, progresso: 0, alvo: 4.5 },
    { codigo: 'vendedor-de-destaque', conquistada: false, progresso: 8, alvo: 20 },
    { codigo: 'meta-batida', conquistada: false, progresso: 0, alvo: 1 },
  ],
  historico: [
    { mes: '2026-11-01', conquistadas: ['top-3', 'maratonista'] },
    { mes: '2026-10-01', conquistadas: ['mesa-cheia', 'meta-batida'] },
    { mes: '2026-09-01', conquistadas: [] },
  ],
};

describe('Desempenho (UC17)', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<Desempenho>;

  async function renderizar(resposta: RankingDeDesempenho, medalhas: MedalhasDoGarcom = MEDALHAS): Promise<HTMLElement> {
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
    http.expectOne(`${API}/api/indicadores/medalhas`).flush(medalhas);
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
    // Sem avaliações suficientes no restaurante, a avaliação nem aparece como parte do índice.
    expect(texto(pagina)).not.toContain('Avaliação dos clientes');
  });

  const COM_AVALIACAO: RankingDeDesempenho = { ...RANKING, pesoFaturamento: 0.4, pesoMesasAtendidas: 0.3, pesoAvaliacao: 0.3 };

  it('com avaliações, mostra a nota do próprio garçom', async () => {
    const pagina = await renderizar({
      ...COM_AVALIACAO,
      posicoes: [{ ...RANKING.posicoes[0], notaConsiderada: 4.59 }],
    });

    expect(texto(pagina)).toContain('Avaliação dos clientes');
    expect(texto(pagina)).toContain('4,59 / 5');
  });

  it('abaixo do mínimo, não mostra a nota', async () => {
    const pagina = await renderizar(COM_AVALIACAO);

    expect(texto(pagina)).not.toContain('/ 5');
  });

  it('mostra as medalhas do mês com o que falta e as conquistas dos meses anteriores', async () => {
    const pagina = await renderizar(RANKING);

    const conquistadas = [...pagina.querySelectorAll('.grade li.conquistada strong')].map((e) => e.textContent?.trim());
    expect(conquistadas).toEqual(['Pódio', 'Maratonista']);
    expect(texto(pagina)).toContain('2º no mês');
    expect(texto(pagina)).toContain('64/100 mesas');
    expect(texto(pagina)).toContain('8/20 da meta');
    expect(texto(pagina)).toContain('Conquistas anteriores');
    const meses = [...pagina.querySelectorAll('.historico li')];
    expect(meses.map((m) => m.querySelector('.mes')?.textContent?.trim().toLowerCase())).toEqual(['outubro 2026', 'setembro 2026']);
    expect(meses[0].querySelectorAll('.selo').length).toBe(2);
  });

  it('sem turnos no período, explica em vez de mostrar tela vazia', async () => {
    const pagina = await renderizar({ ...RANKING, totalNoRanking: 0, posicoes: [] });

    expect(texto(pagina)).toContain('Ainda sem turnos no período');
  });
});
