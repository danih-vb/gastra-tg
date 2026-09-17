using Gastra.Domain.Indicadores;
using Gastra.Domain.Repositorios;

namespace Gastra.Api.Tests.Infraestrutura;

/// <summary>
/// Substitui as views nos testes da API: o banco em memória não tem views. As consultas reais são testadas
/// contra o MySQL em RepositorioIndicadoresTests.
/// </summary>
public class IndicadoresFalsos : IRepositorioIndicadores
{
    public Dictionary<int, decimal> FaturamentoMedioPorPraca { get; } = [];
    public Dictionary<int, decimal> FaturamentoPorTurnoDoGarcom { get; } = [];
    public List<IndicadorGarcom> Garcons { get; } = [];
    public List<IndicadorPraca> Pracas { get; } = [];
    public List<IndicadorItemCardapio> Itens { get; } = [];
    public List<IndicadorPracaNoTempo> PorHora { get; } = [];
    public List<IndicadorPracaNoTempo> PorDiaDaSemana { get; } = [];
    public List<(DateOnly Inicio, DateOnly Fim)> PeriodosConsultados { get; } = [];

    public void Reiniciar()
    {
        FaturamentoMedioPorPraca.Clear();
        FaturamentoPorTurnoDoGarcom.Clear();
        Garcons.Clear();
        Pracas.Clear();
        Itens.Clear();
        PorHora.Clear();
        PorDiaDaSemana.Clear();
        PeriodosConsultados.Clear();
    }

    public Task<Dictionary<int, decimal>> ObterFaturamentoMedioPorPraca() => Task.FromResult(new Dictionary<int, decimal>(FaturamentoMedioPorPraca));

    public Task<Dictionary<int, decimal>> ObterFaturamentoMedioPorTurnoDoGarcom(DateOnly inicio, DateOnly fim) =>
        Registrar(inicio, fim, new Dictionary<int, decimal>(FaturamentoPorTurnoDoGarcom));

    public Task<List<IndicadorGarcom>> ObterIndicadoresPorGarcom(DateOnly inicio, DateOnly fim) => Registrar(inicio, fim, Garcons.ToList());

    public Task<List<IndicadorPraca>> ObterIndicadoresPorPraca(DateOnly inicio, DateOnly fim) => Registrar(inicio, fim, Pracas.ToList());

    public Task<List<IndicadorItemCardapio>> ObterIndicadoresPorItem(DateOnly inicio, DateOnly fim) => Registrar(inicio, fim, Itens.ToList());

    public Task<List<IndicadorPracaNoTempo>> ObterFaturamentoPorPracaEHora(DateOnly inicio, DateOnly fim) => Registrar(inicio, fim, PorHora.ToList());

    public Task<List<IndicadorPracaNoTempo>> ObterFaturamentoPorPracaEDiaDaSemana(DateOnly inicio, DateOnly fim) =>
        Registrar(inicio, fim, PorDiaDaSemana.ToList());

    private Task<T> Registrar<T>(DateOnly inicio, DateOnly fim, T resultado)
    {
        PeriodosConsultados.Add((inicio, fim));
        return Task.FromResult(resultado);
    }
}
