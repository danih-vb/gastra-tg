using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;

namespace Gastra.Domain.Seguranca;

public interface IUsuarioLogado
{
    /// <summary>Usuário dono do token de acesso da requisição atual.</summary>
    Task<Usuario> Obter();

    /// <summary>Papel que está no token da requisição atual, sem consultar o banco.</summary>
    PapelUsuario ObterPapel();

    /// <summary>Id e papel do token, ou nulo quando a requisição não tem usuário (ex.: login).</summary>
    (int Id, PapelUsuario Papel)? ObterIdentificacao();
}
