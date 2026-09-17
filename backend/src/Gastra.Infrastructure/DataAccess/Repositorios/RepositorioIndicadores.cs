using Gastra.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess.Repositorios;

/// <summary>Consultas diretas às views: não há entidade mapeada, porque ninguém grava nelas.</summary>
public class RepositorioIndicadores(GastraDbContext contexto) : IRepositorioIndicadores
{
    public async Task<Dictionary<int, decimal>> ObterFaturamentoMedioPorPraca() =>
        (await contexto.Database
            .SqlQuery<ValorPorId>($"SELECT praca_id AS Id, faturamento_medio_por_turno AS Valor FROM vw_faturamento_medio_praca")
            .ToListAsync())
        .ToDictionary(l => l.Id, l => l.Valor);

    public async Task<Dictionary<int, decimal>> ObterFaturamentoPorGarcom(DateOnly inicio, DateOnly fim) =>
        (await contexto.Database
            .SqlQuery<ValorPorId>($"""
                SELECT garcom_id AS Id, SUM(faturamento) AS Valor
                FROM vw_desempenho_garcom_turno
                WHERE data >= {inicio} AND data < {fim}
                GROUP BY garcom_id
                """)
            .ToListAsync())
        .ToDictionary(l => l.Id, l => l.Valor);

    private sealed record ValorPorId(int Id, decimal Valor);
}
