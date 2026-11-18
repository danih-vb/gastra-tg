using Gastra.Communication.Enums;

namespace Gastra.Communication.Responses;

/// <summary>
/// Resultado do login. Para Garçom e Metre traz o token de acesso direto. Para Gerente e
/// Coordenador (RF16), traz só o token temporário do segundo fator.
/// </summary>
public class LoginResponse
{
    public string? TokenAcesso { get; set; }
    public string? Nome { get; set; }
    public PapelUsuario? Papel { get; set; }

    public bool RequerSegundoFator { get; set; }
    public bool RequerConfiguracaoSegundoFator { get; set; }
    public string? TokenSegundoFator { get; set; }
}
