using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

// UC12 — Registrar item do pedido (RF03)
public interface IRegistrarItemPedidoUseCase
{
    Task<ItemPedidoResponse> Executar(int comandaId, ItemPedidoRequest request);
}

public class RegistrarItemPedidoUseCase(
    IRepositorioComanda repositorio,
    IRepositorioItemCardapio repositorioCardapio,
    IUnitOfWork unitOfWork) : IRegistrarItemPedidoUseCase
{
    public async Task<ItemPedidoResponse> Executar(int comandaId, ItemPedidoRequest request)
    {
        ValidadorComanda.ValidarItem(request);

        var comanda = await BuscadorDeComanda.Aberta(repositorio, comandaId);

        var item = await repositorioCardapio.ObterPorId(request.ItemCardapioId)
                   ?? throw new NaoEncontradoException(MensagensErro.ItemCardapioNaoEncontrado);

        if (!item.Disponivel)
            throw new RegraDeNegocioException(MensagensErro.ItemCardapioIndisponivel);

        var pedido = comanda.AdicionarItem(item, request.Quantidade);
        await unitOfWork.Commit();

        return MapeadorComanda.MontarItem(pedido, new Dictionary<int, string> { [item.Id] = item.Nome });
    }
}
