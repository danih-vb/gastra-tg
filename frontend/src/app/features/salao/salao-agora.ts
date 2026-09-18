import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { GastraApiService } from '../../core/api/gastra-api.service';
import { Comanda, Mesa, Praca } from '../../core/api/modelos';
import { mensagensDeErro } from '../../shared/erros';
import { Folha } from '../../shared/folha/folha';
import { Icone } from '../../shared/icone/icone';
import { COMPOSICOES, MOTIVOS, RESTRICOES, minutosDesde } from '../../shared/rotulos';

interface LinhaDoSalao {
  comanda: Comanda;
  mesa: string;
  pendentes: number;
  minutos: number;
}

/**
 * Painel do salão para o Metre: o que está aberto agora, por praça. Só leitura — quem mexe na comanda é o garçom
 * dela. As restrições aparecem porque o Metre também atende a mesa (RN04).
 */
@Component({
  selector: 'app-salao-agora',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, Folha, Icone],
  templateUrl: './salao-agora.html',
  styleUrl: './salao-agora.scss',
})
export class SalaoAgora {
  private readonly api = inject(GastraApiService);

  protected readonly carregando = signal(true);
  protected readonly erros = signal<string[]>([]);
  protected readonly pracas = signal<Praca[]>([]);
  protected readonly mesas = signal<Mesa[]>([]);
  protected readonly comandas = signal<Comanda[]>([]);
  protected readonly comandaAberta = signal<Comanda | null>(null);

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
      }))
      .sort((a, b) => a.mesa.localeCompare(b.mesa));
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
