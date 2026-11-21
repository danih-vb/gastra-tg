import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { GastraApiService } from '../../core/api/gastra-api.service';
import { Comanda, ItemCardapio, ItemDoPedido, Mesa, Praca } from '../../core/api/modelos';
import { SessaoService } from '../../core/sessao/sessao.service';
import { mensagensDeErro } from '../../shared/erros';
import { Folha } from '../../shared/folha/folha';
import { Icone } from '../../shared/icone/icone';
import { COMPOSICOES, MOTIVOS, RESTRICOES, atrasado, minutosDesde } from '../../shared/rotulos';

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
  imports: [CurrencyPipe, FormsModule, Folha, Icone],
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
  protected readonly lancandoCortesia = signal(false);
  protected readonly itemEmAjuste = signal<ItemDoPedido | null>(null);
  protected readonly salvando = signal(false);
  protected readonly errosDaFolha = signal<string[]>([]);
  protected cortesiaItemId: number | null = null;
  protected cortesiaQuantidade = 1;
  protected cortesiaMotivo = '';
  protected novoPreco: number | null = null;
  protected motivoDoAjuste = '';

  /** Ordem alfabética: na cortesia o Metre procura pelo nome, inclusive itens que não estão à venda. */
  protected readonly itensParaCortesia = computed(() =>
    [...this.cardapio()].sort((a, b) => a.nome.localeCompare(b.nome, 'pt-BR')),
  );

  protected readonly COMPOSICOES = COMPOSICOES;
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
    this.itemEmAjuste.set(null);
    this.errosDaFolha.set([]);
  }

  protected abrirCortesia(): void {
    this.fecharFormularios();
    this.cortesiaItemId = null;
    this.cortesiaQuantidade = 1;
    this.cortesiaMotivo = '';
    this.lancandoCortesia.set(true);
    if (!this.cardapio().length) {
      this.api.listarCardapio().subscribe({
        next: (itens) => this.cardapio.set(itens),
        error: (erro: unknown) => this.errosDaFolha.set(mensagensDeErro(erro)),
      });
    }
  }

  protected confirmarCortesia(): void {
    const comanda = this.comandaAberta();
    if (!comanda || this.cortesiaItemId === null || this.salvando()) {
      return;
    }
    this.salvando.set(true);
    this.api
      .lancarCortesia(comanda.id, Number(this.cortesiaItemId), this.cortesiaQuantidade, this.cortesiaMotivo.trim())
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
