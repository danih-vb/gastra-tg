import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { SessaoService } from '../../../core/sessao/sessao.service';
import { SegundoFator } from './segundo-fator';

const API = 'http://localhost:5019';

const CONFIGURACAO = {
  uriConfiguracao: 'otpauth://totp/GASTRA:ana@gastra.local?secret=6IOXJI5LQPA4A6Z4HET7OOSI2LBXBZMV&issuer=GASTRA',
  chaveManual: '6IOXJI5LQPA4A6Z4HET7OOSI2LBXBZMV',
};

describe('Segundo fator (UC02)', () => {
  let controle: HttpTestingController;

  /** O primeiro acesso é o caso que interessa: é quando a chave aparece, uma única vez (RN07). */
  async function renderizar(configurar = true): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [SegundoFator],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap(configurar ? { configurar: '1' } : {}) } },
        },
      ],
    }).compileComponents();

    controle = TestBed.inject(HttpTestingController);
    // Há um login esperando o segundo fator: sem isso a tela volta para o login, e com razão.
    vi.spyOn(TestBed.inject(SessaoService), 'segundoFatorPendente', 'get').mockReturnValue(true);
    vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);

    const fixture = TestBed.createComponent(SegundoFator);
    await fixture.whenStable();

    if (configurar) {
      controle.expectOne(`${API}/api/autenticacao/segundo-fator/configurar`).flush(CONFIGURACAO);
      await fixture.whenStable();
      // O QR é desenhado numa promessa: sem esta volta ao laço de eventos, o SVG ainda não existe.
      await new Promise((resolva) => setTimeout(resolva, 0));
      await fixture.whenStable();
    }

    return fixture.nativeElement as HTMLElement;
  }

  afterEach(() => controle.verify());

  it('desenha o QR code da URI otpauth no próprio navegador', async () => {
    const pagina = await renderizar();

    const svg = pagina.querySelector('.qr-code svg');
    expect(svg).not.toBeNull();
    // Um QR de verdade tem muitos módulos desenhados; um SVG vazio passaria pelo teste acima.
    expect(svg!.innerHTML.length).toBeGreaterThan(200);
    expect(controle.match(() => true)).toEqual([]); // a URI não foi para lugar nenhum
  });

  it('mantém a chave digitável como contingência de quem não consegue apontar a câmera', async () => {
    const pagina = await renderizar();

    const detalhes = pagina.querySelector('details.chave-manual')!;
    expect(detalhes.querySelector('code')?.textContent).toBe(CONFIGURACAO.chaveManual);
    expect(detalhes.querySelector<HTMLAnchorElement>('a')?.getAttribute('href')).toBe(CONFIGURACAO.uriConfiguracao);
  });

  it('avisa que a chave aparece uma única vez (RN07)', async () => {
    const pagina = await renderizar();

    expect(pagina.textContent).toContain('Esta chave aparece só agora');
  });

  it('nos acessos seguintes pede só o código, sem chave nem QR', async () => {
    const pagina = await renderizar(false);

    expect(pagina.querySelector('.qr-code')).toBeNull();
    expect(pagina.querySelector('details.chave-manual')).toBeNull();
    expect(pagina.textContent).toContain('Digite o código de 6 dígitos');
    expect(pagina.querySelector('#codigo')).not.toBeNull();
  });

  it('erro ao gerar a chave aparece na tela em vez de deixar o cartão vazio', async () => {
    await TestBed.configureTestingModule({
      imports: [SegundoFator],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: convertToParamMap({ configurar: '1' }) } } },
      ],
    }).compileComponents();

    controle = TestBed.inject(HttpTestingController);
    vi.spyOn(TestBed.inject(SessaoService), 'segundoFatorPendente', 'get').mockReturnValue(true);
    vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    const fixture = TestBed.createComponent(SegundoFator);
    await fixture.whenStable();
    controle.expectOne(`${API}/api/autenticacao/segundo-fator/configurar`).flush(
      { erros: ['A verificação em duas etapas já foi configurada.'] },
      { status: 422, statusText: 'Unprocessable Entity' },
    );
    await fixture.whenStable();

    const pagina = fixture.nativeElement as HTMLElement;
    expect(pagina.querySelector('[role="alert"]')?.textContent).toContain('já foi configurada');
  });
});
