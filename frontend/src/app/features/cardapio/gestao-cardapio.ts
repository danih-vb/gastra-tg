import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable, forkJoin } from 'rxjs';
import { GastraApiService } from '../../core/api/gastra-api.service';
import { CategoriaItemCardapio, FlagDietetica, ItemCardapio, Promocao, TipoDesconto } from '../../core/api/modelos';
import { AvisoService } from '../../shared/aviso/aviso.service';
import { mensagensDeErro } from '../../shared/erros';
import { Folha } from '../../shared/folha/folha';
import { Icone } from '../../shared/icone/icone';
import { CATEGORIAS, FLAGS } from '../../shared/rotulos';
import { dataDeHoje } from '../comandas/turno';

type Aba = 'itens' | 'promocoes';
type Painel = 'novo-item' | 'preco' | 'foto' | 'nova-promocao' | 'remover-promocao' | null;

/** "12,50", "12.50" e "1.234,50" viram número; vazio vira NaN para a validação da API responder. */
export function lerValor(texto: string | number | null | undefined): number {
  const valor = String(texto ?? '').trim();
  if (!valor) return Number.NaN;
  return /^\d+\.\d{1,2}$/.test(valor) ? Number(valor) : Number(valor.replace(/\./g, '').replace(',', '.'));
}

/** Mesma conta do domínio (Promocao): desconto percentual ou fixo, e nunca abaixo de R$ 0,01. */
export function precoComDesconto(preco: number, tipo: TipoDesconto, valor: number): number {
  const resultado = tipo === 'Percentual' ? preco * (1 - valor / 100) : preco - valor;
  return Math.max(0.01, Math.round(resultado * 100) / 100);
}

/**
 * UC05–UC09 — cardápio e promoções, para Gerente e Coordenador (RF19–RF22). As validações ficam na API: a tela
 * mostra as mensagens que ela devolve, para as regras não existirem em dois lugares.
 */
@Component({
  selector: 'app-gestao-cardapio',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, DatePipe, FormsModule, Folha, Icone],
  templateUrl: './gestao-cardapio.html',
  styleUrl: './gestao-cardapio.scss',
})
export class GestaoCardapio {
  private readonly api = inject(GastraApiService);
  private readonly avisos = inject(AvisoService);

  protected readonly aba = signal<Aba>('itens');
  protected readonly carregando = signal(true);
  protected readonly salvando = signal(false);
  protected readonly erros = signal<string[]>([]);
  protected readonly errosDoPainel = signal<string[]>([]);
  protected readonly itens = signal<ItemCardapio[]>([]);
  protected readonly promocoes = signal<Promocao[]>([]);

  protected readonly busca = signal('');
  protected readonly categoria = signal<CategoriaItemCardapio | 'todas'>('todas');

  protected readonly painel = signal<Painel>(null);
  protected readonly itemEmEdicao = signal<ItemCardapio | null>(null);
  protected readonly promocaoParaRemover = signal<Promocao | null>(null);

  // Formulário de item
  protected novoItem = { nome: '', categoria: '' as CategoriaItemCardapio | '', preco: '', descricao: '', imagem: '' };
  protected readonly flagsDoNovoItem = signal<FlagDietetica[]>([]);
  protected novoPreco = '';
  protected novaFoto = '';

  // Formulário de promoção
  protected novaPromocao = { descricao: '', valor: '', inicio: dataDeHoje(), fim: '' };
  protected readonly tipoDesconto = signal<TipoDesconto>('Percentual');
  protected readonly valorDaPromocao = signal('');
  protected readonly itensDaPromocao = signal<number[]>([]);

  protected readonly CATEGORIAS = CATEGORIAS;
  protected readonly FLAGS = FLAGS;
  protected readonly flagsPossiveis = Object.keys(FLAGS) as FlagDietetica[];

  protected readonly itensFiltrados = computed(() => {
    const termo = this.busca().trim().toLowerCase();
    return this.itens().filter(
      (item) =>
        (this.categoria() === 'todas' || item.categoria === this.categoria()) &&
        (!termo || item.nome.toLowerCase().includes(termo)),
    );
  });

  /** Prévia dos preços da nova promoção, calculada como o domínio calcula. */
  protected readonly previa = computed(() => {
    const valor = lerValor(this.valorDaPromocao());
    if (!(valor > 0)) return [];
    return this.itens()
      .filter((item) => this.itensDaPromocao().includes(item.id))
      .map((item) => ({ nome: item.nome, de: item.preco, por: precoComDesconto(item.preco, this.tipoDesconto(), valor) }));
  });

  constructor() {
    this.carregar();
  }

  protected nomeDaCategoria(chave: CategoriaItemCardapio): string {
    return CATEGORIAS.find((c) => c.chave === chave)?.nome ?? chave;
  }

  protected situacaoDa(promocao: Promocao): { texto: string; classe: string } {
    if (promocao.vigenteHoje) return { texto: 'Vigente hoje', classe: 'sucesso' };
    return promocao.dataInicio > dataDeHoje() ? { texto: 'Agendada', classe: 'aviso' } : { texto: 'Encerrada', classe: '' };
  }

  // --- Painéis ---

  protected abrirNovoItem(): void {
    this.novoItem = { nome: '', categoria: '', preco: '', descricao: '', imagem: '' };
    this.flagsDoNovoItem.set([]);
    this.abrir('novo-item');
  }

  protected abrirPreco(item: ItemCardapio): void {
    this.itemEmEdicao.set(item);
    this.novoPreco = String(item.preco).replace('.', ',');
    this.abrir('preco');
  }

