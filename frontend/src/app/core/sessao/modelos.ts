/** Papéis de usuário da API (PapelUsuario). */
export type Papel = 'Gerente' | 'Coordenador' | 'Metre' | 'Garcom';

/** Resposta de POST /api/autenticacao/login e /segundo-fator/confirmar (LoginResponse). */
export interface RespostaDeLogin {
  tokenAcesso: string | null;
  nome: string | null;
  papel: Papel | null;
  requerSegundoFator: boolean;
  requerConfiguracaoSegundoFator: boolean;
  tokenSegundoFator: string | null;
}

/** Resposta de POST /api/autenticacao/segundo-fator/configurar. Aparece uma única vez (RN07). */
export interface ConfiguracaoDoSegundoFator {
  uriConfiguracao: string;
  chaveManual: string;
}

/** Formato único de erro da API (ErroResponse). */
export interface RespostaDeErro {
  erros: string[];
}

/** Quem está logado neste navegador. */
export interface UsuarioDaSessao {
  token: string;
  nome: string;
  papel: Papel;
  /** Instante, em milissegundos, em que o token deixa de valer. */
  expiraEm: number;
}

/** Para onde o login leva: direto ao sistema ou a uma das etapas do segundo fator (RF16). */
export type ResultadoDoLogin = 'acesso-liberado' | 'confirmar-segundo-fator' | 'configurar-segundo-fator';
