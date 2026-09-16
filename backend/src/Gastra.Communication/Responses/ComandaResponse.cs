using Gastra.Communication.Enums;

namespace Gastra.Communication.Responses;

public class ComandaResponse
{
    public int Id { get; set; }
    public int MesaId { get; set; }
    public StatusComanda Status { get; set; }
    public int QuantidadePessoas { get; set; }
    public ComposicaoMesa Composicao { get; set; }
    public bool ComposicaoAjustadaManualmente { get; set; }
    public bool TaxaServicoRemovida { get; set; }
    public string CodigoAcessoCliente { get; set; } = string.Empty;
    public DateTime DataHoraAbertura { get; set; }
    public DateTime? DataHoraFechamento { get; set; }
    public List<ItemPedidoResponse> Itens { get; set; } = [];
    /// <summary>
    /// Só para Garçom e Metre, com a comanda aberta (RN04). Para os demais papéis e depois do fechamento,
    /// vem vazia: é dado do cliente.
    /// </summary>
    public List<RestricaoAlimentarResponse> Restricoes { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal TaxaServico { get; set; }
    public decimal Total { get; set; }
}
