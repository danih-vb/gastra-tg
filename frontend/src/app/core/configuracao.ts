import { InjectionToken } from '@angular/core';
import { ambiente } from '../../environments/environment';

/**
 * Endereço da API do GASTRA. Vem do ambiente da compilação (decisão D12): absoluto no desenvolvimento
 * (`ng serve` e API em portas diferentes) e vazio no contêiner, onde o nginx repassa /api e tudo sai da
 * mesma origem.
 */
export const URL_DA_API = new InjectionToken<string>('URL_DA_API', {
  providedIn: 'root',
  factory: () => ambiente.urlDaApi,
});

/**
 * Onde a sessão fica guardada. É o sessionStorage: a sessão some ao fechar a aba, o que reduz o tempo que um token
 * fica disponível num computador compartilhado do salão. Injetável para os testes usarem um armazenamento em memória.
 */
export const ARMAZENAMENTO_DA_SESSAO = new InjectionToken<Storage>('ARMAZENAMENTO_DA_SESSAO', {
  providedIn: 'root',
  factory: () => sessionStorage,
});
