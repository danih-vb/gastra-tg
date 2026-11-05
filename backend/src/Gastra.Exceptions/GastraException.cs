namespace Gastra.Exceptions;

/// <summary>
/// Base das exceções de negócio do GASTRA. Cada exceção sabe qual status HTTP representa;
/// o filtro de exceções da API usa isso para montar a resposta.
/// </summary>
public abstract class GastraException(string mensagem) : SystemException(mensagem)
{
    public abstract int StatusCode { get; }

    public abstract List<string> ObterErros();
}
