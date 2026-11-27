import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/** Foto do funcionário, ou as iniciais quando o Gerente ainda não cadastrou uma ("Carla Mendes" vira CM). */
@Component({
  selector: 'app-avatar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (foto()) {
      <img [src]="foto()" alt="" />
    } @else {
      {{ iniciais() }}
    }
  `,
  styles: `
    :host {
      display: grid;
      place-items: center;
      flex: none;
      width: var(--tamanho-avatar, 36px);
      height: var(--tamanho-avatar, 36px);
      overflow: hidden;
      border-radius: 50%;
      font-size: 0.8125rem;
      font-weight: 700;
      letter-spacing: 0.04em;
      color: var(--marinho);
      background: var(--marinho-suave);
    }
    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
  `,
  host: { 'aria-hidden': 'true' },
})
export class Avatar {
  readonly nome = input.required<string>();
  readonly foto = input<string | null | undefined>(null);

  protected readonly iniciais = computed(() => {
    const partes = this.nome().trim().split(/\s+/).filter(Boolean);
    const primeira = partes[0]?.[0] ?? '';
    const ultima = partes.length > 1 ? partes[partes.length - 1][0] : '';
    return (primeira + ultima).toUpperCase();
  });
}
