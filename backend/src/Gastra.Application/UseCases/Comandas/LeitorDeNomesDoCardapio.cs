using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;

namespace Gastra.Application.UseCases.Comandas;

/// <summary>Busca de uma vez o nome dos itens do cardápio usados na comanda, evitando uma consulta por item.</summary>
internal static class LeitorDeNomesDoCardapio
{
    public static async Task<IReadOnlyDictionary<int, string>> Obter(IRepositorioItemCardapio repositorio, Comanda comanda)
    {
        var ids = comanda.Itens.Select(i => i.ItemDoCardapioId).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, string>();

        var itens = await repositorio.ListarPorIds(ids);
        return itens.ToDictionary(i => i.Id, i => i.Nome);
    }
}
