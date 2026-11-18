namespace Gastra.Communication.Requests;

/// <summary>UC04 — senha nova definida pelo Gerente para outra conta.</summary>
public class RedefinirSenhaRequest
{
    public string Senha { get; set; } = string.Empty;
}
