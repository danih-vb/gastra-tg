using Gastra.Domain.Entidades;

namespace Gastra.Domain.Repositorios;

public interface IRepositorioItemCardapio
{
    Task Adicionar(ItemDoCardapio item);
    Task<ItemDoCardapio?> ObterPorId(int id);
    Task<List<ItemDoCardapio>> ListarTodos();
    Task<List<ItemDoCardapio>> ListarPorIds(IEnumerable<int> ids);
    Task<List<ItemDoCardapio>> ListarDisponiveis();
    Task<List<ItemDoCardapio>> BuscarPorNome(string nome);
}
