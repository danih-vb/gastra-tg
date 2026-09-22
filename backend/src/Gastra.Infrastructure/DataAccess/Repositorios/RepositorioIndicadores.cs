using Gastra.Domain.Indicadores;
using Gastra.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess.Repositorios;

/// <summary>
/// Consultas diretas às views: não há entidade mapeada, porque ninguém grava nelas. Os parâmetros vão por
/// interpolação do EF (<c>SqlQuery</c>), que vira parâmetro SQL, e nunca por concatenação de texto.
/// </summary>
public class RepositorioIndicadores(GastraDbContext contexto) : IRepositorioIndicadores
{
    public async Task<Dictionary<int, decimal>> ObterFaturamentoMedioPorPraca() =>
        (await contexto.Database
            .SqlQuery<ValorPorId>($"SELECT praca_id AS Id, faturamento_medio_por_turno AS Valor FROM vw_faturamento_medio_praca")
            .ToListAsync())
        .ToDictionary(l => l.Id, l => l.Valor);

    public async Task<Dictionary<int, decimal>> ObterFaturamentoMedioPorTurnoDoGarcom(DateOnly inicio, DateOnly fim) =>
        (await contexto.Database
            .SqlQuery<ValorPorId>($"""
                SELECT garcom_id AS Id, AVG(faturamento) AS Valor
                FROM vw_desempenho_garcom_turno
                WHERE data >= {inicio} AND data < {fim}
                GROUP BY garcom_id
                """)
            .ToListAsync())
        .ToDictionary(l => l.Id, l => l.Valor);

    // Turno e mesa atendida contados pelo par (data, período): a mesma mesa em dois turnos são dois atendimentos.
    public Task<List<IndicadorGarcom>> ObterIndicadoresPorGarcom(DateOnly inicio, DateOnly fim) =>
        contexto.Database.SqlQuery<IndicadorGarcom>($"""
            SELECT garcom_id AS GarcomId,
                   SUM(faturamento) AS Faturamento,
                   COUNT(*) AS Comandas,
                   COUNT(DISTINCT data, periodo) AS Turnos,
                   COUNT(DISTINCT data, periodo, mesa_id) AS MesasAtendidas,
                   CAST(COALESCE(SUM(minutos_atendimento), 0) AS SIGNED) AS SomaMinutosAtendimento
            FROM vw_comanda_faturamento
            WHERE data >= {inicio} AND data < {fim}
            GROUP BY garcom_id
            """).ToListAsync();

    public Task<List<IndicadorPraca>> ObterIndicadoresPorPraca(DateOnly inicio, DateOnly fim) =>
        contexto.Database.SqlQuery<IndicadorPraca>($"""
            SELECT praca_id AS PracaId,
                   SUM(faturamento) AS Faturamento,
                   CAST(SUM(comandas) AS SIGNED) AS Comandas,
                   COUNT(*) AS Turnos
            FROM vw_faturamento_praca_turno
            WHERE data >= {inicio} AND data < {fim}
            GROUP BY praca_id
            """).ToListAsync();

    public Task<List<IndicadorItemCardapio>> ObterIndicadoresPorItem(DateOnly inicio, DateOnly fim) =>
        contexto.Database.SqlQuery<IndicadorItemCardapio>($"""
            SELECT item_cardapio_id AS ItemCardapioId,
                   categoria AS Categoria,
                   SUM(quantidade) AS Quantidade,
                   SUM(faturamento) AS Faturamento
            FROM vw_faturamento_item_cardapio
            WHERE data >= {inicio} AND data < {fim}
            GROUP BY item_cardapio_id, categoria
            """).ToListAsync();

    public Task<List<LinhaAvaliacao>> ObterDistribuicaoDeAvaliacoes(DateOnly inicio, DateOnly fim) =>
        contexto.Database.SqlQuery<LinhaAvaliacao>($"""
            SELECT a.nota AS Nota, COUNT(*) AS Quantidade
            FROM avaliacao_atendimento a
            JOIN comanda c ON c.id = a.comanda_id
            WHERE DATE(CONVERT_TZ(c.data_hora_fechamento, '+00:00', '-03:00')) >= {inicio}
              AND DATE(CONVERT_TZ(c.data_hora_fechamento, '+00:00', '-03:00')) < {fim}
            GROUP BY a.nota
            """).ToListAsync();

    public Task<List<IndicadorPracaNoTempo>> ObterFaturamentoPorPracaEHora(DateOnly inicio, DateOnly fim) =>
        contexto.Database.SqlQuery<IndicadorPracaNoTempo>($"""
            SELECT praca_id AS PracaId, hora AS Fatia, SUM(faturamento) AS Faturamento, COUNT(*) AS Comandas
            FROM vw_comanda_faturamento
            WHERE data >= {inicio} AND data < {fim}
            GROUP BY praca_id, hora
            """).ToListAsync();

    public Task<List<IndicadorPracaNoTempo>> ObterFaturamentoPorPracaEDiaDaSemana(DateOnly inicio, DateOnly fim) =>
        contexto.Database.SqlQuery<IndicadorPracaNoTempo>($"""
            SELECT praca_id AS PracaId, dia_semana AS Fatia, SUM(faturamento) AS Faturamento, COUNT(*) AS Comandas
            FROM vw_comanda_faturamento
            WHERE data >= {inicio} AND data < {fim}
            GROUP BY praca_id, dia_semana
            """).ToListAsync();

    private sealed record ValorPorId(int Id, decimal Valor);
}
