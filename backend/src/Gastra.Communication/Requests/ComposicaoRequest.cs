using Gastra.Communication.Enums;

namespace Gastra.Communication.Requests;

public class ComposicaoRequest
{
    public int QuantidadePessoas { get; set; }
    public ComposicaoMesa Composicao { get; set; }
}
