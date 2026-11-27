using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

/// <summary>O garçom confirma ou recusa um item que o Metre ou o Gerente lançou na mesa dele.</summary>
public interface IConfirmarItemUseCase
{
    Task Executar(int comandaId, int itemId, ConfirmacaoItemRequest request);
}

public class ConfirmarItemUseCase(
    IRepositorioComanda repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IConfirmarItemUseCase
{
    public async Task Executar(int comandaId, int itemId, ConfirmacaoItemRequest request)
    {
        var comanda = await BuscadorDeComanda.Aberta(repositorio, comandaId);

        var item = comanda.Itens.FirstOrDefault(i => i.Id == itemId)
                   ?? throw new NaoEncontradoException(MensagensErro.ItemPedidoNaoEncontrado);

        if (item.Status != StatusItemPedido.AConfirmar)
            throw new RegraDeNegocioException(MensagensErro.ItemNaoEstaAConfirmar);

        if (request.Aceitar)
            item.ConfirmarPeloGarcom();
        else
            item.RecusarPeloGarcom();

        await auditoria.Registrar(
            request.Aceitar ? EventoAuditoria.ItemConfirmadoPeloGarcom : EventoAuditoria.ItemRecusadoPeloGarcom,
            alvo: (nameof(ItemDoPedido), item.Id), detalhes: new { ComandaId = comanda.Id });
        await unitOfWork.Commit();
    }
}
