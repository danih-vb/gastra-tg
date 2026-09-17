using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;

namespace Gastra.Application.UseCases.Comandas;

/// <summary>
/// Nome do garçom de cada comanda, buscado de uma vez só. O mapa de mesas do garçom e o painel do salão do
/// metre precisam saber quem atende cada mesa (#141); é dado da equipe, não do cliente, e não entra na RN04.
/// </summary>
internal static class LeitorDeNomesDeGarcons
{
    public static async Task<IReadOnlyDictionary<int, string>> Obter(
        IRepositorioUsuario repositorio, IEnumerable<Comanda> comandas)
    {
        var ids = comandas.Select(c => c.GarcomId).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, string>();

        var garcons = await repositorio.ListarPorIds(ids);
        return garcons.ToDictionary(g => g.Id, g => g.Nome);
    }

    public static async Task<string> Obter(IRepositorioUsuario repositorio, Comanda comanda)
    {
        var nomes = await Obter(repositorio, [comanda]);
        return nomes.TryGetValue(comanda.GarcomId, out var nome) ? nome : string.Empty;
    }
}
