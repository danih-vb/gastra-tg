using Gastra.Communication.Enums;

namespace Gastra.Communication.Responses;

public class ItemPedidoResponse
{
    public int Id { get; set; }
    public int ItemDoCardapioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitarioNoMomento { get; set; }
    public decimal Valor { get; set; }
    public StatusItemPedido Status { get; set; }
    public MotivoCancelamento? MotivoCancelamento { get; set; }
    public DateTime DataHoraRegistro { get; set; }
}
