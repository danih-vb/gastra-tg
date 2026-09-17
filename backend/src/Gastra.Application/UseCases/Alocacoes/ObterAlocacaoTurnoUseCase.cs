using Gastra.Communication.Responses;
using Gastra.Domain.Repositorios;
using ComunicacaoEnums = Gastra.Communication.Enums;

namespace Gastra.Application.UseCases.Alocacoes;

// Consulta de quem está em qual praça no turno (salão inteiro)
public interface IObterAlocacaoTurnoUseCase
{
    Task<AlocacaoTurnoResponse> Executar(DateOnly data, ComunicacaoEnums.PeriodoAlocacao periodo);
}

public class ObterAlocacaoTurnoUseCase(
    IRepositorioAlocacao repositorio,
    IRepositorioUsuario repositorioUsuario,
    IRepositorioPraca repositorioPraca) : IObterAlocacaoTurnoUseCase
{
    public async Task<AlocacaoTurnoResponse> Executar(DateOnly data, ComunicacaoEnums.PeriodoAlocacao periodoRequest)
    {
        var periodo = LeitorDoTurno.Periodo(periodoRequest);
        var turno = await repositorio.ListarDoTurno(data, periodo);
        return await LeitorDoTurno.Montar(data, periodo, turno, repositorioUsuario, repositorioPraca);
    }
}
