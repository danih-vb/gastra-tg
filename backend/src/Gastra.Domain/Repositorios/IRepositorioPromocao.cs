using Gastra.Domain.Entidades;

namespace Gastra.Domain.Repositorios;

public interface IRepositorioPromocao
{
    Task Adicionar(Promocao promocao);
    Task<Promocao?> ObterPorId(int id);
    Task<List<Promocao>> Listar(bool somenteAtivas);

    /// <summary>Promoções ativas cujo período inclui a data.</summary>
    Task<List<Promocao>> ListarVigentes(DateOnly data);
}
