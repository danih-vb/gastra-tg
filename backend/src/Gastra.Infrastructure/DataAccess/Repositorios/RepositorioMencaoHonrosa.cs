using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess.Repositorios;

public class RepositorioMencaoHonrosa(GastraDbContext contexto) : IRepositorioMencaoHonrosa
{
    public async Task Adicionar(MencaoHonrosa mencao) => await contexto.MencoesHonrosas.AddAsync(mencao);

    public async Task<MencaoHonrosa?> ObterPorId(int id) => await contexto.MencoesHonrosas.FirstOrDefaultAsync(m => m.Id == id);

    public void Remover(MencaoHonrosa mencao) => contexto.MencoesHonrosas.Remove(mencao);

    public async Task<List<MencaoHonrosa>> ListarDoGarcom(int garcomId) =>
        await contexto.MencoesHonrosas.AsNoTracking()
            .Where(m => m.GarcomId == garcomId)
            .OrderByDescending(m => m.Mes).ThenByDescending(m => m.Id)
            .ToListAsync();
}
