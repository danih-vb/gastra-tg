using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess.Repositorios;

public class RepositorioDestaque(GastraDbContext contexto) : IRepositorioDestaque
{
    // O banco guarda o horário em UTC; o dia do restaurante é o de Brasília (UTC-3).
    private static readonly TimeSpan FusoDoRestaurante = TimeSpan.FromHours(3);

    public async Task Adicionar(DestaqueDoCardapio destaque) => await contexto.Destaques.AddAsync(destaque);

    public async Task<DestaqueDoCardapio?> ObterPorId(int id) => await contexto.Destaques.FirstOrDefaultAsync(d => d.Id == id);

    public void Remover(DestaqueDoCardapio destaque) => contexto.Destaques.Remove(destaque);

    public async Task<List<DestaqueDoCardapio>> ListarAPartirDe(DateOnly data) =>
        await contexto.Destaques.AsNoTracking()
            .Where(d => d.DataFim >= data)
            .OrderBy(d => d.DataInicio).ThenBy(d => d.Tipo)
            .ToListAsync();

    public async Task<int> QuantidadeVendida(int itemDoCardapioId, DateOnly inicio, DateOnly fim)
    {
        var de = inicio.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) + FusoDoRestaurante;
        var ate = fim.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) + FusoDoRestaurante;

        return await contexto.Comandas.AsNoTracking()
            .Where(c => c.DataHoraAbertura >= de && c.DataHoraAbertura < ate)
            .SelectMany(c => c.Itens)
            .Where(i => i.ItemDoCardapioId == itemDoCardapioId && !i.Cortesia &&
                        (i.Status == StatusItemPedido.Pendente || i.Status == StatusItemPedido.Entregue))
            .SumAsync(i => i.Quantidade);
    }
}
