namespace Gastra.Exceptions;

/// <summary>Requisição válida, mas que viola uma regra de negócio (HTTP 422).</summary>
public class RegraDeNegocioException(string mensagem) : GastraException(mensagem)
{
    public override int StatusCode => 422;

    public override List<string> ObterErros() => [Message];
}
