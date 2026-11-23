namespace Gastra.Communication.Requests;

/// <summary>Item dado de cortesia pelo Metre ou pelo Gerente.</summary>
public class CortesiaRequest
{
    public int ItemCardapioId { get; set; }
    public int Quantidade { get; set; } = 1;
    public string Motivo { get; set; } = string.Empty;
}
