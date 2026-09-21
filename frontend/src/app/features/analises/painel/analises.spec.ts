import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RankingDeDesempenho, RelatorioCardapio, RelatorioGarcons, RelatorioHorarios, RelatorioPracas } from '../../../core/api/modelos';
import { API } from '../../../core/api/testes';
import { provedoresDeLocalizacao } from '../../../core/localizacao';
import { Analises, datasDoPeriodo } from './analises';

const PERIODO = { inicio: '2026-08-19', fim: '2026-09-17' };

const GARCONS: RelatorioGarcons = {
  periodo: PERIODO,
  garcons: [
    { garcomId: 13, nome: 'Ana Souza', faturamento: 28940, comandas: 212, turnos: 22, faturamentoPorTurno: 1315.45, mesasAtendidas: 205, ticketMedio: 136.51, tempoMedioAtendimentoMinutos: 58 },
  ],
  totais: { faturamento: 80130, comandas: 600, ticketMedio: 133.55, tempoMedioAtendimentoMinutos: 58 },
};
const PRACAS: RelatorioPracas = {
  periodo: PERIODO,
  pracas: [
    { pracaId: 1, codigo: 'A', faturamento: 71240, comandas: 540, turnosComMovimento: 52, faturamentoPorTurno: 1370, ticketMedio: 131.9, faturamentoMedioHistorico: 1370 },
    { pracaId: 2, codigo: 'B', faturamento: 29830, comandas: 214, turnosComMovimento: 38, faturamentoPorTurno: 785, ticketMedio: 139.4, faturamentoMedioHistorico: 785 },
  ],
};
const CARDAPIO: RelatorioCardapio = {
  periodo: PERIODO,
  categorias: [],
  itens: [
    { itemCardapioId: 17, nome: 'Caipirinha de limão', categoria: 'Bebida', quantidade: 1102, faturamento: 26448, participacaoPercentual: 15.9 },
    { itemCardapioId: 4, nome: 'Picanha na chapa', categoria: 'PratoPrincipal', quantidade: 418, faturamento: 49742, participacaoPercentual: 29.9 },
  ],
};
const HORARIOS: RelatorioHorarios = {
  periodo: PERIODO,
  porHora: [
    { pracaId: 1, pracaCodigo: 'A', hora: 20, faturamento: 9800, comandas: 70 },
    { pracaId: 1, pracaCodigo: 'A', hora: 21, faturamento: 11200, comandas: 80 },
    { pracaId: 2, pracaCodigo: 'B', hora: 21, faturamento: 7900, comandas: 60 },
  ],
  porDiaDaSemana: [],
};
const RANKING: RankingDeDesempenho = {
  periodo: PERIODO,
  pesoFaturamento: 0.5,
  pesoMesasAtendidas: 0.5,
  totalNoRanking: 1,
  posicoes: [{ posicao: 1, garcomId: 13, nome: 'Ana Souza', indice: 99.9, faturamentoPorTurno: 1315.45, mesasPorTurno: 9.3, turnos: 22 }],
};

describe('datas do período', () => {
  it('inclui o dia de hoje em cada opção', () => {
    const hoje = new Date('2026-09-17T15:00:00');

    expect(datasDoPeriodo('7', hoje)).toEqual({ inicio: '2026-09-11', fim: '2026-09-17' });
    expect(datasDoPeriodo('30', hoje)).toEqual({ inicio: '2026-08-19', fim: '2026-09-17' });
    expect(datasDoPeriodo('mes', hoje)).toEqual({ inicio: '2026-09-01', fim: '2026-09-17' });
  });
});

