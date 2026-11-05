using Gastra.Domain.Enums;

namespace Gastra.Domain.Entidades;

/// <summary>
/// Desconto num ou mais itens do cardápio, com período de validade (RF22, UC08, UC09). Remover é desativar:
/// a promoção continua no banco, porque itens de comandas antigas foram vendidos com o preço dela.
/// </summary>
public class Promocao : EntidadeBase
{
    public const int TamanhoMaximoDescricao = 200;

    /// <summary>Nenhum item sai de graça: o preço promocional nunca fica abaixo disto.</summary>
    public const decimal PrecoMinimo = 0.01m;

    private readonly List<PromocaoItem> _itens = [];

    public string Descricao { get; private set; } = string.Empty;
    public TipoDesconto TipoDesconto { get; private set; }
    public decimal ValorDesconto { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }
    public bool Ativa { get; private set; }

    public IReadOnlyCollection<int> ItemCardapioIds => _itens.Select(i => i.ItemCardapioId).ToList();

    // Usado pelo Entity Framework ao ler do banco.
    private Promocao()
    {
    }

    public Promocao(
        string descricao,
        TipoDesconto tipoDesconto,
        decimal valorDesconto,
        DateOnly dataInicio,
        DateOnly dataFim,
        IEnumerable<ItemDoCardapio> itens)
    {
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Trim().Length > TamanhoMaximoDescricao)
            throw new ArgumentException("A descrição é obrigatória e tem até 200 caracteres.", nameof(descricao));

        if (!Enum.IsDefined(tipoDesconto))
            throw new ArgumentOutOfRangeException(nameof(tipoDesconto), "Tipo de desconto inválido.");

        if (valorDesconto <= 0 || (tipoDesconto == TipoDesconto.Percentual && valorDesconto >= 100))
            throw new ArgumentOutOfRangeException(nameof(valorDesconto), "O desconto precisa ser positivo e, em percentual, menor que 100.");

        if (dataFim < dataInicio)
            throw new ArgumentException("A data final não pode ser anterior à inicial.", nameof(dataFim));

        var lista = itens.DistinctBy(i => i.Id).ToList();
        if (lista.Count == 0)
            throw new ArgumentException("A promoção precisa de pelo menos um item.", nameof(itens));

        // Desconto fixo maior que o preço daria item de graça (ou negativo).
        if (tipoDesconto == TipoDesconto.ValorFixo && lista.Any(i => valorDesconto >= i.Preco))
            throw new ArgumentOutOfRangeException(nameof(valorDesconto), "O desconto fixo precisa ser menor que o preço de cada item.");

        Descricao = descricao.Trim();
        TipoDesconto = tipoDesconto;
        ValorDesconto = valorDesconto;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Ativa = true;
        _itens.AddRange(lista.Select(i => new PromocaoItem(i.Id)));
    }

    /// <summary>UC09: a promoção deixa de valer, mas o registro fica.</summary>
    public void Desativar()
    {
        if (!Ativa)
            throw new InvalidOperationException("A promoção já está desativada.");

        Ativa = false;
    }

    public bool VigenteEm(DateOnly data) => Ativa && data >= DataInicio && data <= DataFim;

    public bool IncluiItem(int itemCardapioId) => _itens.Any(i => i.ItemCardapioId == itemCardapioId);

    public decimal AplicarDesconto(decimal preco)
    {
        var comDesconto = TipoDesconto == TipoDesconto.Percentual
            ? preco * (1 - ValorDesconto / 100)
            : preco - ValorDesconto;

        return Math.Max(PrecoMinimo, Math.Round(comDesconto, 2, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Preço promocional do item na data, ou nulo se nenhuma promoção vigente o inclui. Com mais de uma
    /// promoção valendo para o mesmo item, vale a de maior desconto: é o que o cliente esperaria.
    /// </summary>
    public static decimal? PrecoPromocional(ItemDoCardapio item, IEnumerable<Promocao> promocoes, DateOnly data)
    {
        var precos = promocoes
            .Where(p => p.VigenteEm(data) && p.IncluiItem(item.Id))
            .Select(p => p.AplicarDesconto(item.Preco))
            .ToList();

        return precos.Count == 0 ? null : precos.Min();
    }
}

/// <summary>Item vinculado à promoção: a relação N:N do MER, em tabela própria.</summary>
public class PromocaoItem
{
    public int ItemCardapioId { get; private set; }

    private PromocaoItem()
    {
    }

    internal PromocaoItem(int itemCardapioId)
    {
        ItemCardapioId = itemCardapioId;
    }
}
