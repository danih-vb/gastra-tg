namespace Gastra.Communication.Responses;

public class MedalhaResponse
{
    /// <summary>top-3, mesa-cheia, maratonista, bem-avaliado, vendedor-de-destaque ou meta-batida.</summary>
    public string Codigo { get; set; } = string.Empty;
    public bool Conquistada { get; set; }
    public decimal Progresso { get; set; }
    public decimal Alvo { get; set; }
}

public class ConquistasDoMesResponse
{
    /// <summary>Primeiro dia do mês.</summary>
    public DateOnly Mes { get; set; }
    public List<string> Conquistadas { get; set; } = [];
}

public class MedalhasResponse
{
    public int GarcomId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateOnly Mes { get; set; }

    /// <summary>As seis medalhas do mês atual, com o progresso das que faltam.</summary>
    public List<MedalhaResponse> Medalhas { get; set; } = [];

    /// <summary>O que foi conquistado em cada um dos meses anteriores, do mais recente ao mais antigo.</summary>
    public List<ConquistasDoMesResponse> Historico { get; set; } = [];

    /// <summary>Menções honrosas dadas pelo Gerente, da mais recente para a mais antiga.</summary>
    public List<MencaoHonrosaResponse> Mencoes { get; set; } = [];
}