describe('Análises (UC16, UC17)', () => {
  let http!: HttpTestingController;
  let fixture: ComponentFixture<Analises>;

  async function renderizar(): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [Analises],
      providers: [provideHttpClient(), provideHttpClientTesting(), provedoresDeLocalizacao()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(Analises);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  async function responder(garcons: RelatorioGarcons = GARCONS): Promise<void> {
    const pedido = (nome: string) => http.expectOne((r) => r.url === `${API}/api/indicadores/${nome}`);
    pedido('garcons').flush(garcons);
    pedido('pracas').flush(PRACAS);
    pedido('cardapio').flush(CARDAPIO);
    pedido('horarios').flush(HORARIOS);
    pedido('desempenho').flush(RANKING);
    await fixture.whenStable();
  }

  function texto(pagina: HTMLElement): string {
    return (pagina.textContent ?? '').replace(/ /g, ' ');
  }

  afterEach(() => http.verify());

  it('pede os cinco relatórios com o mesmo período e mostra os totais', async () => {
    const pagina = await renderizar();
    const { inicio, fim } = datasDoPeriodo('30');
    const pedidos = http.match((r) => r.url.startsWith(`${API}/api/indicadores/`));
    expect(pedidos.length).toBe(5);
    expect(pedidos.every((p) => p.request.params.get('inicio') === inicio && p.request.params.get('fim') === fim)).toBe(true);
    pedidos.forEach((p) => {
      const nome = p.request.url.split('/').pop();
      p.flush({ garcons: GARCONS, pracas: PRACAS, cardapio: CARDAPIO, horarios: HORARIOS, desempenho: RANKING }[nome!]!);
    });
    await fixture.whenStable();

    expect(texto(pagina)).toContain('R$ 80.130');
    expect(texto(pagina)).toContain('600');
    expect(texto(pagina)).toContain('R$ 133,55');
    expect(texto(pagina)).toContain('58 min');
  });

  it('destaca como alto potencial a praça acima da média, pela mesma regra da RN03', async () => {
    const pagina = await renderizar();
    await responder();

    const barras = [...pagina.querySelectorAll('.barra')];
    expect(barras.find((b) => b.textContent?.includes('Praça A'))?.classList.contains('destaque')).toBe(true);
    expect(barras.find((b) => b.textContent?.includes('Praça B'))?.classList.contains('destaque')).toBe(false);
  });

  it('monta o mapa de calor por praça e hora, com o pico', async () => {
    const pagina = await renderizar();
    await responder();

    const cabecalho = [...pagina.querySelectorAll('.calor thead th')].map((th) => th.textContent?.trim());
    expect(cabecalho).toEqual(['Praça', '20h', '21h']);
    expect(texto(pagina)).toContain('Pico: 21h na praça A');
  });

  it('ranking com os detalhes do garçom e itens ordenados pelo faturamento', async () => {
    const pagina = await renderizar();
    await responder();

    expect(texto(pagina)).toContain('Ana Souza');
    expect(texto(pagina)).toContain('99,9');
    expect(texto(pagina)).toContain('R$ 136,51');

    const itens = [...pagina.querySelectorAll('section[aria-labelledby="t-itens"] tbody tr')].map((tr) => tr.querySelector('strong')?.textContent);
    expect(itens).toEqual(['Picanha na chapa', 'Caipirinha de limão']);
  });

  it('período sem comanda fechada explica em vez de mostrar zeros', async () => {
    const pagina = await renderizar();
    await responder({ ...GARCONS, garcons: [], totais: { faturamento: 0, comandas: 0, ticketMedio: 0, tempoMedioAtendimentoMinutos: 0 } });

    expect(texto(pagina)).toContain('Sem movimento no período');
    expect(pagina.querySelector('.kpis')).toBeNull();
  });

  it('trocar o período pede os relatórios de novo', async () => {
    const pagina = await renderizar();
    await responder();

    [...pagina.querySelectorAll<HTMLButtonElement>('.segmentado button')].find((b) => b.textContent?.includes('7 dias'))!.click();
    const { inicio } = datasDoPeriodo('7');
    const pedidos = http.match((r) => r.url.startsWith(`${API}/api/indicadores/`));
    expect(pedidos.length).toBe(5);
    expect(pedidos.every((p) => p.request.params.get('inicio') === inicio)).toBe(true);
    pedidos.forEach((p) => {
      const nome = p.request.url.split('/').pop();
      p.flush({ garcons: GARCONS, pracas: PRACAS, cardapio: CARDAPIO, horarios: HORARIOS, desempenho: RANKING }[nome!]!);
    });
    await fixture.whenStable();
  });
});
