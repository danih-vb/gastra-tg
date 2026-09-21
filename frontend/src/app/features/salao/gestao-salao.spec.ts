import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { API, mesa, praca } from '../../core/api/testes';
import { AvisoService } from '../../shared/aviso/aviso.service';
import { GestaoSalao } from './gestao-salao';

describe('Gestão do salão (UC24)', () => {
  let http!: HttpTestingController;
  let fixture: ComponentFixture<GestaoSalao>;
  let avisos: AvisoService;

  async function renderizar(comPracas = true): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [GestaoSalao],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    avisos = TestBed.inject(AvisoService);
    fixture = TestBed.createComponent(GestaoSalao);
    await fixture.whenStable();
    responderListas(comPracas);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  function responderListas(comPracas = true): void {
    http.expectOne(`${API}/api/pracas`).flush(
      comPracas ? [{ ...praca(1, 'A'), quantidadeGarcons: 3 }, { ...praca(2, 'B'), quantidadeGarcons: 2 }] : [],
    );
    http.expectOne(`${API}/api/mesas`).flush(
      comPracas ? [mesa(10, '10', 1, 4), mesa(2, '02', 1, 6), mesa(20, '20', 2, 2)] : [],
    );
  }

  function botao(raiz: ParentNode, texto: string): HTMLButtonElement {
    return [...raiz.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.trim().includes(texto))!;
  }

  function preencher(pagina: HTMLElement, seletor: string, valor: string): void {
    const campo = pagina.querySelector<HTMLInputElement | HTMLSelectElement>(seletor)!;
    campo.value = valor;
    campo.dispatchEvent(new Event(campo.tagName === 'SELECT' ? 'change' : 'input'));
  }

  afterEach(() => http.verify());

  it('agrupa as mesas por praça, em ordem, e soma vagas e lugares', async () => {
    const pagina = await renderizar();

    expect(pagina.textContent).toContain('5 garçons por turno'); // 3 + 2 vagas
    const pracaA = [...pagina.querySelectorAll('section.cartao')].find((s) => s.textContent?.includes('Praça A'))!;
    expect([...pracaA.querySelectorAll('.principal-item strong')].map((e) => e.textContent)).toEqual(['Mesa 02', 'Mesa 10']);
    expect(pracaA.textContent).toContain('2 mesas · 10 lugares');
  });

  it('sem praça cadastrada, explica o primeiro passo e não deixa criar mesa', async () => {
    const pagina = await renderizar(false);

    expect(pagina.textContent).toContain('Nenhuma praça cadastrada');
    expect(botao(pagina, 'Nova mesa').disabled).toBe(true);
  });

  it('cria a praça com código e vagas', async () => {
    const pagina = await renderizar();

    botao(pagina, 'Nova praça').click();
    await fixture.whenStable();
    preencher(pagina, '#praca-codigo', 'Varanda');
    preencher(pagina, '#praca-garcons', '2');
    pagina.querySelector('app-folha form')!.dispatchEvent(new Event('submit'));

    const requisicao = http.expectOne({ method: 'POST', url: `${API}/api/pracas` });
    expect(requisicao.request.body).toEqual({ codigo: 'Varanda', quantidadeGarcons: 2 });
    requisicao.flush(praca(3, 'Varanda'));
    responderListas();
    await fixture.whenStable();

    expect(avisos.aviso()?.mensagem).toBe('Praça Varanda salva');
  });

  it('mostra dentro do painel a mensagem da API quando o código repete', async () => {
    const pagina = await renderizar();

    botao(pagina, 'Nova praça').click();
    await fixture.whenStable();
    preencher(pagina, '#praca-codigo', 'A');
    pagina.querySelector('app-folha form')!.dispatchEvent(new Event('submit'));
    http.expectOne({ method: 'POST', url: `${API}/api/pracas` }).flush(
      { erros: ['Já existe uma praça com este código.'] },
      { status: 422, statusText: 'Unprocessable Entity' },
    );
    await fixture.whenStable();

    expect(pagina.querySelector('app-folha [role="alert"]')?.textContent).toContain('Já existe uma praça com este código.');
  });

  it('nova mesa vai para a praça escolhida; editar mesa não deixa mudar de praça (REL01)', async () => {
    const pagina = await renderizar();

    botao(pagina, 'Nova mesa').click();
    await fixture.whenStable();
    preencher(pagina, '#mesa-numero', '21');
    preencher(pagina, '#mesa-capacidade', '6');
    preencher(pagina, '#mesa-praca', '2');
    pagina.querySelector('app-folha form')!.dispatchEvent(new Event('submit'));

    const criacao = http.expectOne({ method: 'POST', url: `${API}/api/mesas` });
    expect(criacao.request.body).toEqual({ numero: '21', capacidade: 6, pracaId: 2 });
    criacao.flush(mesa(21, '21', 2));
    responderListas();
    await fixture.whenStable();

    botao([...pagina.querySelectorAll('section.cartao')].find((s) => s.textContent?.includes('Praça A'))!, 'Editar').click();
    await fixture.whenStable();

    expect(pagina.querySelector('#mesa-praca')).toBeNull();
    expect(pagina.querySelector('app-folha')?.textContent).toContain('A praça da mesa não muda');

    preencher(pagina, '#mesa-capacidade', '8');
    pagina.querySelector('app-folha form')!.dispatchEvent(new Event('submit'));
    const edicao = http.expectOne({ method: 'PUT', url: `${API}/api/mesas/2` });
    expect(edicao.request.body).toEqual({ numero: '02', capacidade: 8 });
    edicao.flush(mesa(2, '02', 1, 8));
    responderListas();
    await fixture.whenStable();
  });
});
