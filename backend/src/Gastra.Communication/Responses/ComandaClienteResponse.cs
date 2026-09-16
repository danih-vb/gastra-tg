namespace Gastra.Communication.Responses;

/// <summary>
/// Consulta do cliente por QR code (UC20). Traz apenas o que ele precisa ver da própria conta: nada
/// de identificador do garçom, restrição alimentar ou dado de outra mesa (RN04).
/// </summary>
public class ComandaClienteResponse
{
    public string Mesa { get; set; } = string.Empty;
    public DateTime DataHoraAbertura { get; set; }
    public bool Fechada { get; set; }
    public List<ItemConsultaCliente> Itens { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal TaxaServico { get; set; }
    public decimal Total { get; set; }
}

public class ItemConsultaCliente
{
    public string Nome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Valor { get; set; }
    public string Situacao { get; set; } = string.Empty;
}
