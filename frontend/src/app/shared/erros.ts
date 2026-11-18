import { HttpErrorResponse } from '@angular/common/http';
import { RespostaDeErro } from '../core/sessao/modelos';

/** Mensagens de erro para mostrar ao usuário: as da API quando existem (já traduzidas), senão uma genérica. */
export function mensagensDeErro(erro: unknown): string[] {
  if (erro instanceof HttpErrorResponse) {
    const corpo = erro.error as Partial<RespostaDeErro> | null;
    if (corpo?.erros?.length) {
      return corpo.erros;
    }
    if (erro.status === 0) {
      return ['Não foi possível falar com o servidor. Verifique a conexão e tente de novo.'];
    }
  }
  return ['Algo deu errado. Tente de novo.'];
}
