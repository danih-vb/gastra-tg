import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { GastraApiService } from '../../core/api/gastra-api.service';
import { Usuario } from '../../core/api/modelos';
import { Papel } from '../../core/sessao/modelos';
import { SessaoService } from '../../core/sessao/sessao.service';
import { AvisoService } from '../../shared/aviso/aviso.service';
import { mensagensDeErro } from '../../shared/erros';
import { Folha } from '../../shared/folha/folha';
import { Icone } from '../../shared/icone/icone';
import { PAPEIS } from '../../shared/rotulos';

type Filtro = 'ativos' | 'inativos' | 'todos';
type Painel = 'conta' | 'inativar' | 'senha' | 'segundo-fator' | null;

/**
 * UC04 — contas de usuário (RF18). Inativar não apaga: a pessoa perde o acesso na hora e o histórico continua.
 * Redefinir senha e zerar o segundo fator (#141) não valem para a própria conta, e a API recusa se tentar.
 */
@Component({
  selector: 'app-gestao-usuarios',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, Folha, Icone],
  templateUrl: './gestao-usuarios.html',
  styleUrl: './gestao-usuarios.scss',
})
export class GestaoUsuarios {
  private readonly api = inject(GastraApiService);
  private readonly sessao = inject(SessaoService);
  private readonly avisos = inject(AvisoService);

  protected readonly carregando = signal(true);
  protected readonly salvando = signal(false);
  protected readonly erros = signal<string[]>([]);
  protected readonly errosDoPainel = signal<string[]>([]);
  protected readonly usuarios = signal<Usuario[]>([]);
  protected readonly filtro = signal<Filtro>('ativos');

  protected readonly painel = signal<Painel>(null);
  protected readonly usuarioEmFoco = signal<Usuario | null>(null);
  protected formularioDaConta = { nome: '', email: '', papel: '' as Papel | '', senha: '' };
  protected senhaNova = '';

  protected readonly PAPEIS = PAPEIS;
  protected readonly papeisPossiveis = Object.keys(PAPEIS) as Papel[];
  protected readonly filtros: { chave: Filtro; nome: string }[] = [
    { chave: 'ativos', nome: 'Ativos' },
    { chave: 'inativos', nome: 'Inativos' },
    { chave: 'todos', nome: 'Todos' },
  ];

  protected readonly meuId = computed(() => this.sessao.usuario()?.id ?? 0);

  protected readonly listados = computed(() =>
    this.usuarios().filter((u) => (this.filtro() === 'todos' ? true : this.filtro() === 'ativos' ? u.ativo : !u.ativo)),
  );

  constructor() {
    this.carregar();
  }

  protected quantos(filtro: Filtro): number {
    return this.usuarios().filter((u) => (filtro === 'todos' ? true : filtro === 'ativos' ? u.ativo : !u.ativo)).length;
  }

  /** Só Gerente e Coordenador usam o autenticador (RF16). */
  protected usaSegundoFator(usuario: Usuario): boolean {
    return usuario.papel === 'Gerente' || usuario.papel === 'Coordenador';
  }

  protected abrirConta(usuario: Usuario | null): void {
    this.usuarioEmFoco.set(usuario);
    this.formularioDaConta = {
      nome: usuario?.nome ?? '',
      email: usuario?.email ?? '',
      papel: usuario?.papel ?? '',
      senha: '',
    };
    this.abrir('conta');
  }

  protected abrirInativacao(usuario: Usuario): void {
    this.usuarioEmFoco.set(usuario);
    this.abrir('inativar');
  }

  protected abrirSenha(usuario: Usuario): void {
    this.usuarioEmFoco.set(usuario);
    this.senhaNova = '';
    this.abrir('senha');
  }

  protected abrirSegundoFator(usuario: Usuario): void {
    this.usuarioEmFoco.set(usuario);
    this.abrir('segundo-fator');
  }

  protected fecharPainel(): void {
    this.painel.set(null);
    this.errosDoPainel.set([]);
  }

  protected salvarConta(): void {
    const usuario = this.usuarioEmFoco();
    const { nome, email, papel, senha } = this.formularioDaConta;
    if (!papel) {
      this.errosDoPainel.set(['Escolha o papel.']);
      return;
    }
    this.salvar(
      usuario
        ? this.api.editarUsuario(usuario.id, nome.trim(), email.trim(), papel)
        : this.api.cadastrarUsuario({ nome: nome.trim(), email: email.trim(), senha, papel }),
      `${nome.trim()} ${usuario ? 'atualizado' : 'cadastrado'}`,
    );
  }

  protected confirmarInativacao(): void {
    const usuario = this.usuarioEmFoco();
    if (!usuario) return;
    this.salvar(this.api.alterarSituacaoDoUsuario(usuario.id, false), `${usuario.nome} inativado`);
  }

  protected reativar(usuario: Usuario): void {
    this.salvar(this.api.alterarSituacaoDoUsuario(usuario.id, true), `${usuario.nome} reativado`);
  }

  protected confirmarSenha(): void {
    const usuario = this.usuarioEmFoco();
    if (!usuario) return;
    this.salvar(this.api.redefinirSenha(usuario.id, this.senhaNova), `Senha de ${usuario.nome} redefinida`);
  }

  protected confirmarSegundoFator(): void {
    const usuario = this.usuarioEmFoco();
    if (!usuario) return;
    this.salvar(this.api.reiniciarSegundoFator(usuario.id), `Verificação em duas etapas de ${usuario.nome} zerada`);
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erros.set([]);
    this.api.listarUsuarios().subscribe({
      next: (usuarios) => {
        this.usuarios.set(usuarios);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.erros.set(mensagensDeErro(erro));
        this.carregando.set(false);
      },
    });
  }

  private abrir(painel: Painel): void {
    this.errosDoPainel.set([]);
    this.painel.set(painel);
  }

  private salvar<T>(chamada: Observable<T>, mensagem: string): void {
    this.salvando.set(true);
    this.errosDoPainel.set([]);
    chamada.subscribe({
      next: () => {
        this.salvando.set(false);
        this.painel.set(null);
        this.avisos.mostrar(mensagem);
        this.carregar();
      },
      error: (erro: unknown) => {
        this.salvando.set(false);
        if (this.painel()) {
          this.errosDoPainel.set(mensagensDeErro(erro));
        } else {
          this.erros.set(mensagensDeErro(erro));
        }
      },
    });
  }
}
