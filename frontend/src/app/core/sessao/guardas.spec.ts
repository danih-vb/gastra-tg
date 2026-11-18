import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, provideRouter, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { ARMAZENAMENTO_DA_SESSAO } from '../configuracao';
import { exigePapel, paraPaginaInicial } from './guardas';
import { Papel } from './modelos';

describe('guardas de rota', () => {
  function configurar(papel: Papel | null): Router {
    const armazenamento = new Map<string, string>();
    if (papel) {
      armazenamento.set('gastra.sessao', JSON.stringify({ token: 't', nome: 'Ana', papel, expiraEm: Date.now() + 60_000 }));
    }
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: ARMAZENAMENTO_DA_SESSAO,
          useValue: {
            getItem: (c: string) => armazenamento.get(c) ?? null,
            setItem: (c: string, v: string) => armazenamento.set(c, v),
            removeItem: (c: string) => armazenamento.delete(c),
          },
        },
      ],
    });
    return TestBed.inject(Router);
  }

  function executar(papeis: Papel[], url = '/alocacao'): boolean | UrlTree {
    return TestBed.runInInjectionContext(() =>
      exigePapel(...papeis)({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
    ) as boolean | UrlTree;
  }

  it('sem login, manda para o login lembrando a tela pedida', () => {
    const router = configurar(null);

    const resultado = executar(['Metre']);

    expect(router.serializeUrl(resultado as UrlTree)).toBe('/acesso/login?voltar=%2Falocacao');
  });

  it('com o papel certo, deixa entrar', () => {
    configurar('Metre');

    expect(executar(['Metre'])).toBe(true);
  });

  it('com outro papel, manda para sem permissão', () => {
    const router = configurar('Garcom');

    expect(router.serializeUrl(executar(['Metre']) as UrlTree)).toBe('/sem-permissao');
  });

  it.each([
    ['Garcom', '/comandas'],
    ['Metre', '/alocacao'],
    ['Coordenador', '/cardapio'],
    ['Gerente', '/analises'],
  ] as [Papel, string][])('a raiz leva o %s para %s', (papel, esperado) => {
    const router = configurar(papel);

    const resultado = TestBed.runInInjectionContext(() =>
      paraPaginaInicial({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    ) as UrlTree;

    expect(router.serializeUrl(resultado)).toBe(esperado);
  });
});
