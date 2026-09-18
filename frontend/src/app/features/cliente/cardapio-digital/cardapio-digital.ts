import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GastraApiService } from '../../../core/api/gastra-api.service';
import { FlagDietetica, ItemCardapio } from '../../../core/api/modelos';
import { mensagensDeErro } from '../../../shared/erros';
import { Icone } from '../../../shared/icone/icone';
import { CATEGORIAS, FLAGS } from '../../../shared/rotulos';

/**
 * UC19 — cardápio digital (RF05). Tela pública: o cliente chega pelo QR code da mesa e não faz login. Não tem
 * função de pedido; o pedido continua sendo feito com o garçom.
 */
@Component({
  selector: 'app-cardapio-digital',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, RouterLink, Icone],
  templateUrl: './cardapio-digital.html',
  styleUrl: './cardapio-digital.scss',
})
export class CardapioDigital {
  private readonly api = inject(GastraApiService);

  protected readonly carregando = signal(true);
  protected readonly erros = signal<string[]>([]);
  protected readonly itens = signal<ItemCardapio[]>([]);
  protected readonly filtros = signal<FlagDietetica[]>([]);

  protected readonly CATEGORIAS = CATEGORIAS;
  protected readonly FLAGS = FLAGS;
  protected readonly flagsPossiveis = Object.keys(FLAGS) as FlagDietetica[];

  /** Os filtros combinam entre si: "vegetariano e sem glúten" mostra só o que atende aos dois. */
  protected readonly filtrados = computed(() =>
    this.itens().filter((item) => this.filtros().every((flag) => item.flagsDieteticas.includes(flag))),
  );

  protected readonly grupos = computed(() =>
    CATEGORIAS.map((categoria) => ({
      ...categoria,
      itens: this.filtrados().filter((item) => item.categoria === categoria.chave),
    })).filter((grupo) => grupo.itens.length > 0),
  );

  constructor() {
    this.carregar();
  }

  protected alternarFiltro(flag: FlagDietetica): void {
    this.filtros.update((atuais) => (atuais.includes(flag) ? atuais.filter((f) => f !== flag) : [...atuais, flag]));
  }

  protected limparFiltros(): void {
    this.filtros.set([]);
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erros.set([]);
    this.api.listarCardapioDigital().subscribe({
      next: (itens) => {
        this.itens.set(itens);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.erros.set(mensagensDeErro(erro));
        this.carregando.set(false);
      },
    });
  }
}
