using Gastra.Communication.Enums;

namespace Gastra.Communication.Responses;

public class DestaqueResponse
{
    public int Id { get; set; }
    public int ItemCardapioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoDestaque Tipo { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public int? Meta { get; set; }

    /// <summary>Unidades vendidas pela equipe no período, sem cortesias e sem itens cancelados.</summary>
    public int Vendidos { get; set; }

    public bool VigenteHoje { get; set; }
}
