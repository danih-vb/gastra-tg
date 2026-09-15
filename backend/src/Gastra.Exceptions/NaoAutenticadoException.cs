namespace Gastra.Exceptions;

/// <summary>Credenciais, token ou código de verificação inválidos (HTTP 401).</summary>
public class NaoAutenticadoException(string mensagem) : GastraException(mensagem)
{
    public override int StatusCode => 401;

    public override List<string> ObterErros() => [Message];
}
