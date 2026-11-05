using Gastra.Communication.Enums;

namespace Gastra.Communication.Requests;

/// <summary>UC08 — nova promoção.</summary>
public class PromocaoRequest
{
    public string Descricao { get; set; } = string.Empty;
    public TipoDesconto TipoDesconto { get; set; }

    /// <summary>Percentual (0 a 100, exclusivo) ou valor fixo em reais, conforme o tipo.</summary>
    public decimal ValorDesconto { get; set; }

    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public List<int> ItemCardapioIds { get; set; } = [];
}
