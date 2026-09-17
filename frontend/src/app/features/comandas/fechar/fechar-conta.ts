import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { GastraApiService } from '../../../core/api/gastra-api.service';
import { Comanda, Mesa } from '../../../core/api/modelos';
import { AvisoService } from '../../../shared/aviso/aviso.service';
import { mensagensDeErro } from '../../../shared/erros';
import { Folha } from '../../../shared/folha/folha';
import { Icone } from '../../../shared/icone/icone';
import { tempoRelativo } from '../../../shared/rotulos';

/** UC14 — fechamento da conta: taxa de serviço (RF04) e bloqueio por itens pendentes (RN02). */
@Component({
  selector: 'app-fechar-conta',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, RouterLink, Folha, Icone],
  templateUrl: './fechar-conta.html',
  styleUrl: './fechar-conta.scss',
})
export class FecharConta implements OnInit {
  private readonly api = inject(GastraApiService);
  private readonly avisos = inject(AvisoService);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly carregando = signal(true);
  protected readonly salvando = signal(false);
  protected readonly erros = signal<string[]>([]);
  protected readonly comanda = signal<Comanda | null>(null);
  protected readonly mesas = signal<Mesa[]>([]);
  protected readonly confirmando = signal(false);
  protected readonly fechada = signal(false);

  protected readonly mesa = computed(() => this.mesas().find((m) => m.id === this.comanda()?.mesaId) ?? null);
  protected readonly pendentes = computed(() => this.comanda()?.itens.filter((i) => i.status === 'Pendente') ?? []);
  protected readonly cobrados = computed(() => this.comanda()?.itens.filter((i) => i.status !== 'Cancelado') ?? []);

  // O id da rota só existe depois que o Angular preenche as entradas, e não dentro do construtor.
  ngOnInit(): void {
    this.carregar();
  }

  protected tempo(dataIso: string): string {
    return tempoRelativo(dataIso);
  }

  /** RF04: a taxa entra por padrão e só sai a pedido do cliente. A API só tem o caminho de remover. */
  protected removerTaxa(): void {
    const comanda = this.comanda();
    if (!comanda || comanda.taxaServicoRemovida || this.salvando()) {
      return;
    }
    this.salvando.set(true);
    this.api.removerTaxaDeServico(comanda.id).subscribe({
      next: (atualizada) => {
        this.comanda.set(atualizada);
        this.salvando.set(false);
        this.avisos.mostrar('Taxa de serviço retirada a pedido do cliente');
      },
      error: (erro: unknown) => {
        this.salvando.set(false);
        this.erros.set(mensagensDeErro(erro));
      },
    });
  }

  protected fechar(): void {
    const comanda = this.comanda();
    if (!comanda || this.salvando()) {
      return;
    }
    this.salvando.set(true);
    this.api.fecharComanda(comanda.id).subscribe({
      next: (fechada) => {
        this.comanda.set(fechada);
        this.salvando.set(false);
        this.confirmando.set(false);
        this.fechada.set(true);
      },
      error: (erro: unknown) => {
        this.salvando.set(false);
        this.confirmando.set(false);
        this.erros.set(mensagensDeErro(erro));
      },
    });
  }

  protected voltarAoSalao(): void {
    void this.router.navigate(['/comandas']);
  }

  protected carregar(): void {
    this.carregando.set(true);
    forkJoin({ comanda: this.api.obterComanda(Number(this.id())), mesas: this.api.listarMesas() }).subscribe({
      next: ({ comanda, mesas }) => {
        this.comanda.set(comanda);
        this.mesas.set(mesas);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.erros.set(mensagensDeErro(erro));
        this.carregando.set(false);
      },
    });
  }
}
