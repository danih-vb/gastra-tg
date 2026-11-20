import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import {
  provideRouter,
  withComponentInputBinding,
  withNavigationErrorHandler,
} from '@angular/router';
import { routes } from './app.routes';
import { provedoresDeLocalizacao } from './core/localizacao';
import { autenticacaoInterceptor } from './core/sessao/autenticacao.interceptor';
import { recarregarSeVersaoMudou } from './core/versao-nova';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provedoresDeLocalizacao(),
    provideRouter(
      routes,
      withComponentInputBinding(),
      withNavigationErrorHandler((erro) => recarregarSeVersaoMudou(erro)),
    ),
    provideHttpClient(withInterceptors([autenticacaoInterceptor])),
  ],
};
