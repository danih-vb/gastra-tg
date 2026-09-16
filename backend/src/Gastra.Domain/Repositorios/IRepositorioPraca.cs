using Gastra.Domain.Entidades;

namespace Gastra.Domain.Repositorios;

public interface IRepositorioPraca
{
    Task Adicionar(Praca praca);
    Task<Praca?> ObterPorId(int id);
    Task<List<Praca>> ListarTodas();
}
