import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AlocacaoDoTurno, GarcomDoTurno } from '../../core/api/modelos';
import { API, praca } from '../../core/api/testes';
import { dataDeHoje, periodoDoTurno } from '../comandas/turno';
import { Alocacao } from './alocacao';

const GARCONS: GarcomDoTurno[] = [
  { id: 11, nome: 'Carla Mendes' },
  { id: 12, nome: 'João Pereira' },
  { id: 13, nome: 'Ana Souza' },
];

function turno(designacoes: [number, number][], confirmada = false, servicoDisponivel: boolean | null = true): AlocacaoDoTurno {
  const nomes = new Map(GARCONS.map((g) => [g.id, g.nome]));
  const codigos = new Map([
    [1, 'A'],
    [2, 'B'],
  ]);
  return {
    data: dataDeHoje(),
    periodo: periodoDoTurno(),
    confirmada,
    servicoDisponivel,
    designacoes: designacoes.map(([garcomId, pracaId]) => ({
      garcomId,
      garcomNome: nomes.get(garcomId)!,
      pracaId,
      pracaCodigo: codigos.get(pracaId)!,
    })),
  };
}

describe('Alocação (UC15, UC21, UC22)', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<Alocacao>;

  async function renderizar(
    existente: AlocacaoDoTurno | 'sem-turno' = 'sem-turno',
    vagas: [number, number] = [2, 1],
  ): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [Alocacao],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(Alocacao);
    await fixture.whenStable();

    // Por padrão, praça A com 2 vagas e praça B com 1: com 3 presentes, todas ficam cheias.
    http.expectOne(`${API}/api/pracas`).flush([
      { ...praca(1, 'A'), quantidadeGarcons: vagas[0] },
      { ...praca(2, 'B'), quantidadeGarcons: vagas[1] },
    ]);
    http.expectOne(`${API}/api/alocacoes/garcons`).flush(GARCONS);
    const pedido = http.expectOne(`${API}/api/alocacoes/${dataDeHoje()}/${periodoDoTurno()}`);
    if (existente === 'sem-turno') {
      pedido.flush({ erros: ['Não há alocação para este turno.'] }, { status: 404, statusText: 'Not Found' });
    } else {
      pedido.flush(existente);
    }
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function botao(pagina: HTMLElement, texto: string): HTMLButtonElement {
    return [...pagina.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.includes(texto))!;
  }

  afterEach(() => http.verify());

  it('começa pela presença, com todos marcados e as vagas à vista', async () => {
    const pagina = await renderizar();

    expect(pagina.querySelectorAll('.presenca input:checked').length).toBe(3);
    expect(pagina.textContent).toContain('3');
    expect(botao(pagina, 'Gerar sugestão').disabled).toBe(false);
  });

  it('com mais presentes que vagas, explica o problema e bloqueia a geração', async () => {
    const pagina = await renderizar('sem-turno', [1, 1]); // 2 vagas para 3 garçons

    expect(pagina.querySelector('[role="alert"]')?.textContent).toContain('mais garçons presentes do que vagas');
    expect(botao(pagina, 'Gerar sugestão').disabled).toBe(true);

    // Desmarcar um resolve, sem precisar mexer no cadastro do salão.
    pagina.querySelectorAll<HTMLInputElement>('.presenca input')[2].click();
    await fixture.whenStable();

    expect(pagina.querySelector('[role="alert"]')).toBeNull();
    expect(botao(pagina, 'Gerar sugestão').disabled).toBe(false);
  });

  it('gera a sugestão da RN03 com quem está presente', async () => {
    const pagina = await renderizar();

    botao(pagina, 'Gerar sugestão').click();
    const requisicao = http.expectOne(`${API}/api/alocacoes/sugestao`);
    expect(requisicao.request.body).toEqual({ data: dataDeHoje(), periodo: periodoDoTurno(), garcomIds: [11, 12, 13] });
    requisicao.flush(turno([[11, 1], [12, 1], [13, 2]]));
    await fixture.whenStable();

    expect(pagina.textContent).toContain('Como a sugestão foi feita');
    expect([...pagina.querySelectorAll('.coluna')].length).toBe(2);
    expect(pagina.querySelector('.coluna')?.textContent).toContain('2/2');
  });

  it('na praça cheia, exige escolher com quem trocar e envia a troca (#140)', async () => {
    const pagina = await renderizar(turno([[11, 1], [12, 1], [13, 2]]));

    const cartao = [...pagina.querySelectorAll('.garcom')].find((a) => a.textContent?.includes('Ana Souza'))!;
    cartao.querySelector<HTMLButtonElement>('button')!.click();
    await fixture.whenStable();

    const destino = [...pagina.querySelectorAll<HTMLButtonElement>('.opcoes-praca button')].find((b) => b.textContent?.includes('Praça A'))!;
    expect(destino.textContent).toContain('Cheia');
    destino.click();
    await fixture.whenStable();

    const salvar = [...pagina.querySelectorAll<HTMLButtonElement>('app-folha .rodape-folha .botao')][1];
    expect(salvar.disabled).toBe(true);
    expect(pagina.textContent).toContain('Trocar com quem?');

    [...pagina.querySelectorAll<HTMLButtonElement>('app-folha .chip')].find((c) => c.textContent?.includes('Carla'))!.click();
    await fixture.whenStable();
    expect(salvar.textContent).toContain('Trocar');

    salvar.click();
    const requisicao = http.expectOne(`${API}/api/alocacoes/${dataDeHoje()}/${periodoDoTurno()}/garcons/13`);
    expect(requisicao.request.body).toEqual({ pracaId: 1, trocarComGarcomId: 11 });
    requisicao.flush(turno([[13, 1], [12, 1], [11, 2]]));
    await fixture.whenStable();

    expect(pagina.textContent).toContain('Ajustado');
  });

  it('confirma o turno e trava os ajustes (UC21)', async () => {
    const pagina = await renderizar(turno([[11, 1], [12, 1], [13, 2]]));

    botao(pagina, 'Confirmar alocação').click();
    await fixture.whenStable();
    [...pagina.querySelectorAll<HTMLButtonElement>('app-folha .rodape-folha .botao')][1].click();
    http.expectOne(`${API}/api/alocacoes/${dataDeHoje()}/${periodoDoTurno()}/confirmacao`).flush(
      turno([[11, 1], [12, 1], [13, 2]], true),
    );
    await fixture.whenStable();

    expect(pagina.textContent).toContain('Alocação confirmada');
    expect(pagina.querySelector('.rodape-acoes')).toBeNull();
    expect([...pagina.querySelectorAll('button')].some((b) => b.textContent?.includes('Mudar praça'))).toBe(false);
  });

  it('sem o serviço de análise, todos ficam sem praça e a tela explica (D3)', async () => {
    const pagina = await renderizar();

    botao(pagina, 'Gerar sugestão').click();
    http.expectOne(`${API}/api/alocacoes/sugestao`).flush(turno([], false, false));
    await fixture.whenStable();

    expect(pagina.textContent).toContain('Não foi possível calcular a sugestão agora');
    expect(pagina.querySelector('.coluna.sem-praca')?.textContent).toContain('Carla Mendes');
    expect(botao(pagina, 'Confirmar alocação').disabled).toBe(true);
  });

  it('erro da API aparece na tela', async () => {
    const pagina = await renderizar(turno([[11, 1], [12, 1], [13, 2]]));

    botao(pagina, 'Confirmar alocação').click();
    await fixture.whenStable();
    [...pagina.querySelectorAll<HTMLButtonElement>('app-folha .rodape-folha .botao')][1].click();
    http.expectOne(`${API}/api/alocacoes/${dataDeHoje()}/${periodoDoTurno()}/confirmacao`).flush(
      { erros: ['A alocação deste turno já foi confirmada e não pode mais mudar.'] },
      { status: 422, statusText: 'Unprocessable Entity' },
    );
    await fixture.whenStable();

    expect(pagina.querySelector('[role="alert"]')?.textContent).toContain('já foi confirmada');
  });
});
