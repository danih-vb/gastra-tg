namespace Gastra.Communication.Requests;

public class MencaoHonrosaRequest
{
    public int GarcomId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    /// <summary>Um dos ícones da lista: trofeu, estrela, coracao, faisca, alvo, relogio, pessoas ou check.</summary>
    public string Icone { get; set; } = string.Empty;

    /// <summary>Uma das cores da lista: laranja, marinho, verde, vermelho, roxo ou dourado.</summary>
    public string Cor { get; set; } = string.Empty;

    /// <summary>Qualquer dia do mês a que a menção se refere. Sem data, vale o mês atual.</summary>
    public DateOnly? Mes { get; set; }
}
