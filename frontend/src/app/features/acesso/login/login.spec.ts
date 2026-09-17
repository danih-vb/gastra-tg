import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { ARMAZENAMENTO_DA_SESSAO } from '../../../core/configuracao';
import { ArmazenamentoEmMemoria, loginLiberado } from '../../../core/sessao/testes';
import { Login } from './login';

const API = 'http://localhost:5019';

describe('Login', () => {
  let controle: HttpTestingController;
  let router: Router;

  async function renderizar(queryParams: Record<string, string> = {}): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: new ArmazenamentoEmMemoria() },
        { provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: convertToParamMap(queryParams) } } },
      ],
    }).compileComponents();

    controle = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    const fixture = TestBed.createComponent(Login);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  async function preencherEEnviar(pagina: HTMLElement, email: string, senha: string): Promise<void> {
    const campoEmail = pagina.querySelector<HTMLInputElement>('#email')!;
    const campoSenha = pagina.querySelector<HTMLInputElement>('#senha')!;
    campoEmail.value = email;
    campoEmail.dispatchEvent(new Event('input'));
    campoSenha.value = senha;
    campoSenha.dispatchEvent(new Event('input'));
    pagina.querySelector('form')!.dispatchEvent(new Event('submit'));
  }

  afterEach(() => controle.verify());

  it('com acesso liberado, vai para a tela inicial do papel', async () => {
    const pagina = await renderizar();

    await preencherEEnviar(pagina, 'ana@gastra.test', 'segredo123');
    controle.expectOne(`${API}/api/autenticacao/login`).flush(loginLiberado('Metre'));

    expect(router.navigateByUrl).toHaveBeenCalledWith('/alocacao');
  });

  it('não segue um "voltar" que aponta para fora do sistema', async () => {
    const pagina = await renderizar({ voltar: '//site-malicioso.example' });

    await preencherEEnviar(pagina, 'ana@gastra.test', 'segredo123');
    controle.expectOne(`${API}/api/autenticacao/login`).flush(loginLiberado('Garcom'));

    expect(router.navigateByUrl).toHaveBeenCalledWith('/comandas');
  });

  it('mostra a mensagem da API quando a senha está errada', async () => {
    const pagina = await renderizar();

    await preencherEEnviar(pagina, 'ana@gastra.test', 'errada');
    controle
      .expectOne(`${API}/api/autenticacao/login`)
      .flush({ erros: ['E-mail ou senha inválidos.'] }, { status: 401, statusText: 'Unauthorized' });
    TestBed.tick();

    expect(pagina.querySelector('[role="alert"]')?.textContent).toContain('E-mail ou senha inválidos.');
  });

  it('não chama a API com o formulário vazio', async () => {
    const pagina = await renderizar();

    pagina.querySelector('form')!.dispatchEvent(new Event('submit'));
    TestBed.tick();

    controle.expectNone(`${API}/api/autenticacao/login`);
    expect(pagina.textContent).toContain('Informe um e-mail válido.');
  });
});
