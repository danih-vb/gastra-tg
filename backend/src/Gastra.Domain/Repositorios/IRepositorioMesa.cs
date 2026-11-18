using Gastra.Domain.Entidades;

namespace Gastra.Domain.Repositorios;

public interface IRepositorioMesa
{
    Task Adicionar(Mesa mesa);
    Task<Mesa?> ObterPorId(int id);
    Task<Mesa?> ObterPorNumero(string numero);
    Task<List<Mesa>> ListarTodas();
}
