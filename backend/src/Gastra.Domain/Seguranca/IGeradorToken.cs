using Gastra.Domain.Entidades;

namespace Gastra.Domain.Seguranca;

public interface IGeradorToken
{
    /// <summary>Token de acesso ao sistema (RF15).</summary>
    string GerarTokenAcesso(Usuario usuario);

    /// <summary>
    /// Token temporário entre a senha e o segundo fator (RF16). Não dá acesso a nenhum endpoint:
    /// serve apenas para configurar ou confirmar o segundo fator.
    /// </summary>
    string GerarTokenSegundoFator(Usuario usuario);

    /// <summary>Devolve o id do usuário se o token de segundo fator for válido e não tiver expirado.</summary>
    Task<int?> ValidarTokenSegundoFator(string token);
}
