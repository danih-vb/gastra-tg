import { ChangeDetectionStrategy, Component, ElementRef, afterNextRender, inject, input, output } from '@angular/core';
import { Icone } from '../icone/icone';

/**
 * Painel que sobe pela parte de baixo da tela (bottom sheet). É onde ficam as ações rápidas do garçom: abrir mesa,
 * lançar item, registrar restrição, cancelar. Subir de baixo deixa os botões perto do polegar de quem segura o
 * celular com uma mão só.
 */
@Component({
  selector: 'app-folha',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icone],
  template: `
    <div class="fundo" (click)="fundoClicado($event)">
      <section class="folha" role="dialog" aria-modal="true" [attr.aria-label]="titulo()" tabindex="-1">
        <header>
          <div>
            <h2>{{ titulo() }}</h2>
            @if (subtitulo()) {
              <p class="suave pequeno">{{ subtitulo() }}</p>
            }
          </div>
          <button class="botao-icone" type="button" aria-label="Fechar" (click)="fechar.emit()">
            <app-icone nome="x" />
          </button>
        </header>
        <ng-content />
      </section>
    </div>
  `,
  styleUrl: './folha.scss',
  host: { '(document:keydown.escape)': 'fechar.emit()' },
})
export class Folha {
  private readonly elemento: ElementRef<HTMLElement> = inject(ElementRef);

  readonly titulo = input.required<string>();
  readonly subtitulo = input('');
  readonly fechar = output<void>();

  constructor() {
    // O foco vai para o painel: quem usa leitor de tela ou teclado continua dentro do que acabou de abrir.
    afterNextRender(() => this.elemento.nativeElement.querySelector<HTMLElement>('.folha')?.focus());
  }

  protected fundoClicado(evento: MouseEvent): void {
    if ((evento.target as HTMLElement).classList.contains('fundo')) {
      this.fechar.emit();
    }
  }
}
