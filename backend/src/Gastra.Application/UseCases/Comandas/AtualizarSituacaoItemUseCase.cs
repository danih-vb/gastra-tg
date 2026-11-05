using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;
using Mapster;
using ComunicacaoEnums = Gastra.Communication.Enums;

namespace Gastra.Application.UseCases.Comandas;

// UC23 — Entregar ou cancelar item do pedido (RF24, RN02)
public interface IAtualizarSituacaoItemUseCase
{
    Task Executar(int comandaId, int itemId, SituacaoItemRequest request);
}

public class AtualizarSituacaoItemUseCase(
    IRepositorioComanda repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork)
    : IAtualizarSituacaoItemUseCase
{
    public async Task Executar(int comandaId, int itemId, SituacaoItemRequest request)
    {
        ValidadorComanda.ValidarSituacao(request);

        var comanda = await BuscadorDeComanda.Aberta(repositorio, comandaId);

        var item = comanda.Itens.FirstOrDefault(i => i.Id == itemId)
                   ?? throw new NaoEncontradoException(MensagensErro.ItemPedidoNaoEncontrado);

        if (item.Status != StatusItemPedido.Pendente)
            throw new RegraDeNegocioException(MensagensErro.ItemNaoEstaPendente);

        if (request.Situacao == ComunicacaoEnums.StatusItemPedido.Entregue)
            item.MarcarEntregue();
        else
        {
            item.Cancelar(request.MotivoCancelamento!.Value.Adapt<MotivoCancelamento>());

            await auditoria.Registrar(EventoAuditoria.ItemCancelado, alvo: (nameof(ItemDoPedido), item.Id),
                detalhes: new { ComandaId = comanda.Id, Motivo = item.MotivoCancelamento });
        }

        await unitOfWork.Commit();
    }
}
