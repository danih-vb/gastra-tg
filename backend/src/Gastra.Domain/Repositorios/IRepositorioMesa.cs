using Gastra.Domain.Entidades;

namespace Gastra.Domain.Repositorios;

public interface IRepositorioMesa
{
    Task Adicionar(Mesa mesa);
    Task<Mesa?> ObterPorId(int id);
    Task<List<Mesa>> ListarTodas();
}
