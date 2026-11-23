import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { CodigoDaMedalha, Medalha, MedalhasDoGarcom } from '../../core/api/modelos';
import { Icone } from '../icone/icone';

export const MEDALHAS: Record<CodigoDaMedalha, { nome: string; icone: string }> = {
  'top-3': { nome: 'Pódio', icone: 'trofeu' },
  'mesa-cheia': { nome: 'Mesa cheia', icone: 'mesa' },
  maratonista: { nome: 'Maratonista', icone: 'relogio' },
  'bem-avaliado': { nome: 'Bem avaliado', icone: 'estrela' },
  'vendedor-de-destaque': { nome: 'Vendedor de destaque', icone: 'faisca' },
  'meta-batida': { nome: 'Meta batida', icone: 'alvo' },
};

/** As seis medalhas do mês, com quanto falta para cada uma, e o que foi conquistado nos meses anteriores. */
@Component({
  selector: 'app-medalhas',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, DecimalPipe, Icone],
  template: `
    <ul class="grade" aria-label="Medalhas do mês">
      @for (medalha of dados().medalhas; track medalha.codigo) {
        <li [class.conquistada]="medalha.conquistada">
          <span class="selo"><app-icone [nome]="MEDALHAS[medalha.codigo].icone" /></span>
          <strong>{{ MEDALHAS[medalha.codigo].nome }}</strong>
          <span class="progresso pequeno">
            @switch (medalha.codigo) {
              @case ('top-3') {
                {{ medalha.progresso ? medalha.progresso + 'º no mês' : 'Sem posição' }}
              }
              @case ('mesa-cheia') {
                {{ medalha.progresso }}/{{ medalha.alvo }} mesas
              }
              @case ('maratonista') {
                {{ medalha.progresso }}/{{ medalha.alvo }} turnos
              }
              @case ('bem-avaliado') {
                @if (medalha.progresso) {
                  Nota {{ medalha.progresso | number: '1.1-1' }}
                } @else {
                  Sem avaliações
                }
              }
              @case ('vendedor-de-destaque') {
                {{ medalha.progresso }}/{{ medalha.alvo }} da meta
              }
              @case ('meta-batida') {
                {{ medalha.conquistada ? 'Equipe' : 'Ainda não' }}
              }
            }
          </span>
          @if (!medalha.conquistada && temBarra(medalha)) {
            <span class="barra" aria-hidden="true"><span [style.width.%]="(medalha.progresso / medalha.alvo) * 100"></span></span>
          }
        </li>
      }
    </ul>

    @if (anteriores().length) {
      <h3>Conquistas anteriores</h3>
      <ul class="lista historico">
        @for (mes of anteriores(); track mes.mes) {
          <li>
            <div class="principal-item">
              <strong class="mes">{{ mes.mes | date: 'MMMM yyyy' }}</strong>
            </div>
            <span class="conquistas">
              @for (codigo of mes.conquistadas; track codigo) {
                <span class="selo mini" [title]="MEDALHAS[codigo].nome">
                  <app-icone [nome]="MEDALHAS[codigo].icone" [rotulo]="MEDALHAS[codigo].nome" />
                </span>
              } @empty {
                <span class="suave">—</span>
              }
            </span>
          </li>
        }
      </ul>
    }
  `,
  styles: `
    :host { display: grid; gap: 12px; }
    .grade {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
      gap: 8px;
      margin: 0;
      padding: 0;
      list-style: none;
    }
    .grade li {
      display: grid;
      justify-items: center;
      gap: 4px;
      padding: 12px 8px;
      text-align: center;
      border: 1px solid var(--borda);
      border-radius: var(--raio);
      background: var(--superficie);
      color: var(--texto-suave);
    }
    .grade li strong { font-size: 0.875rem; color: var(--texto); }
    .grade li:not(.conquistada) strong { color: var(--texto-suave); }
    .grade li.conquistada { border-color: var(--laranja); background: var(--laranja-suave); }
    .selo {
      display: grid;
      place-items: center;
      width: 44px;
      height: 44px;
      border-radius: 50%;
      font-size: 1.375rem;
      color: var(--texto-suave);
      background: var(--fundo);
      border: 2px dashed var(--borda-forte);
    }
    .conquistada .selo, .selo.mini {
      color: #fff;
      background: var(--laranja-forte);
      border: 2px solid var(--laranja-forte);
    }
    .selo.mini { width: 28px; height: 28px; font-size: 0.9375rem; }
    .progresso { font-variant-numeric: tabular-nums; }
    .barra {
      width: 80%;
      height: 6px;
      border-radius: 3px;
      overflow: hidden;
      background: var(--marinho-suave);
    }
    .barra span { display: block; height: 100%; background: var(--laranja); }
    h3 { margin: 4px 0 0; font-size: 0.9375rem; }
    .mes { text-transform: capitalize; }
    .conquistas { display: flex; flex-wrap: wrap; gap: 4px; justify-content: flex-end; }
  `,
})
export class Medalhas {
  readonly dados = input.required<MedalhasDoGarcom>();

  protected readonly MEDALHAS = MEDALHAS;

  /** O histórico começa no mês atual, que já aparece na grade. */
  protected readonly anteriores = computed(() => this.dados().historico.slice(1));

  protected temBarra(medalha: Medalha): boolean {
    return medalha.codigo !== 'top-3' && medalha.codigo !== 'meta-batida' && medalha.codigo !== 'bem-avaliado';
  }
}
