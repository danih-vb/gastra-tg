using Gastra.Communication.Enums;

namespace Gastra.Communication.Requests;

public class DestaqueRequest
{
    public int ItemCardapioId { get; set; }
    public TipoDestaque Tipo { get; set; }

    /// <summary>O dia do destaque ou qualquer dia do mês da meta. Sem data, vale hoje.</summary>
    public DateOnly? Data { get; set; }

    /// <summary>Unidades a vender no mês; só na meta do mês.</summary>
    public int? Meta { get; set; }
}
