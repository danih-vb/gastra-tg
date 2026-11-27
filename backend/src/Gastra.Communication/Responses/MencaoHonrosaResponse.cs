namespace Gastra.Communication.Responses;

public class MencaoHonrosaResponse
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Icone { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;

    /// <summary>Primeiro dia do mês.</summary>
    public DateOnly Mes { get; set; }
}
