using Gastra.Domain.Enums;

namespace Gastra.Domain.Entidades;

/// <summary>
/// Item lançado numa comanda. Só existe dentro de uma comanda (composição): é criado por
/// <see cref="Comanda.AdicionarItem"/>.
/// </summary>
public class ItemDoPedido : EntidadeBase
{
    public int ComandaId { get; private set; }
    public int ItemDoCardapioId { get; private set; }
    public int Quantidade { get; private set; }

    /// <summary>
    /// Preço copiado do cardápio no momento do lançamento: uma alteração de preço depois disso não
    /// muda o valor de uma comanda já aberta.
    /// </summary>
    public decimal PrecoUnitarioNoMomento { get; private set; }

    /// <summary>Cortesia do Metre ou do Gerente: entra na conta por R$ 0,00 e não conta como venda.</summary>
    public bool Cortesia { get; private set; }

    /// <summary>Preço antes de uma cortesia ou de um ajuste do Gerente. Nulo se o preço nunca mudou.</summary>
    public decimal? PrecoOriginal { get; private set; }

    /// <summary>Por que o preço mudou: obrigatório na cortesia e no ajuste, para o caixa e a gestão entenderem.</summary>
    public string? MotivoDoAjuste { get; private set; }

    public const int TamanhoMaximoDoMotivo = 120;

    /// <summary>Papel de quem lançou, quando não foi o garçom da mesa (Metre ou Gerente). Nulo quando foi o garçom.</summary>
    public PapelUsuario? LancadoPor { get; private set; }

    public DateTime DataHoraRegistro { get; private set; }
    public StatusItemPedido Status { get; private set; }
    public MotivoCancelamento? MotivoCancelamento { get; private set; }

    // Usado pelo Entity Framework ao ler do banco.
    private ItemDoPedido()
    {
    }

    internal ItemDoPedido(int itemDoCardapioId, int quantidade, decimal precoUnitario)
    {
        if (quantidade < 1)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade deve ser maior que zero.");

        if (precoUnitario <= 0)
            throw new ArgumentOutOfRangeException(nameof(precoUnitario), "O preço deve ser maior que zero.");

        ItemDoCardapioId = itemDoCardapioId;
        Quantidade = quantidade;
        PrecoUnitarioNoMomento = precoUnitario;
        DataHoraRegistro = DateTime.UtcNow;
        Status = StatusItemPedido.Pendente;
    }

    /// <summary>Item dado de cortesia: preço zero, guardando o do cardápio para mostrar quanto foi oferecido.</summary>
    internal static ItemDoPedido CriarCortesia(int itemDoCardapioId, int quantidade, decimal precoDoCardapio, string motivo)
    {
        var item = new ItemDoPedido(itemDoCardapioId, quantidade, precoDoCardapio);
        item.Cortesia = true;
        item.PrecoOriginal = precoDoCardapio;
        item.PrecoUnitarioNoMomento = 0;
        item.MotivoDoAjuste = ValidarMotivo(motivo);
        return item;
    }

    /// <summary>Ajuste de preço feito pelo Gerente numa comanda aberta. O primeiro preço fica guardado.</summary>
    public void AlterarPreco(decimal novoPreco, string motivo)
    {
        if (Status == StatusItemPedido.Cancelado)
            throw new InvalidOperationException("Um item cancelado não tem preço a alterar.");

        if (novoPreco < 0)
            throw new ArgumentOutOfRangeException(nameof(novoPreco), "O preço não pode ser negativo.");

        if (novoPreco == PrecoUnitarioNoMomento)
            throw new InvalidOperationException("O preço novo é igual ao atual.");

        var motivoValido = ValidarMotivo(motivo);
        PrecoOriginal ??= PrecoUnitarioNoMomento;
        PrecoUnitarioNoMomento = novoPreco;
        MotivoDoAjuste = motivoValido;
    }

    private static string ValidarMotivo(string motivo)
    {
        var limpo = motivo?.Trim() ?? string.Empty;
        if (limpo.Length == 0)
            throw new ArgumentException("O motivo é obrigatório.", nameof(motivo));
        if (limpo.Length > TamanhoMaximoDoMotivo)
            throw new ArgumentException("O motivo é longo demais.", nameof(motivo));
        return limpo;
    }

    /// <summary>Lançado pelo Metre ou pelo Gerente: fica fora da conta até o garçom da mesa confirmar.</summary>
    internal void AguardarConfirmacaoDoGarcom(PapelUsuario lancadoPor)
    {
        if (lancadoPor is not (PapelUsuario.Metre or PapelUsuario.Gerente))
            throw new ArgumentOutOfRangeException(nameof(lancadoPor), "Só o Metre e o Gerente lançam para o garçom confirmar.");

        LancadoPor = lancadoPor;
        Status = StatusItemPedido.AConfirmar;
    }

    /// <summary>O garçom aceita o item: ele passa a contar como pedido a entregar, a partir de agora.</summary>
    public void ConfirmarPeloGarcom()
    {
        if (Status != StatusItemPedido.AConfirmar)
            throw new InvalidOperationException("O item não está aguardando confirmação.");

        Status = StatusItemPedido.Pendente;
        DataHoraRegistro = DateTime.UtcNow;
    }

    /// <summary>O garçom recusa o item: ele sai da conta, com o motivo registrado.</summary>
    public void RecusarPeloGarcom()
    {
        if (Status != StatusItemPedido.AConfirmar)
            throw new InvalidOperationException("O item não está aguardando confirmação.");

        Status = StatusItemPedido.Cancelado;
        MotivoCancelamento = Enums.MotivoCancelamento.RecusadoPeloGarcom;
    }

    public void MarcarEntregue()
    {
        if (Status != StatusItemPedido.Pendente)
            throw new InvalidOperationException("Só um item pendente pode ser marcado como entregue.");

        Status = StatusItemPedido.Entregue;
    }

    /// <summary>RN02: cancelar exige justificativa da lista fechada.</summary>
    public void Cancelar(MotivoCancelamento motivo)
    {
        if (Status == StatusItemPedido.Cancelado)
            throw new InvalidOperationException("O item já está cancelado.");

        if (!Enum.IsDefined(motivo))
            throw new ArgumentOutOfRangeException(nameof(motivo), "Motivo de cancelamento inválido.");

        Status = StatusItemPedido.Cancelado;
        MotivoCancelamento = motivo;
    }

    /// <summary>Item cancelado não entra na conta.</summary>
    public decimal CalcularValor() =>
        Status is StatusItemPedido.Cancelado or StatusItemPedido.AConfirmar ? 0 : Quantidade * PrecoUnitarioNoMomento;
}
