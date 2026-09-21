import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, effect, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { GastraApiService } from '../../../core/api/gastra-api.service';
import { ComandaDoCliente, StatusItemPedido } from '../../../core/api/modelos';
import { mensagensDeErro } from '../../../shared/erros';
import { Icone } from '../../../shared/icone/icone';

/** De quanto em quanto tempo a conta se atualiza sozinha enquanto a mesa está aberta. */
const INTERVALO_MS = 15_000;

/**
 * UC20 — conta da mesa em tempo real (RF13), aberta pelo QR code. Tela pública: mostra só itens e valores daquela
 * mesa, sem nome de garçom, restrição alimentar ou qualquer dado pessoal (RN04).
 */
@Component({
  selector: 'app-conta-do-cliente',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, DatePipe, FormsModule, RouterLink, Icone],
  templateUrl: './conta-do-cliente.html',
  styleUrl: './conta-do-cliente.scss',
})
export class ContaDoCliente implements OnInit, OnDestroy {
  private readonly api = inject(GastraApiService);

  /** Código do QR code da mesa, vindo da rota. */
  readonly codigo = input.required<string>();

  protected readonly carregando = signal(true);
  protected readonly erros = signal<string[]>([]);
  protected readonly naoEncontrada = signal(false);
  protected readonly comanda = signal<ComandaDoCliente | null>(null);
  protected readonly atualizadoHa = signal(0);

  private relogio: ReturnType<typeof setInterval> | null = null;

  /** RF25 — avaliação do atendimento. A tela só mostra isto quando a API diz que dá (conta fechada, sem avaliação, no prazo). */
  protected readonly NOTAS = [1, 2, 3, 4, 5];
  protected readonly LIMITE_DO_COMENTARIO = 280;
  protected readonly nota = signal(0);
  protected comentario = '';
  protected readonly enviando = signal(false);
  protected readonly errosDaAvaliacao = signal<string[]>([]);

  protected readonly SITUACAO: Record<StatusItemPedido, { texto: string; classe: string }> = {
    Pendente: { texto: 'A caminho', classe: 'aviso' },
    Entregue: { texto: 'Entregue', classe: 'sucesso' },
    Cancelado: { texto: 'Cancelado · não cobrado', classe: '' },
  };

  constructor() {
    // Trocar só o código na mesma rota reaproveita o componente: sem isto, a tela continuaria mostrando a conta
    // (ou o erro) do código anterior.
    effect(() => {
      this.codigo();
      this.carregar();
    });
  }

  ngOnInit(): void {
    this.relogio = setInterval(() => {
      this.atualizadoHa.update((segundos) => segundos + INTERVALO_MS / 1000);
      if (!this.comanda()?.fechada) {
        this.carregar(true);
      }
    }, INTERVALO_MS);
  }

  ngOnDestroy(): void {
    if (this.relogio) {
      clearInterval(this.relogio);
    }
  }

  protected atualizarAgora(): void {
    this.carregar();
  }

  protected enviarAvaliacao(): void {
    const nota = this.nota();
    if (!nota) return;

    this.enviando.set(true);
    this.errosDaAvaliacao.set([]);
    this.api.avaliarAtendimento(this.codigo(), { nota, comentario: this.comentario.trim() || undefined }).subscribe({
      next: () => {
        this.enviando.set(false);
        this.comentario = '';
        // Recarrega em vez de marcar na mão: quem decide se já foi avaliada é a API.
        this.carregar(true);
      },
      error: (erro: unknown) => {
        this.enviando.set(false);
        this.errosDaAvaliacao.set(mensagensDeErro(erro));
      },
    });
  }

  /** `emSegundoPlano` mantém o que está na tela enquanto a atualização automática acontece. */
  protected carregar(emSegundoPlano = false): void {
    if (!emSegundoPlano) {
      this.carregando.set(true);
      this.erros.set([]);
    }
    this.api.consultarComandaDoCliente(this.codigo()).subscribe({
      next: (comanda) => {
        this.comanda.set(comanda);
        this.naoEncontrada.set(false);
        this.carregando.set(false);
        this.atualizadoHa.set(0);
      },
      error: (erro: unknown) => {
        this.carregando.set(false);
        // 404: o código não existe (ou foi digitado errado). Não é erro de sistema, e a tela explica o que fazer.
        if (typeof erro === 'object' && erro !== null && 'status' in erro && erro.status === 404) {
          this.naoEncontrada.set(true);
          return;
        }
        if (!emSegundoPlano) {
          this.erros.set(mensagensDeErro(erro));
        }
      },
    });
  }
}
