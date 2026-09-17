import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { GastraApiService } from '../../../core/api/gastra-api.service';
import { Comanda, ItemDoPedido, Mesa } from '../../../core/api/modelos';
import { SessaoService } from '../../../core/sessao/sessao.service';
import { AvisoService } from '../../../shared/aviso/aviso.service';
import { mensagensDeErro } from '../../../shared/erros';
import { Icone } from '../../../shared/icone/icone';
import { minutosDesde, tempoRelativo } from '../../../shared/rotulos';

interface Pendencia {
  comanda: Comanda;
  item: ItemDoPedido;
  mesa: string;
  minutos: number;
}

/** RF03 — a lista de pendências visível: tudo que o garçom ainda deve levar, do mais antigo para o mais novo. */
@Component({
  selector: 'app-pendencias',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, Icone],
  templateUrl: './pendencias.html',
  styleUrl: './pendencias.scss',
})
export class Pendencias {
  private readonly api = inject(GastraApiService);
  private readonly sessao = inject(SessaoService);
  private readonly avisos = inject(AvisoService);

  protected readonly carregando = signal(true);
  protected readonly salvando = signal(false);
  protected readonly erros = signal<string[]>([]);
  private readonly comandas = signal<Comanda[]>([]);
  private readonly mesas = signal<Mesa[]>([]);

  protected readonly pendencias = computed<Pendencia[]>(() => {
    const meuId = this.sessao.usuario()?.id;
    const agora = Date.now();
    return this.comandas()
      .filter((c) => c.garcomId === meuId)
      .flatMap((comanda) =>
        comanda.itens
          .filter((item) => item.status === 'Pendente')
          .map((item) => ({
            comanda,
            item,
            mesa: this.mesas().find((m) => m.id === comanda.mesaId)?.numero ?? String(comanda.mesaId),
            minutos: minutosDesde(item.dataHoraRegistro, agora),
          })),
      )
      .sort((a, b) => b.minutos - a.minutos);
  });

  constructor() {
    this.carregar();
  }

  protected tempo(dataIso: string): string {
    return tempoRelativo(dataIso);
  }

  protected entregar(pendencia: Pendencia): void {
    if (this.salvando()) {
      return;
    }
    this.salvando.set(true);
    this.api.atualizarSituacaoDoItem(pendencia.comanda.id, pendencia.item.id, 'Entregue').subscribe({
      next: () => {
        this.salvando.set(false);
        this.avisos.mostrar(`${pendencia.item.nome}: entregue`);
        this.carregar();
      },
      error: (erro: unknown) => {
        this.salvando.set(false);
        this.erros.set(mensagensDeErro(erro));
      },
    });
  }

  protected carregar(): void {
    this.carregando.set(true);
    forkJoin({ comandas: this.api.listarComandasAbertas(), mesas: this.api.listarMesas() }).subscribe({
      next: ({ comandas, mesas }) => {
        this.comandas.set(comandas);
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
