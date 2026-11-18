import { Papel, RespostaDeLogin } from './modelos';

/** Armazenamento em memória no lugar do sessionStorage, para cada teste começar limpo. */
export class ArmazenamentoEmMemoria implements Storage {
  private readonly dados = new Map<string, string>();

  get length(): number {
    return this.dados.size;
  }

  clear(): void {
    this.dados.clear();
  }

  getItem(chave: string): string | null {
    return this.dados.get(chave) ?? null;
  }

  key(indice: number): string | null {
    return [...this.dados.keys()][indice] ?? null;
  }

  removeItem(chave: string): void {
    this.dados.delete(chave);
  }

  setItem(chave: string, valor: string): void {
    this.dados.set(chave, valor);
  }
}

/** JWT falso, só com o "exp" (a assinatura não importa para o navegador). */
export function tokenQueExpiraEm(segundosAPartirDeAgora: number): string {
  const conteudo = btoa(JSON.stringify({ exp: Math.floor(Date.now() / 1000) + segundosAPartirDeAgora }))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '');
  return `cabecalho.${conteudo}.assinatura`;
}

export function loginLiberado(papel: Papel, nome = 'Ana'): RespostaDeLogin {
  return {
    tokenAcesso: tokenQueExpiraEm(3600),
    nome,
    papel,
    requerSegundoFator: false,
    requerConfiguracaoSegundoFator: false,
    tokenSegundoFator: null,
  };
}
