import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { Destaque } from '../../core/api/modelos';
import { Icone } from '../icone/icone';

/** Etiqueta colorida do destaque do dia ou da meta do mês, com a barra de progresso da meta. */
@Component({
  selector: 'app-etiqueta-destaque',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icone],
  template: `
    @if (destaque().tipo === 'DestaqueDoDia') {
      <span class="etiqueta destaque-do-dia"><app-icone nome="faisca" /> Destaque do dia</span>
    } @else {
      <span class="meta" [class.batida]="batida()">
        <span class="etiqueta meta-do-mes">
          <app-icone nome="trofeu" /> Meta do mês · {{ destaque().vendidos }}/{{ destaque().meta }}
        </span>
        <span class="barra" role="img" [attr.aria-label]="destaque().vendidos + ' de ' + destaque().meta + ' vendidos'">
          <span [style.width.%]="percentual()"></span>
        </span>
      </span>
    }
  `,
  styles: `
    :host { display: inline-flex; }
    .destaque-do-dia { color: #8a3a12; background: #ffe2c9; font-weight: 600; }
    .meta { display: inline-flex; flex-direction: column; gap: 3px; min-width: 120px; }
    .meta-do-mes { color: #1f4f8a; background: #dbe9fb; font-weight: 600; }
    .barra { display: block; height: 4px; overflow: hidden; border-radius: 2px; background: #dbe9fb; }
    .barra span { display: block; height: 100%; background: #2f6fc0; }
    .batida .meta-do-mes { color: var(--sucesso); background: var(--sucesso-suave); }
    .batida .barra span { background: var(--sucesso); }
  `,
})
export class EtiquetaDestaque {
  readonly destaque = input.required<Destaque>();
  protected readonly percentual = computed(() => {
    const meta = this.destaque().meta ?? 0;
    return meta ? Math.min(100, (this.destaque().vendidos / meta) * 100) : 0;
  });
  protected readonly batida = computed(() => this.percentual() >= 100);
}
