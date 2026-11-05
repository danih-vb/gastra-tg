namespace Gastra.Communication.Requests;

public class AbrirComandaRequest
{
    public int MesaId { get; set; }

    /// <summary>Informado pelo garçom na abertura: base da composição da mesa (RN01).</summary>
    public int QuantidadePessoas { get; set; }
}
