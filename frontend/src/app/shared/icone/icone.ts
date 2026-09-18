import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { inject } from '@angular/core';

/** Ícones em traço, 24×24, os mesmos do protótipo (docs/ux-ui/prototipo/assets/base.js). */
const CAMINHOS: Record<string, string> = {
  mesa: '<path d="M4 9h16M6 9v10M18 9v10M3 6h18"/>',
  pessoas:
    '<circle cx="9" cy="8" r="3"/><path d="M3 20c0-3.3 2.7-6 6-6s6 2.7 6 6"/><circle cx="17" cy="9" r="2.5"/><path d="M16 14.2c2.9.4 5 2.8 5 5.8"/>',
  voltar: '<path d="M15 5l-7 7 7 7"/>',
  mais: '<path d="M12 5v14M5 12h14"/>',
  menos: '<path d="M5 12h14"/>',
  check: '<path d="M5 12.5l4.5 4.5L19 7"/>',
  x: '<path d="M6 6l12 12M18 6L6 18"/>',
  alerta: '<path d="M12 3l9.5 17h-19z"/><path d="M12 10v4M12 17.5v.01"/>',
  info: '<circle cx="12" cy="12" r="9"/><path d="M12 11v6M12 7.5v.01"/>',
  relogio: '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',
  trofeu:
    '<path d="M8 4h8v5a4 4 0 01-8 0z"/><path d="M8 6H5a3 3 0 003 4M16 6h3a3 3 0 01-3 4M12 13v4M8 20h8"/>',
  sair: '<path d="M14 4h5v16h-5M10 8l-4 4 4 4M6 12h10"/>',
  faisca: '<path d="M12 3v4M12 17v4M3 12h4M17 12h4M6 6l2.5 2.5M15.5 15.5L18 18M6 18l2.5-2.5M15.5 8.5L18 6"/>',
  cadeado: '<rect x="5" y="11" width="14" height="10" rx="2"/><path d="M8 11V7a4 4 0 018 0v4"/>',
  escudo: '<path d="M12 3l8 3v6c0 5-3.5 8-8 9-4.5-1-8-4-8-9V6z"/>',
  qr: '<rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/><path d="M14 14h3v3h-3zM20 14v.01M14 20h.01M17 20h4v-3"/>',
  etiqueta: '<path d="M3 12V3h9l9 9-9 9z"/><circle cx="7.5" cy="7.5" r="1.5"/>',
  atualizar: '<path d="M20 12a8 8 0 11-2.3-5.7M20 4v5h-5"/>',
  cardapio: '<path d="M6 3h12v18H6z"/><path d="M9 8h6M9 12h6M9 16h4"/>',
  avancar: '<path d="M9 5l7 7-7 7"/>',
};

@Component({
  selector: 'app-icone',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <svg
      class="icone"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
      [attr.role]="rotulo() ? 'img' : null"
      [attr.aria-label]="rotulo() || null"
      [attr.aria-hidden]="rotulo() ? null : 'true'"
      [innerHTML]="desenho()"
    ></svg>
  `,
  styles: ':host { display: inline-flex; }',
})
export class Icone {
  private readonly sanitizador = inject(DomSanitizer);

  readonly nome = input.required<string>();
  /** Preenchido só quando o ícone carrega sentido sozinho; sem ele o ícone fica escondido dos leitores de tela. */
  readonly rotulo = input('');

  protected readonly desenho = computed<SafeHtml>(() =>
    this.sanitizador.bypassSecurityTrustHtml(CAMINHOS[this.nome()] ?? ''),
  );
}
