import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Icone } from '../../../shared/icone/icone';

/**
 * Porta de entrada do cliente: daqui ele vê o cardápio (UC19). A própria conta (UC20) abre direto pelo QR code que o
 * garçom mostra na comanda: o código de acesso tem 32 caracteres e não é para ser digitado. Sem login, e sem pedido.
 */
@Component({
  selector: 'app-inicio-do-cliente',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, Icone],
  templateUrl: './inicio-do-cliente.html',
  styleUrl: './inicio-do-cliente.scss',
})
export class InicioDoCliente {}
