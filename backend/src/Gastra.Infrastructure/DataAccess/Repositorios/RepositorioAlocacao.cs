using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess.Repositorios;

public class RepositorioAlocacao(GastraDbContext contexto) : IRepositorioAlocacao
{
    public async Task Adicionar(Alocacao alocacao) => await contexto.Alocacoes.AddAsync(alocacao);

    public async Task<List<Alocacao>> ListarDoTurno(DateOnly data, PeriodoAlocacao periodo) =>
        await contexto.Alocacoes.Where(a => a.Data == data && a.Periodo == periodo).OrderBy(a => a.Id).ToListAsync();

    public void Remover(IEnumerable<Alocacao> alocacoes) => contexto.Alocacoes.RemoveRange(alocacoes);

    public async Task<Dictionary<int, List<int>>> ListarPracasConfirmadasAntesDe(IEnumerable<int> garcomIds, DateOnly data)
    {
        var ids = garcomIds.Distinct().ToList();

        var linhas = await contexto.Alocacoes.AsNoTracking()
            .Where(a => a.Confirmada && a.Data < data && ids.Contains(a.GarcomId))
            .Select(a => new { a.GarcomId, a.PracaId, a.Data, a.Periodo })
            .ToListAsync();

        // Mais recente primeiro: data decrescente e, no mesmo dia, jantar antes do almoço.
        return ids.ToDictionary(
            id => id,
            id => linhas.Where(l => l.GarcomId == id)
                .OrderByDescending(l => l.Data).ThenByDescending(l => l.Periodo)
                .Select(l => l.PracaId).ToList());
    }
}
