import { Component, inject, OnInit, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { PAGINA_INICIAL } from '../../../core/sessao/guardas';
import { ConfiguracaoDoSegundoFator } from '../../../core/sessao/modelos';
import { SessaoService } from '../../../core/sessao/sessao.service';
import { mensagensDeErro } from '../../../shared/erros';
import { Icone } from '../../../shared/icone/icone';

/**
 * UC02 — Confirmar o segundo fator (Gerente e Coordenador). No primeiro acesso, antes da confirmação, mostra a chave
 * para vincular o app autenticador; ela aparece uma única vez (RN07).
 */
@Component({
  selector: 'app-segundo-fator',
  imports: [ReactiveFormsModule, Icone],
  templateUrl: './segundo-fator.html',
  styleUrl: '../acesso.scss',
})
export class SegundoFator implements OnInit {
  private readonly sessao = inject(SessaoService);
  private readonly router = inject(Router);
  private readonly rota = inject(ActivatedRoute);

  protected readonly configurando = this.rota.snapshot.queryParamMap.get('configurar') === '1';
  protected readonly configuracao = signal<ConfiguracaoDoSegundoFator | null>(null);
  protected readonly enviando = signal(false);
  protected readonly erros = signal<string[]>([]);

  protected readonly formulario = inject(NonNullableFormBuilder).group({
    codigo: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
  });

  ngOnInit(): void {
    // Sem login em andamento (página recarregada, acesso direto), recomeça pelo login.
    if (!this.sessao.segundoFatorPendente) {
      void this.router.navigate(['/acesso/login']);
      return;
    }

    if (this.configurando) {
      this.sessao.configurarSegundoFator().subscribe({
        next: (configuracao) => this.configuracao.set(configuracao),
        error: (erro: unknown) => this.erros.set(mensagensDeErro(erro)),
      });
    }
  }

  protected confirmar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.erros.set([]);

    this.sessao.confirmarSegundoFator(this.formulario.getRawValue().codigo).subscribe({
      next: () => {
        this.enviando.set(false);
        const voltar = this.rota.snapshot.queryParamMap.get('voltar');
        const usuario = this.sessao.usuario();
        const destino = voltar?.startsWith('/') && !voltar.startsWith('//') ? voltar : usuario ? PAGINA_INICIAL[usuario.papel] : '/';
        void this.router.navigateByUrl(destino);
      },
      error: (erro: unknown) => {
        this.enviando.set(false);
        this.formulario.reset();
        this.erros.set(mensagensDeErro(erro));
      },
    });
  }
}
