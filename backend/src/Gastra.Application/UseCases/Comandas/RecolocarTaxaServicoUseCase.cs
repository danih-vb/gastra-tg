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

// RF04 — o cliente pode mudar de ideia e voltar a pagar a taxa antes do fechamento
public interface IRecolocarTaxaServicoUseCase
{
    Task Executar(int comandaId);
}

public class RecolocarTaxaServicoUseCase(
    IRepositorioComanda repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork)
    : IRecolocarTaxaServicoUseCase
{
    public async Task Executar(int comandaId)
    {
        var comanda = await BuscadorDeComanda.Aberta(repositorio, comandaId);

        comanda.RecolocarTaxaServico();

        await auditoria.Registrar(EventoAuditoria.TaxaServicoRecolocada, alvo: (nameof(Comanda), comanda.Id));
        await unitOfWork.Commit();
    }
}
