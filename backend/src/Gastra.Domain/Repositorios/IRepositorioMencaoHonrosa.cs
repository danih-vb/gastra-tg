using Gastra.Domain.Entidades;

namespace Gastra.Domain.Repositorios;

public interface IRepositorioMencaoHonrosa
{
    Task Adicionar(MencaoHonrosa mencao);
    Task<MencaoHonrosa?> ObterPorId(int id);
    void Remover(MencaoHonrosa mencao);

    /// <summary>As menções do garçom, da mais recente para a mais antiga.</summary>
    Task<List<MencaoHonrosa>> ListarDoGarcom(int garcomId);
}
