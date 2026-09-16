using Gastra.Domain.Enums;

namespace Gastra.Domain.Entidades;

public class ItemDoCardapio : EntidadeBase
{
    private readonly List<ItemCardapioFlag> _flags = [];

    public string Nome { get; private set; } = string.Empty;
    public CategoriaItemCardapio Categoria { get; private set; }
    public decimal Preco { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public bool Disponivel { get; private set; }
    public string? Imagem { get; private set; }

    public IReadOnlyCollection<FlagDietetica> FlagsDieteticas => _flags.Select(f => f.Flag).ToList();

    // Usado pelo Entity Framework ao ler do banco.
    private ItemDoCardapio()
    {
    }

    public ItemDoCardapio(
        string nome,
        CategoriaItemCardapio categoria,
        decimal preco,
        string descricao,
        IEnumerable<FlagDietetica> flagsDieteticas,
        string? imagem = null)
    {
        Nome = nome;
        Categoria = categoria;
        Descricao = descricao;
        Imagem = imagem;
        Disponivel = true;
        DefinirPreco(preco);
        DefinirFlags(flagsDieteticas);
    }

    /// <summary>RF20.</summary>
    public void AtualizarPreco(decimal novoPreco) => DefinirPreco(novoPreco);

    /// <summary>RF21.</summary>
    public void MarcarDisponibilidade(bool disponivel) => Disponivel = disponivel;

    /// <summary>
    /// Diz se o item pode ser sugerido a uma mesa com essa restrição. Na dúvida, não: só atende quem tem a
    /// flag no cardápio. Alergia e "outro" dependem do texto livre e não filtram nada; ficam com o garçom.
    /// </summary>
    public bool AtendeRestricao(CategoriaRestricao restricao) => restricao switch
    {
        CategoriaRestricao.Vegano => Possui(FlagDietetica.Vegano),
        CategoriaRestricao.Vegetariano => Possui(FlagDietetica.Vegetariano) || Possui(FlagDietetica.Vegano),
        CategoriaRestricao.SemGluten => Possui(FlagDietetica.SemGluten),
        CategoriaRestricao.SemLactose => Possui(FlagDietetica.SemLactose) || Possui(FlagDietetica.Vegano),
        _ => true,
    };

    private bool Possui(FlagDietetica flag) => _flags.Any(f => f.Flag == flag);

    private void DefinirPreco(decimal preco)
    {
        if (preco <= 0)
            throw new ArgumentOutOfRangeException(nameof(preco), preco, "O preço deve ser maior que zero.");

        Preco = preco;
    }

    private void DefinirFlags(IEnumerable<FlagDietetica> flags)
    {
        _flags.Clear();
        foreach (var flag in flags.Distinct())
            _flags.Add(new ItemCardapioFlag(flag));
    }
}
