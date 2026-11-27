using Gastra.Domain.Enums;

namespace Gastra.Domain.Entidades;

/// <summary>
/// Item que o Gerente quer ver saindo: destaque de um dia ou meta de vendas de um mês. Para o garçom, o item aparece
/// primeiro e colorido na hora de lançar; a meta mostra quanto a equipe já vendeu.
/// </summary>
public class DestaqueDoCardapio : EntidadeBase
{
    public const int MetaMaxima = 10_000;

    public int ItemDoCardapioId { get; private set; }
    public TipoDestaque Tipo { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }

    /// <summary>Unidades a vender no mês. Nulo no destaque do dia.</summary>
    public int? Meta { get; private set; }

    // Usado pelo Entity Framework ao ler do banco.
    private DestaqueDoCardapio()
    {
    }

    /// <param name="referencia">O dia do destaque, ou qualquer dia do mês da meta.</param>
    public DestaqueDoCardapio(int itemDoCardapioId, TipoDestaque tipo, DateOnly referencia, int? meta)
    {
        if (!Enum.IsDefined(tipo))
            throw new ArgumentOutOfRangeException(nameof(tipo), "Tipo de destaque inválido.");

        ItemDoCardapioId = itemDoCardapioId;
        Tipo = tipo;

        if (tipo == TipoDestaque.DestaqueDoDia)
        {
            DataInicio = DataFim = referencia;
            return;
        }

        if (meta is null or < 1 or > MetaMaxima)
            throw new ArgumentOutOfRangeException(nameof(meta), "A meta precisa ser de 1 a 10.000 unidades.");

        DataInicio = new DateOnly(referencia.Year, referencia.Month, 1);
        DataFim = DataInicio.AddMonths(1).AddDays(-1);
        Meta = meta;
    }

    public bool VigenteEm(DateOnly data) => data >= DataInicio && data <= DataFim;

    public bool ColideCom(DestaqueDoCardapio outro) =>
        outro.ItemDoCardapioId == ItemDoCardapioId && outro.Tipo == Tipo &&
        outro.DataInicio <= DataFim && outro.DataFim >= DataInicio;
}
