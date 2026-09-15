using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess.Repositorios;

public class RepositorioItemCardapio(GastraDbContext contexto) : IRepositorioItemCardapio
{
    public async Task Adicionar(ItemDoCardapio item) => await contexto.ItensCardapio.AddAsync(item);

    public async Task<ItemDoCardapio?> ObterPorId(int id) =>
        await contexto.ItensCardapio.FirstOrDefaultAsync(i => i.Id == id);

    public async Task<List<ItemDoCardapio>> ListarDisponiveis() =>
        await contexto.ItensCardapio.AsNoTracking()
            .Where(i => i.Disponivel)
            .OrderBy(i => i.Categoria).ThenBy(i => i.Nome)
            .ToListAsync();

    public async Task<List<ItemDoCardapio>> BuscarPorNome(string nome) =>
        await contexto.ItensCardapio.AsNoTracking()
            .Where(i => i.Nome.Contains(nome))
            .OrderBy(i => i.Nome)
            .ToListAsync();
}
