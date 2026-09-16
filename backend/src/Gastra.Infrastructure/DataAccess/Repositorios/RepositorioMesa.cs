using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess.Repositorios;

public class RepositorioMesa(GastraDbContext contexto) : IRepositorioMesa
{
    public async Task Adicionar(Mesa mesa) => await contexto.Mesas.AddAsync(mesa);

    public async Task<Mesa?> ObterPorId(int id) => await contexto.Mesas.FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<Mesa>> ListarTodas() =>
        await contexto.Mesas.AsNoTracking().OrderBy(m => m.Numero).ToListAsync();
}
