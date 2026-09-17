import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { ARMAZENAMENTO_DA_SESSAO } from '../configuracao';
import { autenticacaoInterceptor } from './autenticacao.interceptor';
import { SessaoService } from './sessao.service';
import { ArmazenamentoEmMemoria, loginLiberado } from './testes';

const API = 'http://localhost:5019';

describe('autenticacaoInterceptor', () => {
  let http: HttpClient;
  let controle: HttpTestingController;
  let sessao: SessaoService;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([autenticacaoInterceptor])),
        provideHttpClientTesting(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: new ArmazenamentoEmMemoria() },
      ],
    });
    http = TestBed.inject(HttpClient);
    controle = TestBed.inject(HttpTestingController);
    sessao = TestBed.inject(SessaoService);
    router = TestBed.inject(Router);
  });

  afterEach(() => controle.verify());

  function logar(): string {
    sessao.entrar('ana@gastra.test', 'segredo123').subscribe();
    const resposta = loginLiberado('Garcom');
    controle.expectOne(`${API}/api/autenticacao/login`).flush(resposta);
    return resposta.tokenAcesso!;
  }

  it('envia o token nas chamadas à API', () => {
    const token = logar();

    http.get(`${API}/api/comandas`).subscribe();

    expect(controle.expectOne(`${API}/api/comandas`).request.headers.get('Authorization')).toBe(`Bearer ${token}`);
  });

  it('nunca envia o token para outro endereço', () => {
    logar();

    http.get('https://outro-site.example/dados').subscribe();

    expect(controle.expectOne('https://outro-site.example/dados').request.headers.has('Authorization')).toBe(false);
  });

  it('401 fora do login encerra a sessão e leva ao login', () => {
    logar();
    const navegar = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    http.get(`${API}/api/comandas`).subscribe({ error: () => undefined });
    controle.expectOne(`${API}/api/comandas`).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(sessao.autenticado()).toBe(false);
    expect(navegar).toHaveBeenCalledWith(['/acesso/login'], { queryParams: { sessao: 'encerrada' } });
  });

  it('401 do próprio login (senha errada) não mexe na navegação', () => {
    const navegar = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    sessao.entrar('ana@gastra.test', 'errada').subscribe({ error: () => undefined });
    controle
      .expectOne(`${API}/api/autenticacao/login`)
      .flush({ erros: ['E-mail ou senha inválidos.'] }, { status: 401, statusText: 'Unauthorized' });

    expect(navegar).not.toHaveBeenCalled();
  });
});
