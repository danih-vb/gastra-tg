import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { URL_DA_API } from '../configuracao';
import {
  AlocacaoDoTurno,
  CategoriaRestricao,
  Comanda,
  ItemCardapio,
  ItemDoPedido,
  Mesa,
  MotivoCancelamento,
  PeriodoAlocacao,
  Praca,
  RankingDeDesempenho,
  SugestoesDaComanda,
} from './modelos';

/**
 * Chamadas à API usadas pelas telas do Garçom (UC10–UC14, UC17, UC18, UC23). Cada método é uma rota do backend,
 * sem regra de negócio: a regra fica no servidor, e a tela só apresenta o resultado.
 */
@Injectable({ providedIn: 'root' })
export class GastraApiService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(URL_DA_API);

  // --- Salão e cardápio ---

  listarPracas(): Observable<Praca[]> {
    return this.http.get<Praca[]>(`${this.api}/api/pracas`);
  }

  listarMesas(): Observable<Mesa[]> {
    return this.http.get<Mesa[]>(`${this.api}/api/mesas`);
  }

  listarCardapio(): Observable<ItemCardapio[]> {
    return this.http.get<ItemCardapio[]>(`${this.api}/api/cardapio`);
  }

  // --- Comandas ---

  listarComandasAbertas(): Observable<Comanda[]> {
    return this.http.get<Comanda[]>(`${this.api}/api/comandas`);
  }

  obterComanda(id: number): Observable<Comanda> {
    return this.http.get<Comanda>(`${this.api}/api/comandas/${id}`);
  }

  /** UC10 — abre a comanda; a composição da mesa vem sugerida pela quantidade de pessoas (RN01). */
  abrirComanda(mesaId: number, quantidadePessoas: number): Observable<Comanda> {
    return this.http.post<Comanda>(`${this.api}/api/comandas`, { mesaId, quantidadePessoas });
  }

  /** UC11 — confirma ou ajusta a composição sugerida. */
  ajustarComposicao(comandaId: number, quantidadePessoas: number, composicao: string): Observable<Comanda> {
    return this.http.patch<Comanda>(`${this.api}/api/comandas/${comandaId}/composicao`, {
      quantidadePessoas,
      composicao,
    });
  }

  /** UC12 — lança um item do cardápio na comanda. */
  registrarItem(comandaId: number, itemCardapioId: number, quantidade: number): Observable<ItemDoPedido> {
    return this.http.post<ItemDoPedido>(`${this.api}/api/comandas/${comandaId}/itens`, {
      itemCardapioId,
      quantidade,
    });
  }

  /** UC23 — marca o item como entregue ou cancela com motivo da lista fechada (RN02). */
  atualizarSituacaoDoItem(
    comandaId: number,
    itemId: number,
    situacao: 'Entregue' | 'Cancelado',
    motivoCancelamento?: MotivoCancelamento,
  ): Observable<ItemDoPedido> {
    return this.http.patch<ItemDoPedido>(`${this.api}/api/comandas/${comandaId}/itens/${itemId}/situacao`, {
      situacao,
      motivoCancelamento: motivoCancelamento ?? null,
    });
  }

  /** UC13 — restrição informada pelo cliente (RF14). A observação é apagada no fechamento. */
  registrarRestricao(comandaId: number, categoria: CategoriaRestricao, observacaoLivre: string | null): Observable<Comanda> {
    return this.http.post<Comanda>(`${this.api}/api/comandas/${comandaId}/restricoes`, {
      categoria,
      observacaoLivre,
    });
  }

  /** RF04 — tira a taxa de serviço a pedido do cliente. */
  removerTaxaDeServico(comandaId: number): Observable<Comanda> {
    return this.http.delete<Comanda>(`${this.api}/api/comandas/${comandaId}/taxa-servico`);
  }

  /** UC14 — fecha a comanda; a API recusa se houver item pendente (RN02). */
  fecharComanda(comandaId: number): Observable<Comanda> {
    return this.http.post<Comanda>(`${this.api}/api/comandas/${comandaId}/fechamento`, {});
  }

  /** UC18 — itens que costumam sair junto com o que já está na mesa (RF09). */
  obterSugestoes(comandaId: number): Observable<SugestoesDaComanda> {
    return this.http.get<SugestoesDaComanda>(`${this.api}/api/comandas/${comandaId}/sugestoes`);
  }

  // --- Turno e desempenho ---

  /** Alocação do turno: é ela que diz qual praça é a do garçom hoje (RF07). */
  obterAlocacao(data: string, periodo: PeriodoAlocacao): Observable<AlocacaoDoTurno> {
    return this.http.get<AlocacaoDoTurno>(`${this.api}/api/alocacoes/${data}/${periodo}`);
  }

  /** UC17 — para o Garçom, a API já devolve apenas a própria posição. */
  obterDesempenho(): Observable<RankingDeDesempenho> {
    return this.http.get<RankingDeDesempenho>(`${this.api}/api/indicadores/desempenho`);
  }
}
