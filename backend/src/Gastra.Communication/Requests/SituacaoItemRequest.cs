using Gastra.Communication.Enums;

namespace Gastra.Communication.Requests;

public class SituacaoItemRequest
{
    /// <summary>Entregue ou Cancelado (RN02).</summary>
    public StatusItemPedido Situacao { get; set; }

    /// <summary>Obrigatório quando a situação é Cancelado.</summary>
    public MotivoCancelamento? MotivoCancelamento { get; set; }
}
