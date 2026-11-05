namespace Gastra.Communication.Requests;

public class CadastrarMesaRequest
{
    public string Numero { get; set; } = string.Empty;
    public int Capacidade { get; set; }
    public int PracaId { get; set; }
}
