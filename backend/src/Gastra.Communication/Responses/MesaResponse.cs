namespace Gastra.Communication.Responses;

public class MesaResponse
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Capacidade { get; set; }
    public int PracaId { get; set; }
    public string PracaCodigo { get; set; } = string.Empty;
}
