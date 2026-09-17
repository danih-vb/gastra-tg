using Gastra.Domain.Repositorios;

namespace Gastra.Api.Tests.Infraestrutura;

/// <summary>
/// Substitui as views nos testes da API: o banco em memória não tem views. A consulta real é testada contra
/// o MySQL em RepositorioIndicadoresTests.
/// </summary>
public class IndicadoresFalsos : IRepositorioIndicadores
{
    public Dictionary<int, decimal> FaturamentoMedioPorPraca { get; } = [];
    public Dictionary<int, decimal> FaturamentoPorTurnoDoGarcom { get; } = [];
    public List<(DateOnly Inicio, DateOnly Fim)> PeriodosConsultados { get; } = [];

    public Task<Dictionary<int, decimal>> ObterFaturamentoMedioPorPraca() => Task.FromResult(new Dictionary<int, decimal>(FaturamentoMedioPorPraca));

    public Task<Dictionary<int, decimal>> ObterFaturamentoMedioPorTurnoDoGarcom(DateOnly inicio, DateOnly fim)
    {
        PeriodosConsultados.Add((inicio, fim));
        return Task.FromResult(new Dictionary<int, decimal>(FaturamentoPorTurnoDoGarcom));
    }
}
