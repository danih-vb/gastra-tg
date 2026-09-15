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
