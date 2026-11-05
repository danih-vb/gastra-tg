import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ARMAZENAMENTO_DA_SESSAO } from '../configuracao';
import { ResultadoDoLogin } from './modelos';
import { expiracaoDoToken, SessaoService } from './sessao.service';
import { ArmazenamentoEmMemoria, loginLiberado, tokenQueExpiraEm } from './testes';

const API = 'http://localhost:5019';

describe('SessaoService', () => {
  let armazenamento: ArmazenamentoEmMemoria;
  let http: HttpTestingController;

  function criar(): SessaoService {
    return TestBed.inject(SessaoService);
  }

  beforeEach(() => {
    armazenamento = new ArmazenamentoEmMemoria();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ARMAZENAMENTO_DA_SESSAO, useValue: armazenamento },
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('login do garçom libera o acesso direto e guarda nome, papel e validade', () => {
    const sessao = criar();
    let resultado: ResultadoDoLogin | undefined;

    sessao.entrar('ana@gastra.test', 'segredo123').subscribe((r) => (resultado = r));
    const requisicao = http.expectOne(`${API}/api/autenticacao/login`);
    expect(requisicao.request.body).toEqual({ email: 'ana@gastra.test', senha: 'segredo123' });
    requisicao.flush(loginLiberado('Garcom'));

    expect(resultado).toBe('acesso-liberado');
    expect(sessao.autenticado()).toBe(true);
    expect(sessao.usuario()?.papel).toBe('Garcom');
    expect(sessao.temPapel('Garcom', 'Metre')).toBe(true);
    expect(sessao.temPapel('Gerente')).toBe(false);
    expect(armazenamento.length).toBe(1);
  });

  it('login do gerente no primeiro acesso pede a configuração do autenticador, sem liberar acesso', () => {
    const sessao = criar();
    let resultado: ResultadoDoLogin | undefined;

    sessao.entrar('gerente@gastra.test', 'segredo123').subscribe((r) => (resultado = r));
    http.expectOne(`${API}/api/autenticacao/login`).flush({
      tokenAcesso: null,
      nome: null,
      papel: null,
      requerSegundoFator: true,
      requerConfiguracaoSegundoFator: true,
      tokenSegundoFator: 'token-temporario',
    });

    expect(resultado).toBe('configurar-segundo-fator');
    expect(sessao.autenticado()).toBe(false);
    expect(sessao.segundoFatorPendente).toBe(true);

    sessao.configurarSegundoFator().subscribe();
    expect(http.expectOne(`${API}/api/autenticacao/segundo-fator/configurar`).request.body).toEqual({
      tokenSegundoFator: 'token-temporario',
    });
  });

  it('confirmar o segundo fator libera o acesso e descarta o token temporário', () => {
    const sessao = criar();
    sessao.entrar('gerente@gastra.test', 'segredo123').subscribe();
    http.expectOne(`${API}/api/autenticacao/login`).flush({
      tokenAcesso: null,
      nome: null,
      papel: null,
      requerSegundoFator: true,
      requerConfiguracaoSegundoFator: false,
      tokenSegundoFator: 'token-temporario',
    });

    sessao.confirmarSegundoFator('123456').subscribe();
    const requisicao = http.expectOne(`${API}/api/autenticacao/segundo-fator/confirmar`);
    expect(requisicao.request.body).toEqual({ tokenSegundoFator: 'token-temporario', codigo: '123456' });
    requisicao.flush(loginLiberado('Gerente', 'Bruno'));

    expect(sessao.usuario()?.nome).toBe('Bruno');
    expect(sessao.segundoFatorPendente).toBe(false);
  });

  it('sair encerra a sessão no navegador mesmo se a API falhar', () => {
    const sessao = criar();
    sessao.entrar('ana@gastra.test', 'segredo123').subscribe();
    http.expectOne(`${API}/api/autenticacao/login`).flush(loginLiberado('Garcom'));

    sessao.sair().subscribe({ error: () => undefined });
    http.expectOne(`${API}/api/autenticacao/logoff`).flush(null, { status: 0, statusText: 'sem rede' });

    expect(sessao.autenticado()).toBe(false);
    expect(armazenamento.length).toBe(0);
  });

  it('restaura a sessão guardada ao recarregar a página', () => {
    armazenamento.setItem(
      'gastra.sessao',
      JSON.stringify({ token: 't', nome: 'Ana', papel: 'Metre', expiraEm: Date.now() + 60_000 }),
    );

    expect(criar().usuario()?.papel).toBe('Metre');
  });

  it('sessão guardada com token vencido é descartada', () => {
    armazenamento.setItem(
      'gastra.sessao',
      JSON.stringify({ token: 't', nome: 'Ana', papel: 'Metre', expiraEm: Date.now() - 1 }),
    );

    const sessao = criar();

    expect(sessao.autenticado()).toBe(false);
    expect(armazenamento.length).toBe(0);
  });

  it('lê a validade do JWT em milissegundos e trata token malformado como vencido', () => {
    const token = tokenQueExpiraEm(120);
    const esperado = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/'))).exp * 1000;

    expect(expiracaoDoToken(token)).toBe(esperado);
    expect(expiracaoDoToken('isto-nao-e-um-jwt')).toBe(0);
  });
});
