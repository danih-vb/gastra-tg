import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { GastraApiService } from '../../../core/api/gastra-api.service';
import {
  CategoriaItemCardapio,
  CategoriaRestricao,
  Comanda as ComandaDaApi,
  ItemCardapio,
  ItemDoPedido,
  Mesa,
  MotivoCancelamento,
  SugestoesDaComanda,
} from '../../../core/api/modelos';
import { AvisoService } from '../../../shared/aviso/aviso.service';
import { mensagensDeErro } from '../../../shared/erros';
import { Folha } from '../../../shared/folha/folha';
import { Icone } from '../../../shared/icone/icone';
import { CATEGORIAS, COMPOSICOES, FLAGS, MOTIVOS, RESTRICOES, composicaoSugerida, tempoRelativo } from '../../../shared/rotulos';

/** UC12, UC13, UC18 e UC23 — a tela em que o garçom passa o atendimento inteiro. */
@Component({
  selector: 'app-comanda',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, FormsModule, RouterLink, Folha, Icone],
  templateUrl: './comanda.html',
  styleUrl: './comanda.scss',
})
export class Comanda implements OnInit {
  private readonly api = inject(GastraApiService);
  private readonly avisos = inject(AvisoService);
  private readonly router = inject(Router);

  /** Id vindo da rota /comandas/:id (withComponentInputBinding). */
  readonly id = input.required<string>();

  protected readonly carregando = signal(true);
  protected readonly salvando = signal(false);
  protected readonly erros = signal<string[]>([]);
  protected readonly comanda = signal<ComandaDaApi | null>(null);
  protected readonly cardapio = signal<ItemCardapio[]>([]);
  private readonly mesas = signal<Mesa[]>([]);
  protected readonly sugestoes = signal<SugestoesDaComanda | null>(null);
  protected readonly categoria = signal<CategoriaItemCardapio>('Entrada');

  // Painéis abertos
  protected readonly itemParaLancar = signal<ItemCardapio | null>(null);
  protected readonly quantidade = signal(1);
  protected readonly itemParaCancelar = signal<ItemDoPedido | null>(null);
  protected readonly motivo = signal<MotivoCancelamento | null>(null);
  protected readonly registrandoRestricao = signal(false);
  protected readonly categoriaDaRestricao = signal<CategoriaRestricao | null>(null);
  protected observacaoDaRestricao = '';
  protected readonly ajustandoComposicao = signal(false);
  protected readonly pessoasNoAjuste = signal(1);

  protected readonly CATEGORIAS = CATEGORIAS;
  protected readonly COMPOSICOES = COMPOSICOES;
  protected readonly FLAGS = FLAGS;
  protected readonly MOTIVOS = MOTIVOS;
  protected readonly RESTRICOES = RESTRICOES;
  protected readonly motivosPossiveis = Object.keys(MOTIVOS) as MotivoCancelamento[];
  protected readonly restricoesPossiveis = Object.keys(RESTRICOES) as CategoriaRestricao[];
  protected readonly composicoesPossiveis = Object.keys(COMPOSICOES) as (keyof typeof COMPOSICOES)[];

  /** A comanda traz o id da mesa; o número que a equipe usa vem da lista de mesas. */
  protected readonly mesa = computed(() => this.mesas().find((m) => m.id === this.comanda()?.mesaId) ?? null);

  protected readonly pendentes = computed(() => this.comanda()?.itens.filter((i) => i.status === 'Pendente') ?? []);
  protected readonly resolvidos = computed(() => this.comanda()?.itens.filter((i) => i.status !== 'Pendente') ?? []);
  protected readonly itensDaCategoria = computed(() => this.cardapio().filter((i) => i.categoria === this.categoria()));
  protected readonly composicaoDoAjuste = computed(() => composicaoSugerida(this.pessoasNoAjuste()));

  // O id da rota só existe depois que o Angular preenche as entradas, e não dentro do construtor.
  ngOnInit(): void {
    this.carregar();
  }

  protected tempo(dataIso: string): string {
    return tempoRelativo(dataIso);
  }

  protected precoDe(item: ItemCardapio): number {
    return item.precoPromocional ?? item.preco;
  }

  protected totalDoLancamento(): number {
    const item = this.itemParaLancar();
    return item ? this.precoDe(item) * this.quantidade() : 0;
  }

  protected abrirLancamento(item: ItemCardapio | undefined): void {
    if (!item || !item.disponivel) {
      return;
    }
    this.quantidade.set(1);
    this.itemParaLancar.set(item);
  }

  /** UC12 — lança o item. O "Desfazer" cancela com o motivo "erro de lançamento", que é o caminho que a API tem. */
  protected lancar(): void {
    const comanda = this.comanda();
    const item = this.itemParaLancar();
    if (!comanda || !item || this.salvando()) {
      return;
    }
    const quantidade = this.quantidade();
    this.executar(this.api.registrarItem(comanda.id, item.id, quantidade), (lancado: ItemDoPedido) => {
      this.itemParaLancar.set(null);
      this.avisos.mostrar(`${quantidade} × ${item.nome} lançado`, {
        rotulo: 'Desfazer',
        executar: () => this.cancelar(lancado, 'ErroDeLancamento'),
      });
    });
  }

