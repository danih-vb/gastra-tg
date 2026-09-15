using Gastra.Communication.Enums;

namespace Gastra.Communication.Requests;

public class ItemCardapioRequest
{
    public string Nome { get; set; } = string.Empty;
    public CategoriaItemCardapio Categoria { get; set; }
    public decimal Preco { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public List<FlagDietetica> FlagsDieteticas { get; set; } = [];
}
