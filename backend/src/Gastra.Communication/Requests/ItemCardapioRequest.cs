using Gastra.Communication.Enums;

namespace Gastra.Communication.Requests;

public class ItemCardapioRequest
{
    public string Nome { get; set; } = string.Empty;
    public CategoriaItemCardapio Categoria { get; set; }
    public decimal Preco { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public List<FlagDietetica> FlagsDieteticas { get; set; } = [];

    /// <summary>
    /// Endereço da foto mostrada no cardápio digital (RF05): URL http(s) ou caminho começando com "/".
    /// A API guarda o endereço, e não o arquivo.
    /// </summary>
    public string? Imagem { get; set; }
}
