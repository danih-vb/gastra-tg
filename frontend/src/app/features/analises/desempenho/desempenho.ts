import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { GastraApiService } from '../../../core/api/gastra-api.service';
import { RankingDeDesempenho } from '../../../core/api/modelos';
import { SessaoService } from '../../../core/sessao/sessao.service';
import { mensagensDeErro } from '../../../shared/erros';
import { Icone } from '../../../shared/icone/icone';

/**
 * UC17 — índice de desempenho (RF11). Para o Garçom, a API já devolve só a própria posição: a tela não tem como
 * mostrar o número dos colegas, nem por engano.
 */
@Component({
  selector: 'app-desempenho',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, DatePipe, DecimalPipe, Icone],
  templateUrl: './desempenho.html',
  styleUrl: './desempenho.scss',
})
export class Desempenho {
  private readonly api = inject(GastraApiService);
  private readonly sessao = inject(SessaoService);

  protected readonly carregando = signal(true);
  protected readonly erros = signal<string[]>([]);
  protected readonly ranking = signal<RankingDeDesempenho | null>(null);

  protected readonly minhaPosicao = computed(() => {
    const meuId = this.sessao.usuario()?.id;
    return this.ranking()?.posicoes.find((p) => p.garcomId === meuId) ?? null;
  });

  constructor() {
    this.carregar();
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erros.set([]);
    this.api.obterDesempenho().subscribe({
      next: (ranking) => {
        this.ranking.set(ranking);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.erros.set(mensagensDeErro(erro));
        this.carregando.set(false);
      },
    });
  }
}
