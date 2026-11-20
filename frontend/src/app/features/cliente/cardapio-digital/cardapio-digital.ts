import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GastraApiService } from '../../../core/api/gastra-api.service';
import { FlagDietetica, ItemCardapio } from '../../../core/api/modelos';
import { mensagensDeErro } from '../../../shared/erros';
import { Folha } from '../../../shared/folha/folha';
import { CHAVE_DA_MESA } from '../conta/conta-do-cliente';
import { Icone } from '../../../shared/icone/icone';
import { CATEGORIAS, FLAGS, ICONES_DAS_FLAGS } from '../../../shared/rotulos';

/**
 * UC19 — cardápio digital (RF05). Tela pública: o cliente chega pelo QR code da mesa e não faz login. Não tem
 * função de pedido; o pedido continua sendo feito com o garçom. A lista "Quero pedir" só ajuda o cliente a lembrar
 * o que vai pedir: fica no próprio celular e nunca vai para o servidor, então não identifica ninguém (RN04).
 */
const CHAVE_DA_LISTA = 'gastra.queroPedir';

function lerLista(): number[] {
  try {
    const salva: unknown = JSON.parse(localStorage.getItem(CHAVE_DA_LISTA) ?? '[]');
    return Array.isArray(salva) ? salva.filter((id): id is number => typeof id === 'number') : [];
  } catch {
    return [];
  }
}

function lerCodigoDaMesa(): string | null {
  try {
    return sessionStorage.getItem(CHAVE_DA_MESA);
  } catch {
    return null;
  }
}

@Component({
  selector: 'app-cardapio-digital',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, RouterLink, Icone, Folha],
  templateUrl: './cardapio-digital.html',
  styleUrl: './cardapio-digital.scss',
})
export class CardapioDigital {
  private readonly api = inject(GastraApiService);

  protected readonly carregando = signal(true);
  protected readonly erros = signal<string[]>([]);
  protected readonly itens = signal<ItemCardapio[]>([]);
  protected readonly filtros = signal<FlagDietetica[]>([]);
  protected readonly emDetalhe = signal<ItemCardapio | null>(null);
  protected readonly vendoLista = signal(false);

  /** Chegou pela conta da mesa (QR code)? Então a lista já oferece chamar o garçom. */
  protected readonly codigoDaMesa = signal<string | null>(lerCodigoDaMesa());
  protected readonly chamando = signal(false);
  protected readonly garcomChamado = signal(false);
  protected readonly erroAoChamar = signal<string | null>(null);
  protected readonly lista = signal<number[]>(lerLista());

  /** Só os itens que continuam no cardápio: um item retirado some da lista sozinho. */
  protected readonly itensDaLista = computed(() =>
    this.itens().filter((item) => this.lista().includes(item.id)),
  );
  protected readonly totalDaLista = computed(() =>
    this.itensDaLista().reduce((total, item) => total + (item.precoPromocional ?? item.preco), 0),
  );

  protected readonly CATEGORIAS = CATEGORIAS;
  protected readonly FLAGS = FLAGS;
  protected readonly ICONES_DAS_FLAGS = ICONES_DAS_FLAGS;
  protected readonly flagsPossiveis = Object.keys(FLAGS) as FlagDietetica[];

  /** Os filtros combinam entre si: "vegetariano e sem glúten" mostra só o que atende aos dois. */
  protected readonly filtrados = computed(() =>
    this.itens().filter((item) =>
      this.filtros().every((flag) => item.flagsDieteticas.includes(flag)),
    ),
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
    this.filtros.update((atuais) =>
      atuais.includes(flag) ? atuais.filter((f) => f !== flag) : [...atuais, flag],
    );
  }

  protected limparFiltros(): void {
    this.filtros.set([]);
  }

  protected naLista(item: ItemCardapio): boolean {
    return this.lista().includes(item.id);
  }

  protected alternarNaLista(item: ItemCardapio): void {
    this.lista.update((ids) =>
      ids.includes(item.id) ? ids.filter((id) => id !== item.id) : [...ids, item.id],
    );
    this.guardarLista();
  }

  protected limparLista(): void {
    this.lista.set([]);
    this.guardarLista();
    this.vendoLista.set(false);
  }

  protected chamarGarcom(): void {
    const codigo = this.codigoDaMesa();
    if (!codigo || this.chamando()) {
      return;
    }
    this.chamando.set(true);
    this.erroAoChamar.set(null);
    this.api.chamarGarcom(codigo).subscribe({
      next: () => {
        this.chamando.set(false);
        this.garcomChamado.set(true);
      },
      error: (erro: unknown) => {
        this.chamando.set(false);
        this.erroAoChamar.set(mensagensDeErro(erro)[0] ?? 'Não deu para chamar o garçom.');
      },
    });
  }

  private guardarLista(): void {
    try {
      localStorage.setItem(CHAVE_DA_LISTA, JSON.stringify(this.lista()));
    } catch {
      // Sem armazenamento (aba anônima), a lista vale enquanto a página estiver aberta.
    }
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
