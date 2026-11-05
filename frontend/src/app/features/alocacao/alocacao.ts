import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { catchError, forkJoin, of } from 'rxjs';
import { GastraApiService } from '../../core/api/gastra-api.service';
import {
  AlocacaoDoTurno,
  DesignacaoDoTurno,
  FaixaDeFaturamento,
  GarcomDoTurno,
  PeriodoAlocacao,
  Praca,
} from '../../core/api/modelos';
import { AvisoService } from '../../shared/aviso/aviso.service';
import { mensagensDeErro } from '../../shared/erros';
import { Folha } from '../../shared/folha/folha';
import { Icone } from '../../shared/icone/icone';
import { dataDeHoje, periodoDoTurno } from '../comandas/turno';

type Etapa = 'presenca' | 'sugestao' | 'confirmada';

/**
 * UC15, UC21 e UC22 — alocação dos garçons pelas praças no turno.
 *
 * A tela não calcula a distribuição: ela mostra o que a RN03 sugeriu e deixa o Metre ajustar antes de confirmar
 * (RF07). Números de faturamento não aparecem aqui — são do Gerente (RNF04).
 */
@Component({
  selector: 'app-alocacao',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Folha, Icone],
  templateUrl: './alocacao.html',
  styleUrl: './alocacao.scss',
})
export class Alocacao {
  private readonly api = inject(GastraApiService);
  private readonly avisos = inject(AvisoService);

  protected readonly carregando = signal(true);
  protected readonly salvando = signal(false);
  protected readonly erros = signal<string[]>([]);

  protected readonly data = signal(dataDeHoje());
  protected readonly periodo = signal<PeriodoAlocacao>(periodoDoTurno());

  protected readonly pracas = signal<Praca[]>([]);
  protected readonly garcons = signal<GarcomDoTurno[]>([]);
  protected readonly presentes = signal<number[]>([]);
  protected readonly turno = signal<AlocacaoDoTurno | null>(null);
  /** Sugestão que veio da RN03, para marcar o que o Metre mudou depois. */
  protected readonly sugerida = signal<Record<number, number>>({});
  protected readonly servicoFalhou = signal(false);

  protected readonly garcomParaMudar = signal<GarcomDoTurno | null>(null);
  protected readonly pracaDeDestino = signal<number | null>(null);
  protected readonly trocarCom = signal<number | null>(null);
  protected readonly confirmando = signal(false);

  protected readonly vagasTotais = computed(() => this.pracas().reduce((total, p) => total + p.quantidadeGarcons, 0));
  protected readonly excessoDePresentes = computed(() => this.presentes().length > this.vagasTotais());

  protected readonly etapa = computed<Etapa>(() => {
    const turno = this.turno();
    if (turno?.confirmada) return 'confirmada';
    return turno && turno.designacoes.length > 0 ? 'sugestao' : this.servicoFalhou() ? 'sugestao' : 'presenca';
  });

  protected readonly designados = computed(() => this.turno()?.designacoes ?? []);

  protected readonly semPraca = computed(() => {
    const comPraca = new Set(this.designados().map((d) => d.garcomId));
    return this.garcons().filter((g) => this.presentes().includes(g.id) && !comPraca.has(g.id));
  });

  protected readonly ajustes = computed(
    () => this.designados().filter((d) => this.sugerida()[d.garcomId] && this.sugerida()[d.garcomId] !== d.pracaId).length,
  );

  constructor() {
    this.carregar();
  }

  protected nomeDaPraca(pracaId: number): string {
    return this.pracas().find((p) => p.id === pracaId)?.codigo ?? String(pracaId);
  }

  protected garconsDaPraca(
    praca: Praca,
  ): { id: number; nome: string; ajustado: boolean; sugerida: number | null; motivo: string | null }[] {
    return this.designados()
      .filter((d) => d.pracaId === praca.id)
      .map((d) => ({
        id: d.garcomId,
        nome: d.garcomNome,
        ajustado: !!this.sugerida()[d.garcomId] && this.sugerida()[d.garcomId] !== d.pracaId,
        sugerida: this.sugerida()[d.garcomId] ?? null,
        motivo: motivoDaSugestao(d),
      }));
  }

  protected ehDeAltoMovimento(pracaId: number): boolean {
    return this.turno()?.pracasDeAltoPotencial?.includes(pracaId) ?? false;
  }

  protected ocupadas(pracaId: number): number {
    return this.designados().filter((d) => d.pracaId === pracaId).length;
  }

  protected alternarPresenca(id: number): void {
    this.presentes.update((atuais) => (atuais.includes(id) ? atuais.filter((x) => x !== id) : [...atuais, id]));
  }

  protected marcarTodos(): void {
    this.presentes.set(this.garcons().map((g) => g.id));
  }

  protected mudarTurno(data: string, periodo: PeriodoAlocacao): void {
    this.data.set(data);
    this.periodo.set(periodo);
    this.carregar();
  }

  /** UC15 — pede a sugestão da RN03 para quem está presente. */
  protected gerarSugestao(): void {
    if (this.excessoDePresentes() || !this.presentes().length || this.salvando()) {
      return;
    }
    this.salvando.set(true);
    this.erros.set([]);
    this.api.gerarSugestao(this.data(), this.periodo(), this.presentes()).subscribe({
      next: (turno) => {
        this.salvando.set(false);
        this.turno.set(turno);
        // D3: sem o serviço de análise a lista vem vazia; o Metre distribui à mão, e o resto segue.
        this.servicoFalhou.set(turno.servicoDisponivel === false);
        this.sugerida.set(Object.fromEntries(turno.designacoes.map((d) => [d.garcomId, d.pracaId])));
      },
      error: (erro: unknown) => {
        this.salvando.set(false);
        this.erros.set(mensagensDeErro(erro));
      },
    });
  }

