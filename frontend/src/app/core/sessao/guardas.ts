import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { Papel } from './modelos';
import { SessaoService } from './sessao.service';

/** Tela inicial de cada papel, conforme o trabalho do dia de cada um. */
export const PAGINA_INICIAL: Record<Papel, string> = {
  Garcom: '/comandas',
  Metre: '/alocacao',
  Coordenador: '/cardapio',
  Gerente: '/analises',
};

/**
 * RNF04 — só entra na rota quem tem um dos papéis. Sem login, vai para o login e volta depois. Logado com outro papel,
 * vai para "sem permissão". A API também confere o papel em cada endpoint: a guarda só evita mostrar tela inútil.
 */
export function exigePapel(...papeis: Papel[]): CanActivateFn {
  return (_rota, estado): boolean | UrlTree => {
    const sessao = inject(SessaoService);
    const router = inject(Router);

    if (!sessao.autenticado() || sessao.token() === null) {
      return router.createUrlTree(['/acesso/login'], { queryParams: { voltar: estado.url } });
    }
    return sessao.temPapel(...papeis) ? true : router.createUrlTree(['/sem-permissao']);
  };
}

/** A raiz do site leva à tela inicial do papel, ou ao login. */
export const paraPaginaInicial: CanActivateFn = (): UrlTree => {
  const sessao = inject(SessaoService);
  const router = inject(Router);
  const usuario = sessao.usuario();

  return usuario && sessao.token() ? router.parseUrl(PAGINA_INICIAL[usuario.papel]) : router.parseUrl('/acesso/login');
};
