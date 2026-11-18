namespace Gastra.Communication.Requests;

/// <summary>
/// A praça não entra aqui: o vínculo entre mesa e praça é fixo no modelo (REL01 do MER), porque o
/// histórico de faturamento por praça perderia o sentido se a mesa mudasse de lugar.
/// </summary>
public class EditarMesaRequest
{
    public string Numero { get; set; } = string.Empty;
    public int Capacidade { get; set; }
}
