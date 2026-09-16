using Gastra.Domain.Entidades;

namespace Gastra.Domain.Repositorios;

public interface IRepositorioComanda
{
    Task Adicionar(Comanda comanda);

    /// <summary>Traz a comanda com os itens e as restrições, que é como as regras precisam dela.</summary>
    Task<Comanda?> ObterPorId(int id);

    /// <summary>Consulta do cliente por QR code (UC20), sem login.</summary>
    Task<Comanda?> ObterPorCodigoAcesso(string codigoAcesso);

    Task<List<Comanda>> ListarAbertas();

    Task<bool> ExisteAbertaNaMesa(int mesaId);
}
