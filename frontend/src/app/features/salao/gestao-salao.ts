import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable, forkJoin } from 'rxjs';
import { GastraApiService } from '../../core/api/gastra-api.service';
import { Mesa, Praca } from '../../core/api/modelos';
import { AvisoService } from '../../shared/aviso/aviso.service';
import { mensagensDeErro } from '../../shared/erros';
import { Folha } from '../../shared/folha/folha';
import { Icone } from '../../shared/icone/icone';

type Painel = 'praca' | 'mesa' | null;

/**
 * UC24 — praças e mesas do salão (RF23). As vagas de garçom por praça são a entrada da alocação (RN03), e o
 * vínculo entre mesa e praça é fixo: mudar de lugar quebraria o histórico de faturamento por praça (REL01).
 */
@Component({
  selector: 'app-gestao-salao',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Folha, Icone],
  templateUrl: './gestao-salao.html',
  styleUrl: './gestao-salao.scss',
})
export class GestaoSalao {
  private readonly api = inject(GastraApiService);
  private readonly avisos = inject(AvisoService);

  protected readonly carregando = signal(true);
  protected readonly salvando = signal(false);
  protected readonly erros = signal<string[]>([]);
  protected readonly errosDoPainel = signal<string[]>([]);
  protected readonly pracas = signal<Praca[]>([]);
  protected readonly mesas = signal<Mesa[]>([]);

  protected readonly painel = signal<Painel>(null);
  protected readonly pracaEmEdicao = signal<Praca | null>(null);
  protected readonly mesaEmEdicao = signal<Mesa | null>(null);

  protected formularioDaPraca = { codigo: '', garcons: '1' };
  protected formularioDaMesa = { numero: '', capacidade: '4', pracaId: '' };

  protected readonly vagasTotais = computed(() => this.pracas().reduce((total, p) => total + p.quantidadeGarcons, 0));

  constructor() {
    this.carregar();
  }

  protected mesasDa(praca: Praca): Mesa[] {
    return this.mesas()
      .filter((m) => m.pracaId === praca.id)
      .sort((a, b) => a.numero.localeCompare(b.numero, 'pt-BR', { numeric: true }));
  }

  protected lugaresDa(praca: Praca): number {
    return this.mesasDa(praca).reduce((total, m) => total + m.capacidade, 0);
  }

  protected abrirPraca(praca: Praca | null): void {
    this.pracaEmEdicao.set(praca);
    this.formularioDaPraca = { codigo: praca?.codigo ?? '', garcons: String(praca?.quantidadeGarcons ?? 1) };
    this.errosDoPainel.set([]);
    this.painel.set('praca');
  }

  protected abrirMesa(mesa: Mesa | null, praca?: Praca): void {
    this.mesaEmEdicao.set(mesa);
    this.formularioDaMesa = {
      numero: mesa?.numero ?? '',
      capacidade: String(mesa?.capacidade ?? 4),
      pracaId: String(mesa?.pracaId ?? praca?.id ?? this.pracas()[0]?.id ?? ''),
    };
    this.errosDoPainel.set([]);
    this.painel.set('mesa');
  }

  protected fecharPainel(): void {
    this.painel.set(null);
    this.errosDoPainel.set([]);
  }

  protected pracaDaMesaEmEdicao(): string {
    const mesa = this.mesaEmEdicao();
    return mesa ? this.pracas().find((p) => p.id === mesa.pracaId)?.codigo ?? '' : '';
  }

  protected salvarPraca(): void {
    const dados = { codigo: this.formularioDaPraca.codigo.trim(), quantidadeGarcons: Number(this.formularioDaPraca.garcons) };
    const praca = this.pracaEmEdicao();
    this.salvar(
      praca ? this.api.editarPraca(praca.id, dados) : this.api.criarPraca(dados),
      `Praça ${dados.codigo} salva`,
    );
  }

  protected salvarMesa(): void {
    const numero = this.formularioDaMesa.numero.trim();
    const capacidade = Number(this.formularioDaMesa.capacidade);
    const mesa = this.mesaEmEdicao();
    this.salvar(
      mesa
        ? this.api.editarMesa(mesa.id, numero, capacidade)
        : this.api.criarMesa({ numero, capacidade, pracaId: Number(this.formularioDaMesa.pracaId) }),
      `Mesa ${numero} salva`,
    );
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erros.set([]);
    forkJoin({ pracas: this.api.listarPracas(), mesas: this.api.listarMesas() }).subscribe({
      next: ({ pracas, mesas }) => {
        this.pracas.set(pracas);
        this.mesas.set(mesas);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.erros.set(mensagensDeErro(erro));
        this.carregando.set(false);
      },
    });
  }

  /** As validações são da API (código repetido, capacidade mínima); a tela mostra a resposta dentro do painel. */
  private salvar<T>(chamada: Observable<T>, mensagem: string): void {
    this.salvando.set(true);
    this.errosDoPainel.set([]);
    chamada.subscribe({
      next: () => {
        this.salvando.set(false);
        this.painel.set(null);
        this.avisos.mostrar(mensagem);
        this.carregar();
      },
      error: (erro: unknown) => {
        this.salvando.set(false);
        this.errosDoPainel.set(mensagensDeErro(erro));
      },
    });
  }
}
