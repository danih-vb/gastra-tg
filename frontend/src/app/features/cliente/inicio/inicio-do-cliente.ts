import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Icone } from '../../../shared/icone/icone';

/**
 * Porta de entrada do cliente: é a página que o QR code da mesa abre. Daqui ele vê o cardápio (UC19) ou acompanha
 * a própria conta (UC20). Sem login, e sem função de pedido.
 */
@Component({
  selector: 'app-inicio-do-cliente',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, RouterLink, Icone],
  templateUrl: './inicio-do-cliente.html',
  styleUrl: './inicio-do-cliente.scss',
})
export class InicioDoCliente {
  private readonly router = inject(Router);

  protected codigo = '';
  protected readonly erro = signal('');

  protected verConta(): void {
    const codigo = this.codigo.trim().toUpperCase();
    if (codigo.length !== 6) {
      this.erro.set('O código tem 6 caracteres.');
      return;
    }
    this.erro.set('');
    void this.router.navigate(['/cliente/conta', codigo]);
  }
}
