import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, isDevMode, provideBrowserGlobalErrorListeners } from '@angular/core';
import {
  provideRouter,
  withComponentInputBinding,
  withNavigationErrorHandler,
} from '@angular/router';
import { provideServiceWorker } from '@angular/service-worker';
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
    // Instalável no celular (PWA). Guarda só o casco (HTML, JS, CSS e ícones): nada de /api, porque conta em tempo real
    // vinda de cache é pior do que erro. Só funciona em origem segura: HTTPS ou localhost.
    provideServiceWorker('ngsw-worker.js', { enabled: !isDevMode(), registrationStrategy: 'registerWhenStable:30000' }),
  ],
};
