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
        Status == StatusItemPedido.Cancelado ? 0 : Quantidade * PrecoUnitarioNoMomento;
}