  /** UC23 — entregar não tem volta na API, por isso o aviso não oferece "Desfazer". */
  protected entregar(item: ItemDoPedido): void {
    const comanda = this.comanda();
    if (!comanda || this.salvando()) {
      return;
    }
    this.executar(this.api.atualizarSituacaoDoItem(comanda.id, item.id, 'Entregue'), () =>
      this.avisos.mostrar(`${item.nome}: entregue`),
    );
  }

  protected confirmarCancelamento(): void {
    const item = this.itemParaCancelar();
    const motivo = this.motivo();
    if (item && motivo) {
      this.cancelar(item, motivo);
    }
  }

  private cancelar(item: ItemDoPedido, motivo: MotivoCancelamento): void {
    const comanda = this.comanda();
    if (!comanda) {
      return;
    }
    this.executar(this.api.atualizarSituacaoDoItem(comanda.id, item.id, 'Cancelado', motivo), () => {
      this.itemParaCancelar.set(null);
      this.motivo.set(null);
      this.avisos.mostrar(`${item.nome}: cancelado`);
    });
  }

  /** UC13 — RF14: categoria da lista fechada e um detalhe opcional, apagado no fechamento. */
  protected registrarRestricao(): void {
    const comanda = this.comanda();
    const categoria = this.categoriaDaRestricao();
    if (!comanda || !categoria || this.salvando()) {
      return;
    }
    const observacao = this.observacaoDaRestricao.trim();
    this.executar(this.api.registrarRestricao(comanda.id, categoria, observacao || null), () => {
      this.registrandoRestricao.set(false);
      this.categoriaDaRestricao.set(null);
      this.observacaoDaRestricao = '';
      this.avisos.mostrar(`Restrição registrada: ${RESTRICOES[categoria]}`);
    });
  }

  protected abrirAjusteDeComposicao(): void {
    this.pessoasNoAjuste.set(this.comanda()?.quantidadePessoas ?? 1);
    this.ajustandoComposicao.set(true);
  }

  /** UC11 — confirmar ou ajustar a composição sugerida. */
  protected salvarComposicao(composicao: keyof typeof COMPOSICOES): void {
    const comanda = this.comanda();
    if (!comanda || this.salvando()) {
      return;
    }
    this.executar(this.api.ajustarComposicao(comanda.id, this.pessoasNoAjuste(), composicao), () => {
      this.ajustandoComposicao.set(false);
      this.avisos.mostrar(`Composição: ${COMPOSICOES[composicao]}`);
    });
  }

  protected irParaFechamento(): void {
    void this.router.navigate(['/comandas', this.id(), 'fechar']);
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erros.set([]);
    const id = Number(this.id());
    forkJoin({
      comanda: this.api.obterComanda(id),
      cardapio: this.api.listarCardapio(),
      mesas: this.api.listarMesas(),
      // D3: sem o serviço de análise, o atendimento continua; só as sugestões somem.
      sugestoes: this.api
        .obterSugestoes(id)
        .pipe(catchError(() => of<SugestoesDaComanda>({ servicoDisponivel: false, confirmarRestricaoComCliente: false, itens: [] }))),
    }).subscribe({
      next: ({ comanda, cardapio, mesas, sugestoes }) => {
        this.comanda.set(comanda);
        this.cardapio.set(cardapio);
        this.mesas.set(mesas);
        this.sugestoes.set(sugestoes);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.erros.set(mensagensDeErro(erro));
        this.carregando.set(false);
      },
    });
  }

  /** Executa uma ação e recarrega a comanda: quem manda no estado é a resposta da API, não a tela. */
  private executar<T>(chamada: import('rxjs').Observable<T>, aoTerminar: (resultado: T) => void): void {
    this.salvando.set(true);
    this.erros.set([]);
    chamada.subscribe({
      next: (resultado) => {
        this.salvando.set(false);
        aoTerminar(resultado);
        this.recarregarComanda();
      },
      error: (erro: unknown) => {
        this.salvando.set(false);
        this.erros.set(mensagensDeErro(erro));
      },
    });
  }

  private recarregarComanda(): void {
    const id = Number(this.id());
    this.api.obterComanda(id).subscribe({ next: (comanda) => this.comanda.set(comanda) });
    this.api
      .obterSugestoes(id)
      .pipe(catchError(() => of<SugestoesDaComanda>({ servicoDisponivel: false, confirmarRestricaoComCliente: false, itens: [] })))
      .subscribe({ next: (sugestoes) => this.sugestoes.set(sugestoes) });
  }
}
