namespace Gastra.Communication.Requests;

public class PracaRequest
{
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Quantos garçons a praça comporta por turno: entrada da alocação (RN03).</summary>
    public int QuantidadeGarcons { get; set; }
}
