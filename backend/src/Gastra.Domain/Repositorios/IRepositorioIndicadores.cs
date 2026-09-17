namespace Gastra.Domain.Repositorios;

/// <summary>
/// Leitura das views analíticas (docs/modelagem/GASTRA_Objetos_Banco.md). Só leitura: os valores são
/// calculados pelo banco a cada consulta e nunca gravados.
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
}
