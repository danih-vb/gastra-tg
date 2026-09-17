namespace Gastra.Communication.Requests;

/// <summary>RF05 — foto do item no cardápio digital.</summary>
public class ImagemItemRequest
{
    /// <summary>Endereço da foto: URL http(s) ou caminho começando com "/". Vazio ou nulo tira a foto.</summary>
    public string? Imagem { get; set; }
}
