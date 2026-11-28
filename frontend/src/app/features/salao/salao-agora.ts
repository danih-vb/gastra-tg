import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { catchError, forkJoin, of } from 'rxjs';
import { GastraApiService } from '../../core/api/gastra-api.service';
import { Comanda, Destaque, ItemCardapio, ItemDoPedido, Mesa, Praca } from '../../core/api/modelos';
import { SessaoService } from '../../core/sessao/sessao.service';
import { mensagensDeErro } from '../../shared/erros';
import { Folha } from '../../shared/folha/folha';
import { Icone } from '../../shared/icone/icone';
import { EscolherItem } from '../../shared/escolher-item/escolher-item';
import { COMPOSICOES, MOTIVOS, RESTRICOES, atrasado, duracao, minutosDesde, FLAGS, ICONES_DAS_FLAGS } from '../../shared/rotulos';

interface LinhaDoSalao {
  comanda: Comanda;
  mesa: string;
  pendentes: number;
  minutos: number;
  atrasados: number;
}

/**
 * Painel do salão para o Metre e o Gerente: o que está aberto agora, por praça. Quem lança e entrega é o garçom da
 * mesa; daqui saem só a cortesia (Metre e Gerente) e o ajuste de preço de um item (Gerente), sempre com motivo.
 * As restrições aparecem porque o Metre também atende a mesa (RN04).
 */
@Component({
  selector: 'app-salao-agora',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, FormsModule, Folha, Icone, EscolherItem],
  templateUrl: './salao-agora.html',
  styleUrl: './salao-agora.scss',
})
export class SalaoAgora {
  private readonly api = inject(GastraApiService);
  private readonly sessao = inject(SessaoService);

  protected readonly carregando = signal(true);
  protected readonly erros = signal<string[]>([]);
  protected readonly pracas = signal<Praca[]>([]);
  protected readonly mesas = signal<Mesa[]>([]);
  protected readonly comandas = signal<Comanda[]>([]);
  protected readonly comandaAberta = signal<Comanda | null>(null);

  protected readonly ehGerente = computed(() => this.sessao.usuario()?.papel === 'Gerente');
  protected readonly cardapio = signal<ItemCardapio[]>([]);
  protected readonly destaques = signal<Destaque[]>([]);
  protected readonly lancandoCortesia = signal(false);
  /** Item comum lançado pelo Metre ou pelo Gerente: espera o garçom da mesa confirmar. */
  protected readonly lancandoItem = signal(false);
  protected readonly itemEmAjuste = signal<ItemDoPedido | null>(null);
  protected readonly salvando = signal(false);
  protected readonly errosDaFolha = signal<string[]>([]);
  /** Item escolhido na lista, à espera da quantidade (e do motivo, na cortesia). */
  protected readonly itemEscolhido = signal<ItemCardapio | null>(null);
  protected readonly quantidade = signal(1);
  protected cortesiaMotivo = '';
  protected readonly FLAGS = FLAGS;
  protected readonly ICONES_DAS_FLAGS = ICONES_DAS_FLAGS;
  protected readonly precoDoEscolhido = computed(() => {
    const item = this.itemEscolhido();
    return item ? (item.precoPromocional ?? item.preco) * this.quantidade() : 0;
  });
  protected novoPreco: number | null = null;
  protected motivoDoAjuste = '';


  protected readonly COMPOSICOES = COMPOSICOES;
  protected readonly duracao = duracao;
  protected readonly MOTIVOS = MOTIVOS;
  protected readonly RESTRICOES = RESTRICOES;

  protected readonly pendentesTotais = computed(() =>
    this.comandas().reduce((total, c) => total + c.itens.filter((i) => i.status === 'Pendente').length, 0),
  );

  protected readonly emConsumo = computed(() => this.comandas().reduce((total, c) => total + c.subtotal, 0));

  constructor() {
    this.carregar();
  }

  protected numeroDaMesa(mesaId: number): string {
    return this.mesas().find((m) => m.id === mesaId)?.numero ?? String(mesaId);
  }

  protected linhasDaPraca(praca: Praca): LinhaDoSalao[] {
    const agora = Date.now();
    return this.comandas()
      .filter((c) => this.mesas().find((m) => m.id === c.mesaId)?.pracaId === praca.id)
      .map((comanda) => ({
        comanda,
        mesa: this.numeroDaMesa(comanda.mesaId),
        pendentes: comanda.itens.filter((i) => i.status === 'Pendente').length,
        minutos: minutosDesde(comanda.dataHoraAbertura, agora),
        atrasados: comanda.itens.filter((i) => i.status === 'Pendente' && atrasado(i.dataHoraRegistro, agora)).length,
      }))
      .sort((a, b) => a.mesa.localeCompare(b.mesa));
  }

