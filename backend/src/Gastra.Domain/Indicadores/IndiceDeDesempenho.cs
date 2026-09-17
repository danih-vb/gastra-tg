namespace Gastra.Domain.Indicadores;

/// <summary>Base do índice de um garçom no período, lida das views.</summary>
public record DesempenhoNoPeriodo(int GarcomId, decimal Faturamento, int Turnos, int MesasAtendidas);

/// <summary>Posição de um garçom no ranking do período.</summary>
public record PosicaoNoRanking(int GarcomId, int Posicao, decimal Indice, decimal FaturamentoPorTurno, decimal MesasPorTurno, int Turnos);

/// <summary>
/// RF11 — índice composto de desempenho: combina faturamento e mesas atendidas, "não apenas venda".
/// </summary>
/// <remarks>
/// <para>
/// As duas partes são <b>por turno trabalhado</b>. Com totais, quem trabalhou mais turnos ficaria na frente só por
/// ter trabalhado mais; é o mesmo problema que a calibração da RN03 mostrou na alocação (#55).
/// </para>
/// <para>
/// Cada parte é dividida pelo maior valor do período e vai de 0 a 100. O índice é a soma ponderada das duas.
/// Assim o número é comparável dentro do período, mas não entre períodos diferentes.
/// </para>
/// </remarks>
public static class IndiceDeDesempenho
{
    /// <summary>Peso do faturamento; o das mesas atendidas é o complemento. Metade e metade: nenhum dos dois domina.</summary>
    public const decimal PesoFaturamento = 0.5m;

    public static IReadOnlyList<PosicaoNoRanking> Calcular(IEnumerable<DesempenhoNoPeriodo> desempenhos)
    {
        var comTurnos = desempenhos.Where(d => d.Turnos > 0)
            .Select(d => (d.GarcomId, d.Turnos, Faturamento: d.Faturamento / d.Turnos, Mesas: (decimal)d.MesasAtendidas / d.Turnos))
            .ToList();

        if (comTurnos.Count == 0)
            return [];

        var maiorFaturamento = comTurnos.Max(d => d.Faturamento);
        var maiorMesas = comTurnos.Max(d => d.Mesas);

        var ordenados = comTurnos
            .Select(d => (d.GarcomId, d.Turnos, d.Faturamento, d.Mesas,
                Indice: Math.Round(100 * (PesoFaturamento * Proporcao(d.Faturamento, maiorFaturamento)
                                          + (1 - PesoFaturamento) * Proporcao(d.Mesas, maiorMesas)), 1)))
            .OrderByDescending(d => d.Indice).ThenBy(d => d.GarcomId)
            .ToList();

        // Empate no índice divide a posição (1, 1, 3), como em qualquer classificação.
        return ordenados
            .Select(d => new PosicaoNoRanking(
                d.GarcomId,
                ordenados.Count(outro => outro.Indice > d.Indice) + 1,
                d.Indice,
                Math.Round(d.Faturamento, 2),
                Math.Round(d.Mesas, 2),
                d.Turnos))
            .ToList();
    }

    private static decimal Proporcao(decimal valor, decimal maior) => maior == 0 ? 0 : valor / maior;
}