  protected abrirFoto(item: ItemCardapio): void {
    this.itemEmEdicao.set(item);
    this.novaFoto = item.imagem ?? '';
    this.abrir('foto');
  }

  protected abrirNovaPromocao(): void {
    this.novaPromocao = { descricao: '', valor: '', inicio: dataDeHoje(), fim: '' };
    this.tipoDesconto.set('Percentual');
    this.valorDaPromocao.set('');
    this.itensDaPromocao.set([]);
    this.abrir('nova-promocao');
  }

  protected abrirRemocao(promocao: Promocao): void {
    this.promocaoParaRemover.set(promocao);
    this.abrir('remover-promocao');
  }

  protected fecharPainel(): void {
    this.painel.set(null);
    this.errosDoPainel.set([]);
  }

  protected alternarFlag(flag: FlagDietetica): void {
    this.flagsDoNovoItem.update((atuais) => (atuais.includes(flag) ? atuais.filter((f) => f !== flag) : [...atuais, flag]));
  }

  protected alternarItemDaPromocao(id: number): void {
    this.itensDaPromocao.update((atuais) => (atuais.includes(id) ? atuais.filter((x) => x !== id) : [...atuais, id]));
  }

  // --- Ações ---

  /** UC05 — cadastrar item. Categoria vazia é a única checagem local: sem ela o formulário nem tem o que mandar. */
  protected cadastrarItem(): void {
    if (!this.novoItem.categoria) {
      this.errosDoPainel.set(['Escolha a categoria.']);
      return;
    }
    const item = {
      nome: this.novoItem.nome.trim(),
      categoria: this.novoItem.categoria,
      preco: lerValor(this.novoItem.preco) || 0,
      descricao: this.novoItem.descricao.trim(),
      flagsDieteticas: this.flagsDoNovoItem(),
      imagem: this.novoItem.imagem.trim() || null,
    };
    this.salvar(this.api.cadastrarItem(item), `${item.nome} cadastrado`);
  }

  /** UC06 — atualizar o preço. Comandas abertas mantêm o preço do momento do pedido. */
  protected salvarPreco(): void {
    const item = this.itemEmEdicao();
    if (!item) return;
    const anterior = item.preco;
    const novo = lerValor(this.novoPreco) || 0;
    this.salvar(this.api.atualizarPreco(item.id, novo), `${item.nome}: novo preço salvo`, () => ({
      rotulo: 'Desfazer',
      executar: () => this.salvar(this.api.atualizarPreco(item.id, anterior), `${item.nome}: preço anterior de volta`),
    }));
  }

  protected salvarFoto(): void {
    const item = this.itemEmEdicao();
    if (!item) return;
    this.salvar(this.api.alterarImagem(item.id, this.novaFoto.trim() || null), this.novaFoto.trim() ? 'Foto salva' : 'Foto removida');
  }

  /** UC07 — disponibilidade com "Desfazer": tirar um item do ar por engano precisa ter volta rápida. */
  protected alternarDisponibilidade(item: ItemCardapio): void {
    const disponivel = !item.disponivel;
    this.salvar(
      this.api.alterarDisponibilidade(item.id, disponivel),
      disponivel ? `${item.nome}: disponível` : `${item.nome}: indisponível (sai do cardápio digital e das comandas)`,
      () => ({ rotulo: 'Desfazer', executar: () => this.salvar(this.api.alterarDisponibilidade(item.id, !disponivel), `${item.nome}: como estava`) }),
    );
  }

  /** UC08 — criar promoção; datas e valores são validados pela API. */
  protected criarPromocao(): void {
    const promocao = {
      descricao: this.novaPromocao.descricao.trim(),
      tipoDesconto: this.tipoDesconto(),
      valorDesconto: lerValor(this.valorDaPromocao()) || 0,
      dataInicio: this.novaPromocao.inicio,
      dataFim: this.novaPromocao.fim || this.novaPromocao.inicio,
      itemCardapioIds: this.itensDaPromocao(),
    };
    this.salvar(this.api.criarPromocao(promocao), `Promoção “${promocao.descricao}” criada`);
  }

  /** UC09 — remover promoção. Ela sai do ar, mas fica no histórico. */
  protected removerPromocao(): void {
    const promocao = this.promocaoParaRemover();
    if (!promocao) return;
    this.salvar(this.api.removerPromocao(promocao.id), `Promoção “${promocao.descricao}” removida`);
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erros.set([]);
    forkJoin({ itens: this.api.listarCardapio(), promocoes: this.api.listarPromocoes() }).subscribe({
      next: ({ itens, promocoes }) => {
        this.itens.set(itens);
        this.promocoes.set(promocoes);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.erros.set(mensagensDeErro(erro));
        this.carregando.set(false);
      },
    });
  }

  private abrir(painel: Painel): void {
    this.errosDoPainel.set([]);
    this.painel.set(painel);
  }

  /** Envia, fecha o painel, avisa e recarrega. Erro da API fica dentro do painel, perto do que precisa corrigir. */
  private salvar<T>(
    chamada: Observable<T>,
    mensagem: string,
    desfazer?: () => { rotulo: string; executar: () => void },
  ): void {
    this.salvando.set(true);
    this.errosDoPainel.set([]);
    chamada.subscribe({
      next: () => {
        this.salvando.set(false);
        this.painel.set(null);
        this.avisos.mostrar(mensagem, desfazer?.());
        this.carregar();
      },
      error: (erro: unknown) => {
        this.salvando.set(false);
        if (this.painel()) {
          this.errosDoPainel.set(mensagensDeErro(erro));
        } else {
          this.erros.set(mensagensDeErro(erro));
        }
      },
    });
  }
}
