import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { GastraApiService } from '../../../core/api/gastra-api.service';
import {
  RankingDeDesempenho,
  RelatorioCardapio,
  RelatorioGarcons,
  RelatorioHorarios,
  RelatorioPracas,
} from '../../../core/api/modelos';
import { mensagensDeErro } from '../../../shared/erros';
import { Icone } from '../../../shared/icone/icone';
import { CATEGORIAS } from '../../../shared/rotulos';
import { dataDeHoje } from '../../comandas/turno';

export type PeriodoDaAnalise = '7' | '30' | 'mes';

/** Primeiro e último dia do período escolhido, no formato da API. O dia de hoje entra. */
export function datasDoPeriodo(periodo: PeriodoDaAnalise, hoje = new Date()): { inicio: string; fim: string } {
  const fim = dataDeHoje(hoje);
  if (periodo === 'mes') {
    return { inicio: `${fim.slice(0, 8)}01`, fim };
  }
  const inicio = new Date(hoje);
  inicio.setDate(inicio.getDate() - (Number(periodo) - 1));
  return { inicio: dataDeHoje(inicio), fim };
}

interface Relatorios {
  garcons: RelatorioGarcons;
  pracas: RelatorioPracas;
  cardapio: RelatorioCardapio;
  horarios: RelatorioHorarios;
  ranking: RankingDeDesempenho;
}

/**
 * UC16 e UC17 — relatórios de BI para o Gerente (RF10, RF11). Os números vêm das views do banco pela API; a tela só
 * organiza e desenha. Os gráficos são HTML e CSS, sem biblioteca: são poucos, simples e ficam acessíveis como tabela.
 */
@Component({
  selector: 'app-analises',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, DatePipe, DecimalPipe, Icone],
  templateUrl: './analises.html',
  styleUrl: './analises.scss',
})
export class Analises {
  private readonly api = inject(GastraApiService);

  protected readonly periodo = signal<PeriodoDaAnalise>('30');
  protected readonly carregando = signal(true);
  protected readonly erros = signal<string[]>([]);
  protected readonly dados = signal<Relatorios | null>(null);
  protected readonly explicandoIndice = signal(false);

  protected readonly periodos: { chave: PeriodoDaAnalise; nome: string }[] = [
    { chave: '7', nome: 'Últimos 7 dias' },
    { chave: '30', nome: 'Últimos 30 dias' },
    { chave: 'mes', nome: 'Este mês' },
  ];

  protected readonly semMovimento = computed(() => (this.dados()?.garcons.totais.comandas ?? 0) === 0);

  protected readonly pracas = computed(() => {
    const pracas = this.dados()?.pracas.pracas ?? [];
    const maior = Math.max(1, ...pracas.map((p) => p.faturamentoPorTurno));
    const comMovimento = pracas.filter((p) => p.turnosComMovimento > 0);
    const media = comMovimento.reduce((total, p) => total + p.faturamentoPorTurno, 0) / (comMovimento.length || 1);
    return pracas.map((p) => ({
      ...p,
      largura: (p.faturamentoPorTurno / maior) * 100,
      // Mesma regra da RN03: acima da média das praças com movimento é alto potencial.
      altoPotencial: p.turnosComMovimento > 0 && p.faturamentoPorTurno > media,
    }));
  });

  /** Mapa de calor: uma linha por praça, uma coluna por hora com movimento no período. */
  protected readonly mapaDeCalor = computed(() => {
    const porHora = this.dados()?.horarios.porHora ?? [];
    const horas = [...new Set(porHora.map((l) => l.hora))].sort((a, b) => a - b);
    const codigos = [...new Set(porHora.map((l) => l.pracaCodigo))].sort();
    const maior = Math.max(1, ...porHora.map((l) => l.faturamento));
    const linhas = codigos.map((codigo) => ({
      codigo,
      celulas: horas.map((hora) => {
        const valor = porHora.find((l) => l.pracaCodigo === codigo && l.hora === hora)?.faturamento ?? 0;
        return { hora, valor, intensidade: valor / maior };
      }),
    }));
    const pico = porHora.reduce<(typeof porHora)[number] | null>((maiorAte, l) => (!maiorAte || l.faturamento > maiorAte.faturamento ? l : maiorAte), null);
    return { horas, linhas, pico };
  });

  protected readonly ranking = computed(() => {
    const dados = this.dados();
    if (!dados) return [];
    return dados.ranking.posicoes.map((posicao) => ({
      ...posicao,
      detalhe: dados.garcons.garcons.find((g) => g.garcomId === posicao.garcomId) ?? null,
    }));
  });

  protected readonly itensMaisVendidos = computed(() =>
    [...(this.dados()?.cardapio.itens ?? [])].sort((a, b) => b.faturamento - a.faturamento).slice(0, 10),
  );

  constructor() {
    this.carregar();
  }

  protected nomeDaCategoria(chave: string): string {
    return CATEGORIAS.find((c) => c.chave === chave)?.nome ?? chave;
  }

  protected escolherPeriodo(periodo: PeriodoDaAnalise): void {
    this.periodo.set(periodo);
    this.carregar();
  }

  protected carregar(): void {
    const { inicio, fim } = datasDoPeriodo(this.periodo());
    this.carregando.set(true);
    this.erros.set([]);
    forkJoin({
      garcons: this.api.relatorioDeGarcons(inicio, fim),
      pracas: this.api.relatorioDePracas(inicio, fim),
      cardapio: this.api.relatorioDoCardapio(inicio, fim),
      horarios: this.api.relatorioDeHorarios(inicio, fim),
      ranking: this.api.rankingDeDesempenho(inicio, fim),
    }).subscribe({
      next: (dados) => {
        this.dados.set(dados);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.erros.set(mensagensDeErro(erro));
        this.carregando.set(false);
      },
    });
  }
}
