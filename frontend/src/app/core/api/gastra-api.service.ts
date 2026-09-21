import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { URL_DA_API } from '../configuracao';
import { Papel } from '../sessao/modelos';
import {
  AlocacaoDoTurno,
  CategoriaRestricao,
  Comanda,
  ComandaDoCliente,
  GarcomDoTurno,
  ItemCardapio,
  ItemDoPedido,
  Mesa,
  MotivoCancelamento,
  NovaAvaliacao,
  NovaMesa,
  NovaPraca,
  NovaPromocao,
  NovoItemDoCardapio,
  NovoUsuario,
  PeriodoAlocacao,
  Praca,
  Promocao,
  RankingDeDesempenho,
  RelatorioAvaliacoes,
  RelatorioCardapio,
  RelatorioGarcons,
  RelatorioHorarios,
  RelatorioPracas,
  SugestoesDaComanda,
  Usuario,
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

  /** UC19 — cardápio digital do cliente: só itens disponíveis, sem login (RF05). */
  listarCardapioDigital(): Observable<ItemCardapio[]> {
    return this.http.get<ItemCardapio[]>(`${this.api}/api/cardapio/digital`);
  }

  /**
   * UC20 — conta da mesa pelo código do QR code, sem login (RF13). O código funciona como senha da comanda, por isso
   * não aparece em log nem em tela além da própria conta.
   */
  consultarComandaDoCliente(codigoAcesso: string): Observable<ComandaDoCliente> {
    return this.http.get<ComandaDoCliente>(`${this.api}/api/comandas/consulta/${encodeURIComponent(codigoAcesso)}`);
  }

  /**
   * UC25 — avaliação do atendimento pelo cliente (RF25). É a única escrita que o código de acesso permite, e
   * a API só aceita com a conta fechada, uma vez e dentro do prazo (RN08).
   */
  avaliarAtendimento(codigoAcesso: string, avaliacao: NovaAvaliacao): Observable<void> {
    return this.http.post<void>(
      `${this.api}/api/comandas/consulta/${encodeURIComponent(codigoAcesso)}/avaliacao`,
      avaliacao,
    );
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

  /** UC15 — garçons ativos que o Metre pode marcar como presentes. */
  listarGarconsDoTurno(): Observable<GarcomDoTurno[]> {
    return this.http.get<GarcomDoTurno[]>(`${this.api}/api/alocacoes/garcons`);
  }

  /** UC15 — pede a sugestão da RN03 para os garçons presentes. */
  gerarSugestao(data: string, periodo: PeriodoAlocacao, garcomIds: number[]): Observable<AlocacaoDoTurno> {
    return this.http.post<AlocacaoDoTurno>(`${this.api}/api/alocacoes/sugestao`, { data, periodo, garcomIds });
  }

  /**
   * UC22 — põe um garçom numa praça. Com a praça cheia, `trocarComGarcomId` troca os dois de lugar (#140): é o
   * único jeito de ajustar no turno em que há um garçom para cada vaga.
   */
  ajustarAlocacao(
    data: string,
    periodo: PeriodoAlocacao,
    garcomId: number,
    pracaId: number,
    trocarComGarcomId?: number,
  ): Observable<AlocacaoDoTurno> {
    return this.http.put<AlocacaoDoTurno>(`${this.api}/api/alocacoes/${data}/${periodo}/garcons/${garcomId}`, {
      pracaId,
      trocarComGarcomId: trocarComGarcomId ?? null,
    });
  }

  /** UC21 — confirma o turno; depois disso a alocação não muda mais. */
  confirmarAlocacao(data: string, periodo: PeriodoAlocacao): Observable<AlocacaoDoTurno> {
    return this.http.post<AlocacaoDoTurno>(`${this.api}/api/alocacoes/${data}/${periodo}/confirmacao`, {});
  }

  /** UC17 — para o Garçom, a API já devolve apenas a própria posição. */
  obterDesempenho(): Observable<RankingDeDesempenho> {
    return this.http.get<RankingDeDesempenho>(`${this.api}/api/indicadores/desempenho`);
  }

  // --- Gerente: relatórios (UC16, UC17). Sem datas, a API usa os últimos 30 dias. ---

  relatorioDeGarcons(inicio: string, fim: string): Observable<RelatorioGarcons> {
    return this.http.get<RelatorioGarcons>(`${this.api}/api/indicadores/garcons`, { params: { inicio, fim } });
  }

  relatorioDePracas(inicio: string, fim: string): Observable<RelatorioPracas> {
    return this.http.get<RelatorioPracas>(`${this.api}/api/indicadores/pracas`, { params: { inicio, fim } });
  }

  relatorioDoCardapio(inicio: string, fim: string): Observable<RelatorioCardapio> {
    return this.http.get<RelatorioCardapio>(`${this.api}/api/indicadores/cardapio`, { params: { inicio, fim } });
  }

  relatorioDeAvaliacoes(inicio: string, fim: string): Observable<RelatorioAvaliacoes> {
    return this.http.get<RelatorioAvaliacoes>(`${this.api}/api/indicadores/avaliacoes`, { params: { inicio, fim } });
  }

  relatorioDeHorarios(inicio: string, fim: string): Observable<RelatorioHorarios> {
    return this.http.get<RelatorioHorarios>(`${this.api}/api/indicadores/horarios`, { params: { inicio, fim } });
  }

  /** Para o Gerente a API devolve o ranking inteiro; para o Garçom, só a própria posição. */
  rankingDeDesempenho(inicio: string, fim: string): Observable<RankingDeDesempenho> {
    return this.http.get<RankingDeDesempenho>(`${this.api}/api/indicadores/desempenho`, { params: { inicio, fim } });
  }

  // --- Gerente e Coordenador: cardápio (UC05–UC07) ---

  cadastrarItem(item: NovoItemDoCardapio): Observable<ItemCardapio> {
    return this.http.post<ItemCardapio>(`${this.api}/api/cardapio`, item);
  }

  atualizarPreco(id: number, preco: number): Observable<void> {
    return this.http.patch<void>(`${this.api}/api/cardapio/${id}/preco`, { preco });
  }

  alterarDisponibilidade(id: number, disponivel: boolean): Observable<void> {
    return this.http.patch<void>(`${this.api}/api/cardapio/${id}/disponibilidade`, { disponivel });
  }

  /** RF05 — endereço da foto; vazio tira a foto (#141). */
  alterarImagem(id: number, imagem: string | null): Observable<void> {
    return this.http.patch<void>(`${this.api}/api/cardapio/${id}/imagem`, { imagem });
  }

  // --- Promoções (UC08, UC09) ---

  listarPromocoes(): Observable<Promocao[]> {
    return this.http.get<Promocao[]>(`${this.api}/api/promocoes`);
  }

  criarPromocao(promocao: NovaPromocao): Observable<Promocao> {
    return this.http.post<Promocao>(`${this.api}/api/promocoes`, promocao);
  }

  removerPromocao(id: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/api/promocoes/${id}`);
  }

  // --- Gerente: salão (UC24) ---

  criarPraca(praca: NovaPraca): Observable<Praca> {
    return this.http.post<Praca>(`${this.api}/api/pracas`, praca);
  }

  editarPraca(id: number, praca: NovaPraca): Observable<Praca> {
    return this.http.put<Praca>(`${this.api}/api/pracas/${id}`, praca);
  }

  criarMesa(mesa: NovaMesa): Observable<Mesa> {
    return this.http.post<Mesa>(`${this.api}/api/mesas`, mesa);
  }

  /** A praça não entra: o vínculo mesa–praça é fixo, senão o histórico por praça perderia o sentido (REL01). */
  editarMesa(id: number, numero: string, capacidade: number): Observable<Mesa> {
    return this.http.put<Mesa>(`${this.api}/api/mesas/${id}`, { numero, capacidade });
  }

  // --- Gerente: contas de usuário (UC04) ---

  listarUsuarios(): Observable<Usuario[]> {
    return this.http.get<Usuario[]>(`${this.api}/api/usuarios`);
  }

  cadastrarUsuario(usuario: NovoUsuario): Observable<Usuario> {
    return this.http.post<Usuario>(`${this.api}/api/usuarios`, usuario);
  }

  editarUsuario(id: number, nome: string, email: string, papel: Papel): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.api}/api/usuarios/${id}`, { nome, email, papel });
  }

  /** RF18 — inativar não apaga: a conta perde o acesso na hora e o histórico continua. */
  alterarSituacaoDoUsuario(id: number, ativo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.api}/api/usuarios/${id}/situacao`, { ativo });
  }

  /** UC04 (#141) — o Gerente define uma senha nova para outra conta; as sessões dela caem. */
  redefinirSenha(id: number, senha: string): Observable<void> {
    return this.http.post<void>(`${this.api}/api/usuarios/${id}/senha`, { senha });
  }

  /** UC04 (#141) — zera a vinculação do autenticador de quem perdeu o celular (RN07). */
  reiniciarSegundoFator(id: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/api/usuarios/${id}/segundo-fator`);
  }
}