  protected abrirComanda(comanda: Comanda): void {
    this.comandaAberta.set(comanda);
    this.fecharFormularios();
  }

  protected fecharComanda(): void {
    this.comandaAberta.set(null);
    this.fecharFormularios();
  }

  protected fecharFormularios(): void {
    this.lancandoCortesia.set(false);
    this.lancandoItem.set(false);
    this.itemEmAjuste.set(null);
    this.errosDaFolha.set([]);
  }

  protected abrirLancamento(): void {
    this.abrirCortesia();
    this.lancandoCortesia.set(false);
    this.lancandoItem.set(true);
  }

  protected confirmarLancamento(): void {
    const comanda = this.comandaAberta();
    const item = this.itemEscolhido();
    if (!comanda || !item || this.salvando()) {
      return;
    }
    this.salvando.set(true);
    this.api
      .registrarItem(comanda.id, item.id, this.quantidade())
      .subscribe({ next: () => this.recarregarComanda(comanda.id), error: (erro: unknown) => this.falhou(erro) });
  }

  protected abrirCortesia(): void {
    this.fecharFormularios();
    this.itemEscolhido.set(null);
    this.quantidade.set(1);
    this.cortesiaMotivo = '';
    this.lancandoCortesia.set(true);
    if (!this.cardapio().length) {
      // Os destaques só colorem a lista: sem eles, o lançamento continua.
      forkJoin({
        itens: this.api.listarCardapio(),
        destaques: this.api.listarDestaques().pipe(catchError(() => of([] as Destaque[]))),
      }).subscribe({
        next: ({ itens, destaques }) => {
          this.cardapio.set(itens);
          this.destaques.set(destaques);
        },
        error: (erro: unknown) => this.errosDaFolha.set(mensagensDeErro(erro)),
      });
    }
  }

  protected confirmarCortesia(): void {
    const comanda = this.comandaAberta();
    const item = this.itemEscolhido();
    if (!comanda || !item || this.salvando()) {
      return;
    }
    this.salvando.set(true);
    this.api
      .lancarCortesia(comanda.id, item.id, this.quantidade(), this.cortesiaMotivo.trim())
      .subscribe({ next: () => this.recarregarComanda(comanda.id), error: (erro: unknown) => this.falhou(erro) });
  }

  protected abrirAjuste(item: ItemDoPedido): void {
    this.fecharFormularios();
    this.novoPreco = item.precoUnitarioNoMomento;
    this.motivoDoAjuste = '';
    this.itemEmAjuste.set(item);
  }

  protected confirmarAjuste(): void {
    const comanda = this.comandaAberta();
    const item = this.itemEmAjuste();
    if (!comanda || !item || this.novoPreco === null || this.salvando()) {
      return;
    }
    this.salvando.set(true);
    this.api
      .alterarPrecoDoItem(comanda.id, item.id, Number(this.novoPreco), this.motivoDoAjuste.trim())
      .subscribe({ next: () => this.recarregarComanda(comanda.id), error: (erro: unknown) => this.falhou(erro) });
  }

  private recarregarComanda(id: number): void {
    this.api.obterComanda(id).subscribe({
      next: (atualizada) => {
        this.comandas.update((todas) => todas.map((c) => (c.id === id ? atualizada : c)));
        this.comandaAberta.set(atualizada);
        this.salvando.set(false);
        this.fecharFormularios();
      },
      error: (erro: unknown) => this.falhou(erro),
    });
  }

  private falhou(erro: unknown): void {
    this.salvando.set(false);
    this.errosDaFolha.set(mensagensDeErro(erro));
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erros.set([]);
    forkJoin({
      pracas: this.api.listarPracas(),
      mesas: this.api.listarMesas(),
      comandas: this.api.listarComandasAbertas(),
    }).subscribe({
      next: ({ pracas, mesas, comandas }) => {
        this.pracas.set(pracas);
        this.mesas.set(mesas);
        this.comandas.set(comandas);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.erros.set(mensagensDeErro(erro));
        this.carregando.set(false);
      },
    });
  }
}
