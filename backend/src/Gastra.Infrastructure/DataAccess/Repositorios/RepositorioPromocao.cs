using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess.Repositorios;

public class RepositorioPromocao(GastraDbContext contexto) : IRepositorioPromocao
{
    public async Task Adicionar(Promocao promocao) => await contexto.Promocoes.AddAsync(promocao);

    public async Task<Promocao?> ObterPorId(int id) => await contexto.Promocoes.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Promocao>> Listar(bool somenteAtivas) =>
        await contexto.Promocoes.AsNoTracking()
            .Where(p => !somenteAtivas || p.Ativa)
            .OrderByDescending(p => p.DataInicio).ThenBy(p => p.Descricao)
            .ToListAsync();

    public async Task<List<Promocao>> ListarVigentes(DateOnly data) =>
        await contexto.Promocoes.AsNoTracking()
            .Where(p => p.Ativa && p.DataInicio <= data && p.DataFim >= data)
            .ToListAsync();
}
