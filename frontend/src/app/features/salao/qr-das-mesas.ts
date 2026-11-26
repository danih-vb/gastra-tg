import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { toString as qrCodeSvg } from 'qrcode';
import { forkJoin } from 'rxjs';
import { GastraApiService } from '../../core/api/gastra-api.service';
import { Mesa, Praca } from '../../core/api/modelos';
import { mensagensDeErro } from '../../shared/erros';
import { Icone } from '../../shared/icone/icone';

/**
 * Folha de QR codes para colar nas mesas (#219). O QR é fixo e abre o cardápio digital com o número da mesa; a conta
 * continua pelo QR que o garçom mostra na comanda, que muda a cada atendimento. Um QR impresso que abrisse a conta
 * deixaria quem passou pela mesa ver a conta dos próximos clientes.
 */
@Component({
  selector: 'app-qr-das-mesas',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, RouterLink, Icone],
  template: `
    <header class="topo nao-imprimir">
      <a class="botao-icone" routerLink="/salao" aria-label="Voltar para o salão"><app-icone nome="voltar" /></a>
      <div>
        <h1>QR codes das mesas</h1>
      </div>
      <div class="acoes">
        <button class="botao" type="button" [disabled]="!listadas().length" (click)="imprimir()">
          <app-icone nome="importar" /> Imprimir
        </button>
      </div>
    </header>

    @if (erros().length) {
      <div class="alerta erro nao-imprimir" role="alert">
        <app-icone nome="alerta" />
        <div>
          @for (erro of erros(); track erro) {
            <div>{{ erro }}</div>
          }
        </div>
      </div>
    }

    <section class="cartao pad ferramentas nao-imprimir">
      <div class="campo">
        <label for="qr-endereco">Endereço do sistema</label>
        <input class="entrada" id="qr-endereco" name="endereco" autocomplete="off" [ngModel]="endereco()"
          (ngModelChange)="endereco.set($event)" />
      </div>
      <div class="chips" role="group" aria-label="Praça">
        <button class="chip" type="button" [attr.aria-pressed]="praca() === null" (click)="praca.set(null)">Todas</button>
        @for (opcao of pracas(); track opcao.id) {
          <button class="chip" type="button" [attr.aria-pressed]="praca() === opcao.id" (click)="praca.set(opcao.id)">
            Praça {{ opcao.codigo }}
          </button>
        }
      </div>
    </section>

    @if (carregando()) {
      <p class="carregando">Carregando as mesas…</p>
    } @else {
      <ul class="folha-de-qr">
        @for (mesa of listadas(); track mesa.id) {
          <li class="etiqueta-qr" [attr.data-link]="link(mesa)">
            <span class="marca">GASTRA</span>
            <strong class="numero">Mesa {{ mesa.numero }}</strong>
            <span class="qr" [innerHTML]="desenhos()[mesa.id] ?? ''"></span>
            <span class="praca">Cardápio · Praça {{ mesa.pracaCodigo }}</span>
          </li>
        } @empty {
          <li class="suave">Nenhuma mesa cadastrada.</li>
        }
      </ul>
    }
  `,
  styles: `
    .ferramentas { display: grid; gap: 12px; margin-bottom: 16px; }
    .folha-de-qr {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 12px;
      margin: 0;
      padding: 0;
      list-style: none;
    }
    .etiqueta-qr {
      display: grid;
      justify-items: center;
      gap: 6px;
      padding: 16px 12px;
      border: 1px dashed var(--borda-forte);
      border-radius: var(--raio);
      background: #fff;
      color: #1f2937;
      text-align: center;
      break-inside: avoid;
    }
    .marca { font-size: 0.75rem; font-weight: 700; letter-spacing: 0.12em; color: #2b3a4e; }
    .numero { font-size: 1.5rem; }
    .qr { display: block; width: 160px; height: 160px; }
    .qr ::ng-deep svg { width: 100%; height: 100%; }
    .praca { font-size: 0.8125rem; color: #4b5563; }
    @media print {
      .nao-imprimir { display: none !important; }
      .folha-de-qr { grid-template-columns: repeat(3, 1fr); gap: 0; }
      .etiqueta-qr { border-radius: 0; padding: 20px 8px; }
    }
  `,
})
export class QrDasMesas {
  private readonly api = inject(GastraApiService);
  private readonly sanitizador = inject(DomSanitizer);

  protected readonly carregando = signal(true);
  protected readonly erros = signal<string[]>([]);
  protected readonly pracas = signal<Praca[]>([]);
  protected readonly mesas = signal<Mesa[]>([]);
  protected readonly praca = signal<number | null>(null);
  /** A origem da própria página: aberta pelo IP da máquina, o QR já funciona no celular do cliente. */
  protected readonly endereco = signal(window.location.origin);
  protected readonly desenhos = signal<Record<number, SafeHtml>>({});

  protected readonly listadas = computed(() => {
    const praca = this.praca();
    return this.mesas()
      .filter((m) => praca === null || m.pracaId === praca)
      .sort(
        (a, b) =>
          a.pracaCodigo.localeCompare(b.pracaCodigo, 'pt-BR') ||
          a.numero.localeCompare(b.numero, 'pt-BR', { numeric: true }),
      );
  });

  constructor() {
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

    // Redesenha quando muda o endereço ou a lista: cada QR leva o link completo.
    effect(() => {
      const mesas = this.mesas();
      const base = this.endereco();
      void Promise.all(
        mesas.map(async (mesa) => {
          const svg = await qrCodeSvg(this.montarLink(base, mesa), { type: 'svg', margin: 1, errorCorrectionLevel: 'M' });
          return [mesa.id, this.sanitizador.bypassSecurityTrustHtml(svg)] as const;
        }),
      )
        .then((pares) => {
          if (this.endereco() === base) this.desenhos.set(Object.fromEntries(pares));
        })
        .catch(() => this.desenhos.set({}));
    });
  }

  protected link(mesa: Mesa): string {
    return this.montarLink(this.endereco(), mesa);
  }

  protected imprimir(): void {
    window.print();
  }

  private montarLink(base: string, mesa: Mesa): string {
    return `${base.trim().replace(/\/+$/, '')}/cliente/cardapio?mesa=${encodeURIComponent(mesa.numero)}`;
  }
}
