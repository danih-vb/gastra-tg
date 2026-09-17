import { Component, input } from '@angular/core';

/**
 * Marca a tela de um módulo que ainda vai ser construída conforme o protótipo avaliado pelas heurísticas de Nielsen
 * (#47, #80). A rota e a proteção por papel já funcionam.
 */
@Component({
  selector: 'app-em-construcao',
  template: `
    <section aria-labelledby="titulo-modulo">
      <h2 id="titulo-modulo">{{ titulo() }}</h2>
      <p>Tela em construção, conforme o protótipo ({{ casosDeUso() }}).</p>
    </section>
  `,
})
export class EmConstrucao {
  // Preenchidos pelo "data" da rota (withComponentInputBinding).
  readonly titulo = input('Módulo');
  readonly casosDeUso = input('');
}
