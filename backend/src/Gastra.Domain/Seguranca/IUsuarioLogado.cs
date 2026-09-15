using Gastra.Domain.Entidades;

namespace Gastra.Domain.Seguranca;

public interface IUsuarioLogado
{
    /// <summary>Usuário dono do token de acesso da requisição atual.</summary>
    Task<Usuario> Obter();
}
