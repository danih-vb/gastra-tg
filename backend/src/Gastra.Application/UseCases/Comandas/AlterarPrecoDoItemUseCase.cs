using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

/// <summary>O Gerente muda o preço de um item já lançado numa comanda aberta, com motivo; o preço original fica guardado.</summary>
public interface IAlterarPrecoDoItemUseCase
{
    Task<ItemPedidoResponse> Executar(int comandaId, int itemId, AlterarPrecoItemRequest request);
}

public class AlterarPrecoDoItemUseCase(
    IRepositorioComanda repositorio,
    IRepositorioItemCardapio repositorioCardapio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IAlterarPrecoDoItemUseCase
{
    public async Task<ItemPedidoResponse> Executar(int comandaId, int itemId, AlterarPrecoItemRequest request)
    {
        ValidadorComanda.ValidarAlteracaoDePreco(request);

        var comanda = await BuscadorDeComanda.Aberta(repositorio, comandaId);

        var pedido = comanda.Itens.SingleOrDefault(i => i.Id == itemId)
                     ?? throw new NaoEncontradoException(MensagensErro.ItemPedidoNaoEncontrado);

        if (pedido.Status == StatusItemPedido.Cancelado)
            throw new RegraDeNegocioException(MensagensErro.ItemCanceladoSemPreco);

        if (pedido.PrecoUnitarioNoMomento == request.Preco)
            throw new RegraDeNegocioException(MensagensErro.PrecoAjustadoIgualAoAtual);

        var precoAnterior = pedido.PrecoUnitarioNoMomento;
        comanda.AlterarPrecoDoItem(itemId, request.Preco, request.Motivo);

        await auditoria.Registrar(EventoAuditoria.PrecoDoItemAlterado, alvo: (nameof(Comanda), comanda.Id),
            detalhes: new { ItemPedidoId = pedido.Id, PrecoAnterior = precoAnterior, PrecoNovo = request.Preco });
        await unitOfWork.Commit();

        var item = await repositorioCardapio.ObterPorId(pedido.ItemDoCardapioId);
        return MapeadorComanda.MontarItem(pedido, new Dictionary<int, string> { [pedido.ItemDoCardapioId] = item?.Nome ?? string.Empty });
    }
}
