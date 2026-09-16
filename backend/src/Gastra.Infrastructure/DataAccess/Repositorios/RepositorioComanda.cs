using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess.Repositorios;

public class RepositorioComanda(GastraDbContext contexto) : IRepositorioComanda
{
    public async Task Adicionar(Comanda comanda) => await contexto.Comandas.AddAsync(comanda);

    public async Task<Comanda?> ObterPorId(int id) =>
        await contexto.Comandas
            .Include(c => c.Itens)
            .Include(c => c.Restricoes)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Comanda?> ObterPorCodigoAcesso(string codigoAcesso) =>
        await contexto.Comandas
            .Include(c => c.Itens)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CodigoAcessoCliente == codigoAcesso);

    public async Task<List<Comanda>> ListarAbertas() =>
        await contexto.Comandas
            .Include(c => c.Itens)
            .AsNoTracking()
            .Where(c => c.Status == StatusComanda.Aberta)
            .OrderBy(c => c.DataHoraAbertura)
            .ToListAsync();

    public async Task<bool> ExisteAbertaNaMesa(int mesaId) =>
        await contexto.Comandas.AnyAsync(c => c.MesaId == mesaId && c.Status == StatusComanda.Aberta);
}
