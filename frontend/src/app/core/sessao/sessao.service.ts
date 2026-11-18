import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, finalize, map } from 'rxjs';
import { ARMAZENAMENTO_DA_SESSAO, URL_DA_API } from '../configuracao';
import {
  ConfiguracaoDoSegundoFator,
  Papel,
  RespostaDeLogin,
  ResultadoDoLogin,
  UsuarioDaSessao,
} from './modelos';

const CHAVE = 'gastra.sessao';

/**
 * Sessão do usuário no navegador: login (UC01), segundo fator (UC02, RN07) e logoff (UC03).
 *
 * O token temporário do segundo fator fica só na memória: se a página recarregar no meio do processo, o usuário
 * começa de novo pelo login, que é o comportamento mais seguro.
 */
@Injectable({ providedIn: 'root' })
export class SessaoService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(URL_DA_API);
  private readonly armazenamento = inject(ARMAZENAMENTO_DA_SESSAO);

  private readonly usuarioAtual = signal<UsuarioDaSessao | null>(this.restaurar());
  private tokenSegundoFator: string | null = null;

  readonly usuario = this.usuarioAtual.asReadonly();
  readonly autenticado = computed(() => this.usuarioAtual() !== null);

  /** Há um login esperando o segundo fator (Gerente ou Coordenador). */
  get segundoFatorPendente(): boolean {
    return this.tokenSegundoFator !== null;
  }

  entrar(email: string, senha: string): Observable<ResultadoDoLogin> {
    return this.http
      .post<RespostaDeLogin>(`${this.api}/api/autenticacao/login`, { email, senha })
      .pipe(map((resposta) => this.tratarLogin(resposta)));
  }

  /** Primeiro acesso de Gerente ou Coordenador: gera a chave do app autenticador, exibida uma única vez (RN07). */
  configurarSegundoFator(): Observable<ConfiguracaoDoSegundoFator> {
    return this.http.post<ConfiguracaoDoSegundoFator>(`${this.api}/api/autenticacao/segundo-fator/configurar`, {
      tokenSegundoFator: this.tokenSegundoFator ?? '',
    });
  }

  confirmarSegundoFator(codigo: string): Observable<void> {
    return this.http
      .post<RespostaDeLogin>(`${this.api}/api/autenticacao/segundo-fator/confirmar`, {
        tokenSegundoFator: this.tokenSegundoFator ?? '',
        codigo,
      })
      .pipe(
        map((resposta) => {
          this.guardar(resposta);
          this.tokenSegundoFator = null;
        }),
      );
  }

  /** Encerra a sessão na API (o token deixa de valer em qualquer aparelho) e no navegador, mesmo se a API falhar. */
  sair(): Observable<void> {
    return this.http.post<void>(`${this.api}/api/autenticacao/logoff`, {}).pipe(finalize(() => this.encerrarLocalmente()));
  }

  encerrarLocalmente(): void {
    this.armazenamento.removeItem(CHAVE);
    this.usuarioAtual.set(null);
    this.tokenSegundoFator = null;
  }

  /** Token válido, ou nulo. Token vencido encerra a sessão na hora. */
  token(): string | null {
    const usuario = this.usuarioAtual();
    if (usuario === null) {
      return null;
    }
    if (Date.now() >= usuario.expiraEm) {
      this.encerrarLocalmente();
      return null;
    }
    return usuario.token;
  }

  temPapel(...papeis: Papel[]): boolean {
    const usuario = this.usuarioAtual();
    return usuario !== null && papeis.includes(usuario.papel);
  }

  private tratarLogin(resposta: RespostaDeLogin): ResultadoDoLogin {
    if (resposta.requerSegundoFator) {
      this.tokenSegundoFator = resposta.tokenSegundoFator;
      return resposta.requerConfiguracaoSegundoFator ? 'configurar-segundo-fator' : 'confirmar-segundo-fator';
    }
    this.guardar(resposta);
    return 'acesso-liberado';
  }

  private guardar(resposta: RespostaDeLogin): void {
    if (!resposta.tokenAcesso || !resposta.nome || !resposta.papel) {
      throw new Error('A API não devolveu o token de acesso.');
    }
    const usuario: UsuarioDaSessao = {
      token: resposta.tokenAcesso,
      nome: resposta.nome,
      papel: resposta.papel,
      id: idDoToken(resposta.tokenAcesso),
      expiraEm: expiracaoDoToken(resposta.tokenAcesso),
    };
    this.armazenamento.setItem(CHAVE, JSON.stringify(usuario));
    this.usuarioAtual.set(usuario);
  }

  private restaurar(): UsuarioDaSessao | null {
    try {
      const guardado = this.armazenamento.getItem(CHAVE);
      if (!guardado) {
        return null;
      }
      const usuario = JSON.parse(guardado) as UsuarioDaSessao;
      if (Date.now() >= usuario.expiraEm) {
        this.armazenamento.removeItem(CHAVE);
        return null;
      }
      return usuario;
    } catch {
      return null;
    }
  }
}

/**
 * Lê o conteúdo do JWT. Só decodifica, não valida a assinatura: quem valida é a API. Serve para o navegador saber
 * quando o token vence e quem é o dono dele, sem uma chamada a mais.
 */
function conteudoDoToken(token: string): { exp?: number; sub?: string } {
  try {
    const conteudo = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(conteudo)) as { exp?: number; sub?: string };
  } catch {
    return {};
  }
}

/** Instante, em milissegundos, em que o token deixa de valer. Zero quando não dá para ler. */
export function expiracaoDoToken(token: string): number {
  const { exp } = conteudoDoToken(token);
  return typeof exp === 'number' ? exp * 1000 : 0;
}

/** Id do usuário dono do token (claim "sub"). É o que identifica as mesas do próprio garçom. */
export function idDoToken(token: string): number {
  return Number(conteudoDoToken(token).sub) || 0;
}
