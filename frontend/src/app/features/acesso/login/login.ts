import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PAGINA_INICIAL } from '../../../core/sessao/guardas';
import { SessaoService } from '../../../core/sessao/sessao.service';
import { mensagensDeErro } from '../../../shared/erros';

/** UC01 — Autenticar-se. Layout provisório: o visual final segue o protótipo (#47). */
@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: '../acesso.scss',
})
export class Login {
  private readonly sessao = inject(SessaoService);
  private readonly router = inject(Router);
  private readonly rota = inject(ActivatedRoute);

  protected readonly enviando = signal(false);
  protected readonly erros = signal<string[]>([]);
  protected readonly sessaoEncerrada = this.rota.snapshot.queryParamMap.get('sessao') === 'encerrada';

  protected readonly formulario = inject(NonNullableFormBuilder).group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', Validators.required],
  });

  protected entrar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.erros.set([]);
    const { email, senha } = this.formulario.getRawValue();

    this.sessao.entrar(email, senha).subscribe({
      next: (resultado) => {
        this.enviando.set(false);
        if (resultado === 'acesso-liberado') {
          void this.router.navigateByUrl(this.destino());
        } else {
          void this.router.navigate(['/acesso/segundo-fator'], {
            queryParams: { configurar: resultado === 'configurar-segundo-fator' ? 1 : null, voltar: this.voltar() },
          });
        }
      },
      error: (erro: unknown) => {
        this.enviando.set(false);
        this.erros.set(mensagensDeErro(erro));
      },
    });
  }

  private voltar(): string | null {
    const voltar = this.rota.snapshot.queryParamMap.get('voltar');
    // Só caminhos internos: um "voltar" apontando para outro site seria redirecionamento aberto.
    return voltar?.startsWith('/') && !voltar.startsWith('//') ? voltar : null;
  }

  private destino(): string {
    const usuario = this.sessao.usuario();
    return this.voltar() ?? (usuario ? PAGINA_INICIAL[usuario.papel] : '/');
  }
}
