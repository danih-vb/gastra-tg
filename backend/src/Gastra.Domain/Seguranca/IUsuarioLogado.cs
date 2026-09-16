using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;

namespace Gastra.Domain.Seguranca;

public interface IUsuarioLogado
{
    /// <summary>Usuário dono do token de acesso da requisição atual.</summary>
    Task<Usuario> Obter();

    /// <summary>Papel que está no token da requisição atual, sem consultar o banco.</summary>
    PapelUsuario ObterPapel();
}
