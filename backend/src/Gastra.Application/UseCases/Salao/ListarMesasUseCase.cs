using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Salao;

// O garçom precisa desta lista para abrir a comanda (UC10).
public interface IListarMesasUseCase
{
    Task<List<MesaResponse>> Executar();
}

public class ListarMesasUseCase(IRepositorioMesa repositorio, IRepositorioPraca repositorioPraca)
    : IListarMesasUseCase
{
    public async Task<List<MesaResponse>> Executar()
    {
        var mesas = await repositorio.ListarTodas();
        var pracas = (await repositorioPraca.ListarTodas()).ToDictionary(p => p.Id, p => p.Codigo);

        return mesas.Select(m => new MesaResponse
        {
            Id = m.Id,
            Numero = m.Numero,
            Capacidade = m.Capacidade,
            PracaId = m.PracaId,
            PracaCodigo = pracas.TryGetValue(m.PracaId, out var codigo) ? codigo : string.Empty,
        }).ToList();
    }
}
