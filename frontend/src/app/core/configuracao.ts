import { InjectionToken } from '@angular/core';

/** Endereço da API do GASTRA. No desenvolvimento, a API roda em http://localhost:5019 (backend/README.md). */
export const URL_DA_API = new InjectionToken<string>('URL_DA_API', {
  providedIn: 'root',
  factory: () => 'http://localhost:5019',
});

/**
 * Onde a sessão fica guardada. É o sessionStorage: a sessão some ao fechar a aba, o que reduz o tempo que um token
 * fica disponível num computador compartilhado do salão. Injetável para os testes usarem um armazenamento em memória.
 */
export const ARMAZENAMENTO_DA_SESSAO = new InjectionToken<Storage>('ARMAZENAMENTO_DA_SESSAO', {
  providedIn: 'root',
  factory: () => sessionStorage,
});
