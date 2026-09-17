import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MENU, NOME_DO_PAPEL } from './core/navegacao';
import { SessaoService } from './core/sessao/sessao.service';

/** Casca da aplicação: cabeçalho, menu conforme o papel e área das telas. */
@Component({
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  private readonly sessao = inject(SessaoService);
  private readonly router = inject(Router);

  protected readonly usuario = this.sessao.usuario;
  protected readonly nomeDoPapel = NOME_DO_PAPEL;
  protected readonly menu = computed(() => {
    const usuario = this.usuario();
    return usuario ? MENU.filter((item) => item.papeis.includes(usuario.papel)) : [];
  });

  protected sair(): void {
    // O logoff na API pode falhar (sem rede); a sessão local é encerrada de qualquer jeito.
    this.sessao.sair().subscribe({ error: () => undefined, complete: () => undefined });
    void this.router.navigate(['/acesso/login']);
  }
}
