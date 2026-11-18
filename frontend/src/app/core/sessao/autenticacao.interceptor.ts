import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { URL_DA_API } from '../configuracao';
import { SessaoService } from './sessao.service';

/**
 * Envia o token nas chamadas à API do GASTRA (e só nelas: o token nunca vai para outro endereço). Se a API responder
 * 401 fora do login, a sessão acabou (logoff em outro aparelho, papel alterado, token vencido): volta para o login.
 */
export const autenticacaoInterceptor: HttpInterceptorFn = (requisicao, proximo) => {
  const api = inject(URL_DA_API);
  const sessao = inject(SessaoService);
  const router = inject(Router);

  if (!requisicao.url.startsWith(api)) {
    return proximo(requisicao);
  }

  const token = sessao.token();
  const comToken = token ? requisicao.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : requisicao;

  return proximo(comToken).pipe(
    catchError((erro: unknown) => {
      const ehDoLogin = requisicao.url.startsWith(`${api}/api/autenticacao/`);
      if (erro instanceof HttpErrorResponse && erro.status === 401 && !ehDoLogin) {
        sessao.encerrarLocalmente();
        void router.navigate(['/acesso/login'], { queryParams: { sessao: 'encerrada' } });
      }
      return throwError(() => erro);
    }),
  );
};
