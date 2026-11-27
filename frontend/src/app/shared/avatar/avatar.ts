import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { catchError, map, of, switchMap } from 'rxjs';
import { GastraApiService } from '../../core/api/gastra-api.service';

/**
 * Foto do funcionário, ou as iniciais quando o Gerente ainda não cadastrou uma ("Carla Mendes" vira CM).
 *
 * A foto só sai da API com login, e um <img src> não manda o token. Por isso ela vem pelo HttpClient e vira um
 * endereço blob: que só existe nesta página e é liberado quando a foto muda ou o crachá sai da tela. Enquanto carrega,
 * ou se a API não entregar, ficam as iniciais.
 */
@Component({
  selector: 'app-avatar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (endereco(); as endereco) {
      <img [src]="endereco" alt="" />
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
  private readonly api = inject(GastraApiService);

  readonly nome = input.required<string>();
  readonly foto = input<string | null | undefined>(null);

  protected readonly endereco = signal<string | null>(null);

  protected readonly iniciais = computed(() => {
    const partes = this.nome().trim().split(/\s+/).filter(Boolean);
    const primeira = partes[0]?.[0] ?? '';
    const ultima = partes.length > 1 ? partes[partes.length - 1][0] : '';
    return (primeira + ultima).toUpperCase();
  });

  constructor() {
    toObservable(this.foto)
      .pipe(
        switchMap((foto) =>
          foto
            ? this.api.fotoDoFuncionario(foto).pipe(
                map((arquivo) => URL.createObjectURL(arquivo)),
                catchError(() => of(null)),
              )
            : of(null),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((endereco) => this.trocar(endereco));
    inject(DestroyRef).onDestroy(() => this.trocar(null));
  }

  private trocar(endereco: string | null): void {
    const anterior = this.endereco();
    if (anterior) URL.revokeObjectURL(anterior);
    this.endereco.set(endereco);
  }
}
