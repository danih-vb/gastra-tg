using Gastra.Communication.Enums;

namespace Gastra.Communication.Responses;

public class ItemCardapioResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public CategoriaItemCardapio Categoria { get; set; }
    public decimal Preco { get; set; }

    /// <summary>Preço com a melhor promoção que vale hoje; nulo quando não há promoção para o item.</summary>
    public decimal? PrecoPromocional { get; set; }

    public string Descricao { get; set; } = string.Empty;
    public List<FlagDietetica> FlagsDieteticas { get; set; } = [];
    public bool Disponivel { get; set; }
    public string? Imagem { get; set; }
}
