namespace Gastra.Exceptions;

/// <summary>Registro inexistente (HTTP 404).</summary>
public class NaoEncontradoException(string mensagem) : GastraException(mensagem)
{
    public override int StatusCode => 404;

    public override List<string> ObterErros() => [Message];
}
