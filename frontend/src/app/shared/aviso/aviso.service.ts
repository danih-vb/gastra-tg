import { Injectable, signal } from '@angular/core';

export interface Aviso {
  id: number;
  mensagem: string;
  /** Ação de desfazer, quando existe: é o que dispensa a confirmação nas ações do dia a dia. */
  acao?: { rotulo: string; executar: () => void };
}

/**
 * Avisos curtos no rodapé da tela. As ações frequentes do garçom (entregar, lançar item) não pedem confirmação:
 * mostram o que aconteceu com um "Desfazer" à mão, que é mais rápido e perdoa o erro do mesmo jeito.
 */
@Injectable({ providedIn: 'root' })
export class AvisoService {
  private readonly atual = signal<Aviso | null>(null);
  private proximoId = 1;
  private temporizador: ReturnType<typeof setTimeout> | null = null;

  readonly aviso = this.atual.asReadonly();

  mostrar(mensagem: string, acao?: { rotulo: string; executar: () => void }): void {
    this.limparTemporizador();
    this.atual.set({ id: this.proximoId++, mensagem, acao });
    this.temporizador = setTimeout(() => this.atual.set(null), acao ? 6000 : 3000);
  }

  desfazer(): void {
    const acao = this.atual()?.acao;
    this.fechar();
    acao?.executar();
  }

  fechar(): void {
    this.limparTemporizador();
    this.atual.set(null);
  }

  private limparTemporizador(): void {
    if (this.temporizador) {
      clearTimeout(this.temporizador);
      this.temporizador = null;
    }
  }
}
