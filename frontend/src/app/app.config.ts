import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { routes } from './app.routes';
import { provedoresDeLocalizacao } from './core/localizacao';
import { autenticacaoInterceptor } from './core/sessao/autenticacao.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provedoresDeLocalizacao(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([autenticacaoInterceptor])),
  ],
};
