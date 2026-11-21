using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

/// <summary>
/// Metre ou Gerente lança um item de cortesia numa comanda aberta: entra na conta por R$ 0,00. Vale para item
/// indisponível no cardápio, que é como o restaurante cadastra o que não vende e só oferece.
/// </summary>
public interface ILancarCortesiaUseCase
{
    Task<ItemPedidoResponse> Executar(int comandaId, CortesiaRequest request);
}

public class LancarCortesiaUseCase(
    IRepositorioComanda repositorio,
    IRepositorioItemCardapio repositorioCardapio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : ILancarCortesiaUseCase
{
    public async Task<ItemPedidoResponse> Executar(int comandaId, CortesiaRequest request)
    {
        ValidadorComanda.ValidarCortesia(request);

        var comanda = await BuscadorDeComanda.Aberta(repositorio, comandaId);

        var item = await repositorioCardapio.ObterPorId(request.ItemCardapioId)
                   ?? throw new NaoEncontradoException(MensagensErro.ItemCardapioNaoEncontrado);

        var pedido = comanda.AdicionarCortesia(item, request.Quantidade, request.Motivo);

        // O motivo é texto livre e pode citar alguém ("aniversário da Ana"): fica na comanda, não no log.
        await auditoria.Registrar(EventoAuditoria.CortesiaLancada, alvo: (nameof(Comanda), comanda.Id),
            detalhes: new { ItemCardapioId = item.Id, pedido.Quantidade, ValorOferecido = item.Preco * pedido.Quantidade });
        await unitOfWork.Commit();

        return MapeadorComanda.MontarItem(pedido, new Dictionary<int, string> { [item.Id] = item.Nome });
    }
}
