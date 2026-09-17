import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { GastraApiService } from '../../../core/api/gastra-api.service';
import { AlocacaoDoTurno, Comanda, Mesa, Praca } from '../../../core/api/modelos';
import { SessaoService } from '../../../core/sessao/sessao.service';
import { AvisoService } from '../../../shared/aviso/aviso.service';
import { mensagensDeErro } from '../../../shared/erros';
import { Folha } from '../../../shared/folha/folha';
import { Icone } from '../../../shared/icone/icone';
import { COMPOSICOES, composicaoSugerida, minutosDesde } from '../../../shared/rotulos';
import { dataDeHoje, periodoDoTurno } from '../turno';

interface MesaNaTela {
  mesa: Mesa;
  comanda: Comanda | null;
  minha: boolean;
  pendentes: number;
  minutos: number;
}

/** UC10 — ponto de partida do garçom: o salão com as mesas livres e as ocupadas. */
@Component({
  selector: 'app-mesas',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, Folha, Icone],
  templateUrl: './mesas.html',
  styleUrl: './mesas.scss',
})
export class Mesas {
  private readonly api = inject(GastraApiService);
  private readonly sessao = inject(SessaoService);
  private readonly avisos = inject(AvisoService);
  private readonly router = inject(Router);

  protected readonly carregando = signal(true);
  protected readonly erros = signal<string[]>([]);
  protected readonly abrindo = signal(false);
  protected readonly filtro = signal<'minha' | 'todas'>('todas');
  protected readonly mesaParaAbrir = signal<Mesa | null>(null);

  private readonly pracas = signal<Praca[]>([]);
  private readonly mesas = signal<Mesa[]>([]);
  private readonly comandas = signal<Comanda[]>([]);
  private readonly alocacao = signal<AlocacaoDoTurno | null>(null);

  protected readonly COMPOSICOES = COMPOSICOES;
  protected readonly pessoasPossiveis = [1, 2, 3, 4, 5, 6, 7, 8];
  protected readonly periodo = periodoDoTurno();

  protected readonly nome = computed(() => this.sessao.usuario()?.nome.split(' ')[0] ?? '');

  /** Praça em que o garçom está alocado no turno (RF07). Nula quando o metre ainda não confirmou a alocação. */
  protected readonly minhaPraca = computed(() => {
    const id = this.sessao.usuario()?.id;
    const designacao = this.alocacao()?.designacoes.find((d) => d.garcomId === id);
    return designacao ? this.pracas().find((p) => p.id === designacao.pracaId) ?? null : null;
  });

  protected readonly pracasVisiveis = computed(() => {
    const minha = this.minhaPraca();
    return this.filtro() === 'minha' && minha ? [minha] : this.pracas();
  });

  protected readonly minhasComandas = computed(() =>
    this.comandas().filter((c) => c.garcomId === this.sessao.usuario()?.id),
  );

  protected readonly pendentesDoTurno = computed(() =>
    this.minhasComandas().reduce((total, c) => total + c.itens.filter((i) => i.status === 'Pendente').length, 0),
  );

  protected readonly emConsumo = computed(() => this.minhasComandas().reduce((total, c) => total + c.subtotal, 0));

  constructor() {
    this.carregar();
  }

  protected mesasDaPraca(praca: Praca): MesaNaTela[] {
    const meuId = this.sessao.usuario()?.id;
    const agora = Date.now();
    return this.mesas()
      .filter((m) => m.pracaId === praca.id)
      .map((mesa) => {
        const comanda = this.comandas().find((c) => c.mesaId === mesa.id) ?? null;
        return {
          mesa,
          comanda,
          minha: comanda?.garcomId === meuId,
          pendentes: comanda?.itens.filter((i) => i.status === 'Pendente').length ?? 0,
          minutos: comanda ? minutosDesde(comanda.dataHoraAbertura, agora) : 0,
        };
      });
  }

  protected livresDaPraca(praca: Praca): number {
    return this.mesasDaPraca(praca).filter((m) => m.comanda === null).length;
  }

  protected composicaoDe(pessoas: number): string {
    return COMPOSICOES[composicaoSugerida(pessoas)];
  }

  protected selecionar(linha: MesaNaTela): void {
    if (linha.comanda === null) {
      this.mesaParaAbrir.set(linha.mesa);
    } else if (linha.minha) {
      void this.router.navigate(['/comandas', linha.comanda.id]);
    } else {
      this.avisos.mostrar(`Mesa ${linha.mesa.numero} está com ${linha.comanda.garcomNome.split(' ')[0]}`);
    }
  }

  /** UC10 + UC11 em dois toques (RNF02): tocar no número de pessoas confirma a composição sugerida. */
  protected abrir(pessoas: number): void {
    const mesa = this.mesaParaAbrir();
    if (!mesa || this.abrindo()) {
      return;
    }
    this.abrindo.set(true);
    this.api.abrirComanda(mesa.id, pessoas).subscribe({
      next: (comanda) => {
        this.abrindo.set(false);
        this.mesaParaAbrir.set(null);
        void this.router.navigate(['/comandas', comanda.id]);
        this.avisos.mostrar(`Mesa ${mesa.numero} aberta · ${COMPOSICOES[comanda.composicao]}`);
      },
      error: (erro: unknown) => {
        this.abrindo.set(false);
        this.mesaParaAbrir.set(null);
        this.erros.set(mensagensDeErro(erro));
      },
    });
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erros.set([]);
    forkJoin({
      pracas: this.api.listarPracas(),
      mesas: this.api.listarMesas(),
      comandas: this.api.listarComandasAbertas(),
      // Sem alocação confirmada para o turno a API responde 404: o salão continua utilizável, só sem "minha praça".
      alocacao: this.api
        .obterAlocacao(dataDeHoje(), this.periodo)
        .pipe(catchError(() => of(null as AlocacaoDoTurno | null))),
    }).subscribe({
      next: ({ pracas, mesas, comandas, alocacao }) => {
        this.pracas.set(pracas);
        this.mesas.set(mesas);
        this.comandas.set(comandas);
        this.alocacao.set(alocacao);
        this.filtro.set(this.minhaPraca() ? 'minha' : 'todas');
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.erros.set(mensagensDeErro(erro));
        this.carregando.set(false);
      },
    });
  }
}
