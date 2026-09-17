import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Icone } from '../icone/icone';
import { AvisoService } from './aviso.service';

@Component({
  selector: 'app-avisos',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icone],
  template: `
    @if (servico.aviso(); as aviso) {
      <div class="aviso-flutuante" role="status">
        <app-icone nome="check" />
        <span>{{ aviso.mensagem }}</span>
        @if (aviso.acao; as acao) {
          <button type="button" (click)="servico.desfazer()">{{ acao.rotulo }}</button>
        }
      </div>
    }
  `,
  styles: `
    .aviso-flutuante {
      position: fixed;
      left: 50%;
      bottom: 88px;
      z-index: 30;
      transform: translateX(-50%);
      display: flex;
      align-items: center;
      gap: 8px;
      width: max-content;
      max-width: min(398px, calc(100vw - 32px));
      padding: 12px 16px;
      color: #fff;
      background: var(--marinho-forte);
      border-radius: var(--raio-pequeno);
      box-shadow: var(--sombra-alta);
    }

    button {
      margin-left: 8px;
      padding: 4px 8px;
      color: #ffd2bd;
      font-weight: 600;
      background: none;
      border: 0;
      cursor: pointer;
    }
  `,
})
export class Avisos {
  protected readonly servico = inject(AvisoService);
}
