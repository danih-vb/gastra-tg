using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess.Repositorios;

public class RepositorioPraca(GastraDbContext contexto) : IRepositorioPraca
{
    public async Task Adicionar(Praca praca) => await contexto.Pracas.AddAsync(praca);

    public async Task<Praca?> ObterPorId(int id) => await contexto.Pracas.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Praca?> ObterPorCodigo(string codigo) =>
        await contexto.Pracas.FirstOrDefaultAsync(p => p.Codigo == codigo.Trim());

    public async Task<List<Praca>> ListarTodas() =>
        await contexto.Pracas.AsNoTracking().OrderBy(p => p.Codigo).ToListAsync();
}
