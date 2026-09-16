using Gastra.Communication.Enums;

namespace Gastra.Communication.Responses;

/// <summary>UC18 — sugestões de itens para o garçom oferecer na mesa (RF09).</summary>
public class SugestoesComandaResponse
{
    /// <summary>
    /// Falso quando a camada analítica não respondeu. A lista vem vazia e o atendimento segue normalmente (D3).
    /// </summary>
    public bool ServicoDisponivel { get; set; }

    /// <summary>
    /// A mesa tem restrição que o cardápio não confere sozinho (alergia ou "outro"): o garçom confirma com
    /// o cliente antes de oferecer.
    /// </summary>
    public bool ConfirmarRestricaoComCliente { get; set; }

    public List<ItemSugeridoResponse> Itens { get; set; } = [];
}

public class ItemSugeridoResponse
{
    public int ItemDoCardapioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public CategoriaItemCardapio Categoria { get; set; }
    public decimal Preco { get; set; }
}
