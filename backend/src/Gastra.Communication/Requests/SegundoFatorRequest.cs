namespace Gastra.Communication.Requests;

public class SegundoFatorRequest
{
    /// <summary>Token temporário devolvido pelo login.</summary>
    public string TokenSegundoFator { get; set; } = string.Empty;

    /// <summary>Código de 6 dígitos do app autenticador. Não usado na configuração.</summary>
    public string Codigo { get; set; } = string.Empty;
}
