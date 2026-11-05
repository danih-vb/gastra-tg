namespace Gastra.Exceptions;

/// <summary>Requisição com formato inválido (HTTP 400).</summary>
public class ErroValidacaoException(List<string> erros) : GastraException(string.Join(" ", erros))
{
    private readonly List<string> _erros = erros;

    public override int StatusCode => 400;

    public override List<string> ObterErros() => _erros;
}
