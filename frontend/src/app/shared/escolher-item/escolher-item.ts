import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CategoriaItemCardapio, Destaque, ItemCardapio } from '../../core/api/modelos';
import { EtiquetaDestaque } from '../destaque/etiqueta-destaque';
import { Icone } from '../icone/icone';
import { CATEGORIAS, FLAGS, ICONES_DAS_FLAGS, semAcento } from '../rotulos';

/**
 * A mesma lista de lançar do garçom (busca, categorias, ícones das restrições, preço e um botão por item), para o
 * Metre e o Gerente escolherem um item sem caixa de seleção. Quem usa decide o que fazer com o item escolhido.
 */
@Component({
  selector: 'app-escolher-item',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, FormsModule, Icone, EtiquetaDestaque],
  template: `
    <div class="busca">
      <app-icone nome="lupa" />
      <input
        id="busca-escolher-item"
        type="search"
        placeholder="Buscar item"
        aria-label="Buscar item do cardápio"
        autocomplete="off"
        [ngModel]="busca()"
        (ngModelChange)="busca.set($event)"
      />
    </div>
    @if (!busca().trim()) {
      <div class="chips rolagem" role="tablist" aria-label="Categorias do cardápio">
        @for (opcao of CATEGORIAS; track opcao.chave) {
          <button
            class="chip"
            type="button"
            role="tab"
            [attr.aria-selected]="categoria() === opcao.chave"
            (click)="categoria.set(categoria() === opcao.chave ? null : opcao.chave)"
          >
            {{ opcao.nome }}
          </button>
        }
      </div>
    }
    <ul class="lista cardapio">
      @for (item of listados(); track item.id) {
        <li [attr.aria-disabled]="podeEscolher(item) ? null : 'true'">
          <div class="principal-item">
            <strong>{{ item.nome }}</strong>
            @if (destaquePorItem().get(item.id); as destaque) {
              <app-etiqueta-destaque [destaque]="destaque" />
            }
            <span class="flags">
              @for (flag of item.flagsDieteticas; track flag) {
                <span class="etiqueta"><app-icone [nome]="ICONES_DAS_FLAGS[flag]" /> {{ FLAGS[flag] }}</span>
              }
              @if (!item.disponivel) {
                <span class="etiqueta">Fora de venda</span>
              }
            </span>
          </div>
          <div class="preco">
            @if (item.precoPromocional !== null) {
              <s>{{ item.preco | currency: 'BRL' }}</s>
              {{ item.precoPromocional | currency: 'BRL' }}
            } @else {
              {{ item.preco | currency: 'BRL' }}
            }
          </div>
          <button
            class="botao compacto"
            type="button"
            [disabled]="!podeEscolher(item)"
            [attr.aria-label]="rotulo() + ': ' + item.nome"
            (click)="escolhido.emit(item)"
          >
            <app-icone [nome]="icone()" /> {{ rotulo() }}
          </button>
        </li>
      } @empty {
        <li class="suave">Nenhum item com "{{ busca().trim() }}".</li>
      }
    </ul>
  `,
  styles: `
    :host { display: grid; gap: 8px; }
    .busca {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 0 12px;
      border: 1px solid var(--borda-forte);
      border-radius: var(--raio-pequeno);
      background: var(--superficie);
      color: var(--texto-suave);
    }
    .busca input {
      flex: 1;
      min-width: 0;
      min-height: var(--toque-minimo);
      border: 0;
      background: transparent;
      font: inherit;
      color: var(--texto);
      outline: none;
    }
    .busca:focus-within { outline: 2px solid var(--laranja-forte); outline-offset: 1px; }
    .lista { max-height: 46vh; overflow-y: auto; }
    .lista > li[aria-disabled='true'] { color: var(--texto-suave); }
    .flags { display: flex; flex-wrap: wrap; gap: 4px; margin-top: 2px; }
    .flags .etiqueta { font-size: 0.6875rem; }
    .preco {
      text-align: right;
      font-weight: 600;
      font-variant-numeric: tabular-nums;
      white-space: nowrap;
    }
    .preco s { display: block; font-size: 0.75rem; font-weight: 400; color: var(--texto-suave); }
  `,
})
export class EscolherItem {
  readonly itens = input.required<ItemCardapio[]>();
  readonly destaques = input<Destaque[]>([]);
  /** Texto do botão de cada item: "Lançar", "Cortesia", "Escolher". */
  readonly rotulo = input('Lançar');
  readonly icone = input('mais');
  /** Cortesia e destaque aceitam item fora de venda (a torta de aniversário que não está no cardápio). */
  readonly aceitarForaDeVenda = input(false);
  readonly escolhido = output<ItemCardapio>();

  protected readonly CATEGORIAS = CATEGORIAS;
  protected readonly FLAGS = FLAGS;
  protected readonly ICONES_DAS_FLAGS = ICONES_DAS_FLAGS;
  protected readonly busca = signal('');
  protected readonly categoria = signal<CategoriaItemCardapio | null>(null);

  protected readonly destaquePorItem = computed(
    () => new Map(this.destaques().filter((d) => d.vigenteHoje).map((d) => [d.itemCardapioId, d])),
  );

  protected readonly listados = computed(() => {
    const termo = semAcento(this.busca().trim());
    const destacados = this.destaquePorItem();
    const ordem = CATEGORIAS.map((c) => c.chave);
    return this.itens()
      .filter((i) => (termo ? semAcento([i.nome, ...i.flagsDieteticas.map((f) => FLAGS[f])].join(' ')).includes(termo) : true))
      .filter((i) => (termo || this.categoria() === null ? true : i.categoria === this.categoria()))
      .sort(
        (a, b) =>
          Number(destacados.has(b.id)) - Number(destacados.has(a.id)) ||
          Number(b.disponivel) - Number(a.disponivel) ||
          ordem.indexOf(a.categoria) - ordem.indexOf(b.categoria) ||
          a.nome.localeCompare(b.nome, 'pt-BR'),
      );
  });

  protected podeEscolher(item: ItemCardapio): boolean {
    return item.disponivel || this.aceitarForaDeVenda();
  }
}
