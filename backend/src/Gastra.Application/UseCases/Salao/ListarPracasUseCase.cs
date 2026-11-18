using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Salao;

public interface IListarPracasUseCase
{
    Task<List<PracaResponse>> Executar();
}

public class ListarPracasUseCase(IRepositorioPraca repositorio) : IListarPracasUseCase
{
    public async Task<List<PracaResponse>> Executar()
    {
        var pracas = await repositorio.ListarTodas();
        return pracas.Select(p => new PracaResponse
        {
            Id = p.Id,
            Codigo = p.Codigo,
            QuantidadeGarcons = p.QuantidadeGarcons,
        }).ToList();
    }
}