  protected voltarParaPresenca(): void {
    this.turno.set(null);
    this.servicoFalhou.set(false);
    this.erros.set([]);
  }

  protected abrirMudanca(garcom: { id: number; nome: string }): void {
    this.garcomParaMudar.set({ id: garcom.id, nome: garcom.nome });
    this.pracaDeDestino.set(null);
    this.trocarCom.set(null);
  }

  protected pracaAtualDe(garcomId: number): number | null {
    return this.designados().find((d) => d.garcomId === garcomId)?.pracaId ?? null;
  }

  protected ocupantesDe(pracaId: number, exceto: number): { id: number; nome: string }[] {
    return this.designados()
      .filter((d) => d.pracaId === pracaId && d.garcomId !== exceto)
      .map((d) => ({ id: d.garcomId, nome: d.garcomNome }));
  }

  /** Texto da etiqueta de vagas da praça no painel de mudança. */
  protected vagasDaPraca(praca: Praca, exceto: number): string {
    const livres = praca.quantidadeGarcons - this.ocupantesDe(praca.id, exceto).length;
    if (livres <= 0) return 'Cheia';
    return livres === 1 ? '1 vaga' : `${livres} vagas`;
  }

  protected destinoCheio(): boolean {
    const destino = this.pracaDeDestino();
    const garcom = this.garcomParaMudar();
    if (!destino || !garcom) {
      return false;
    }
    const praca = this.pracas().find((p) => p.id === destino)!;
    return this.ocupantesDe(destino, garcom.id).length >= praca.quantidadeGarcons;
  }

  protected podeSalvarMudanca(): boolean {
    return !!this.pracaDeDestino() && (!this.destinoCheio() || !!this.trocarCom()) && !this.salvando();
  }

  /** UC22 — move o garçom ou troca com alguém da praça cheia (#140). */
  protected salvarMudanca(): void {
    const garcom = this.garcomParaMudar();
    const destino = this.pracaDeDestino();
    if (!garcom || !destino || !this.podeSalvarMudanca()) {
      return;
    }
    const troca = this.destinoCheio() ? this.trocarCom() ?? undefined : undefined;
    this.salvando.set(true);
    this.erros.set([]);
    this.api.ajustarAlocacao(this.data(), this.periodo(), garcom.id, destino, troca).subscribe({
      next: (turno) => {
        this.salvando.set(false);
        this.turno.set(turno);
        this.garcomParaMudar.set(null);
        const outro = troca ? this.garcons().find((g) => g.id === troca)?.nome.split(' ')[0] : null;
        this.avisos.mostrar(
          outro
            ? `${garcom.nome.split(' ')[0]} e ${outro} trocaram de praça`
            : `${garcom.nome.split(' ')[0]} vai para a praça ${this.nomeDaPraca(destino)}`,
        );
      },
      error: (erro: unknown) => {
        this.salvando.set(false);
        this.erros.set(mensagensDeErro(erro));
      },
    });
  }

  /** UC21 — confirma o turno. Depois disso a alocação não aceita mais ajuste. */
  protected confirmar(): void {
    if (this.salvando()) {
      return;
    }
    this.salvando.set(true);
    this.api.confirmarAlocacao(this.data(), this.periodo()).subscribe({
      next: (turno) => {
        this.salvando.set(false);
        this.confirmando.set(false);
        this.turno.set(turno);
        this.avisos.mostrar('Alocação confirmada');
      },
      error: (erro: unknown) => {
        this.salvando.set(false);
        this.confirmando.set(false);
        this.erros.set(mensagensDeErro(erro));
      },
    });
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erros.set([]);
    this.servicoFalhou.set(false);
    forkJoin({
      pracas: this.api.listarPracas(),
      garcons: this.api.listarGarconsDoTurno(),
      // Turno ainda não montado: a API responde 404, e a tela começa pela presença.
      turno: this.api
        .obterAlocacao(this.data(), this.periodo())
        .pipe(catchError(() => of(null as AlocacaoDoTurno | null))),
    }).subscribe({
      next: ({ pracas, garcons, turno }) => {
        this.pracas.set(pracas);
        this.garcons.set(garcons);
        this.turno.set(turno);
        this.sugerida.set(turno ? Object.fromEntries(turno.designacoes.map((d) => [d.garcomId, d.pracaId])) : {});
        this.presentes.set(turno?.designacoes.length ? turno.designacoes.map((d) => d.garcomId) : garcons.map((g) => g.id));
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.erros.set(mensagensDeErro(erro));
        this.carregando.set(false);
      },
    });
  }
}

const FAIXAS: Record<FaixaDeFaturamento, string> = {
  SemHistorico: 'Sem vendas nos últimos 30 dias',
  AbaixoDaEquipe: 'Vendas abaixo da equipe',
  NaMediaDaEquipe: 'Vendas na média da equipe',
  AcimaDaEquipe: 'Vendas acima da equipe',
};

/**
 * Os dois fatores da RN03 em palavras: a faixa de vendas (nunca o valor, que é indicador de desempenho e o Metre não
 * consulta) e a espera por uma praça de alto movimento. Sem os fatores na resposta, não há o que explicar.
 */
export function motivoDaSugestao(designacao: DesignacaoDoTurno): string | null {
  if (!designacao.faixaDeFaturamento) {
    return null;
  }
  const partes = [FAIXAS[designacao.faixaDeFaturamento]];
  const turnos = designacao.turnosDesdePracaDeAltoPotencial ?? 0;
  if (turnos > 0) {
    partes.push(`${turnos} ${turnos === 1 ? 'turno' : 'turnos'} sem praça de alto movimento`);
  }
  return partes.join(' · ');
}
