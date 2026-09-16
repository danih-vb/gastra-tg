using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

// RF04 — o cliente pode pedir a remoção da taxa antes do fechamento
public interface IRemoverTaxaServicoUseCase
{
    Task Executar(int comandaId);
}

public class RemoverTaxaServicoUseCase(IRepositorioComanda repositorio, IUnitOfWork unitOfWork)
    : IRemoverTaxaServicoUseCase
{
    public async Task Executar(int comandaId)
    {
        var comanda = await BuscadorDeComanda.Aberta(repositorio, comandaId);

        comanda.RemoverTaxaServico();
        await unitOfWork.Commit();
    }
}
