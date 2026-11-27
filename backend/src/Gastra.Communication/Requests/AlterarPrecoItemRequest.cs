namespace Gastra.Communication.Requests;

/// <summary>Preço novo de um item já lançado, definido pelo Gerente.</summary>
public class AlterarPrecoItemRequest
{
    public decimal Preco { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
