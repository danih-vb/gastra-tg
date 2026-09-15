using Gastra.Domain.Entidades;

namespace Gastra.Domain.Repositorios;

public interface IRepositorioUsuario
{
    Task Adicionar(Usuario usuario);
    Task<Usuario?> ObterPorId(int id);
    Task<Usuario?> ObterPorEmail(string email);
    Task<bool> ExisteAlgum();
}
