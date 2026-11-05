using Gastra.Communication.Enums;

namespace Gastra.Communication.Responses;

public class PromocaoResponse
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public TipoDesconto TipoDesconto { get; set; }
    public decimal ValorDesconto { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public bool Ativa { get; set; }
    public bool VigenteHoje { get; set; }
    public List<ItemPromocaoResponse> Itens { get; set; } = [];
}

public class ItemPromocaoResponse
{
    public int ItemCardapioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal PrecoOriginal { get; set; }
    public decimal PrecoComDesconto { get; set; }
}
