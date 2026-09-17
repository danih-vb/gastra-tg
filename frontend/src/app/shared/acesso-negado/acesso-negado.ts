import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/** Logado, mas sem o papel exigido pela tela (RNF04). */
@Component({
  selector: 'app-acesso-negado',
  imports: [RouterLink],
  template: `
    <section aria-labelledby="titulo-acesso-negado">
      <h2 id="titulo-acesso-negado">Sem permissão</h2>
      <p>Seu perfil não tem acesso a esta tela.</p>
      <p><a routerLink="/">Voltar para o início</a></p>
    </section>
  `,
})
export class AcessoNegado {}
