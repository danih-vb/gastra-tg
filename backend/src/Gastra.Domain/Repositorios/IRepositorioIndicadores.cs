using Gastra.Domain.Indicadores;

namespace Gastra.Domain.Repositorios;

/// <summary>
/// Leitura das views analíticas (docs/modelagem/GASTRA_Objetos_Banco.md). Só leitura: os valores são
/// calculados pelo banco a cada consulta e nunca gravados. Nos períodos, o fim é exclusivo: [inicio, fim).
/// </summary>
public interface IRepositorioIndicadores
{
    /// <summary>Faturamento médio por turno de cada praça (<c>vw_faturamento_medio_praca</c>), inclusive as sem movimento.</summary>
    Task<Dictionary<int, decimal>> ObterFaturamentoMedioPorPraca();

    /// <summary>
    /// Faturamento médio por turno trabalhado de cada garçom, com data no intervalo [inicio, fim)
    /// (<c>vw_desempenho_garcom_turno</c>: uma linha por turno em que o garçom fechou comanda).
    /// </summary>
    Task<Dictionary<int, decimal>> ObterFaturamentoMedioPorTurnoDoGarcom(DateOnly inicio, DateOnly fim);

    /// <summary>RF10 — faturamento, comandas, turnos, mesas e minutos de atendimento por garçom.</summary>
    Task<List<IndicadorGarcom>> ObterIndicadoresPorGarcom(DateOnly inicio, DateOnly fim);

    /// <summary>RF10 — faturamento, comandas e turnos com movimento por praça.</summary>
    Task<List<IndicadorPraca>> ObterIndicadoresPorPraca(DateOnly inicio, DateOnly fim);

    /// <summary>Faturamento e quantidade por item do cardápio, com a categoria.</summary>
    Task<List<IndicadorItemCardapio>> ObterIndicadoresPorItem(DateOnly inicio, DateOnly fim);

    /// <summary>Faturamento por praça e hora do dia (horário de Brasília).</summary>
    Task<List<IndicadorPracaNoTempo>> ObterFaturamentoPorPracaEHora(DateOnly inicio, DateOnly fim);

    /// <summary>Faturamento por praça e dia da semana (1 = domingo).</summary>
    Task<List<IndicadorPracaNoTempo>> ObterFaturamentoPorPracaEDiaDaSemana(DateOnly inicio, DateOnly fim);
}
