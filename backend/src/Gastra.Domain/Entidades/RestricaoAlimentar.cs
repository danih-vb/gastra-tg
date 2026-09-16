using Gastra.Domain.Enums;

namespace Gastra.Domain.Entidades;

/// <summary>
/// Restrição ou preferência alimentar informada voluntariamente pelo cliente (RF14). Vive presa à
/// comanda ativa: é criada por <see cref="Comanda.RegistrarRestricao"/>.
/// </summary>
public class RestricaoAlimentar : EntidadeBase
{
    public int ComandaId { get; private set; }
    public CategoriaRestricao Categoria { get; private set; }

    /// <summary>
    /// Detalhe escrito pelo garçom. Pode conter dado de saúde (LGPD art. 5º, II), por isso é apagado
    /// no fechamento da comanda, quando fica apenas a categoria.
    /// </summary>
    public string? ObservacaoLivre { get; private set; }

    // Usado pelo Entity Framework ao ler do banco.
    private RestricaoAlimentar()
    {
    }

    internal RestricaoAlimentar(CategoriaRestricao categoria, string? observacaoLivre)
    {
        if (!Enum.IsDefined(categoria))
            throw new ArgumentOutOfRangeException(nameof(categoria), "Categoria de restrição inválida.");

        Categoria = categoria;
        ObservacaoLivre = string.IsNullOrWhiteSpace(observacaoLivre) ? null : observacaoLivre.Trim();
    }

    internal void ApagarObservacaoLivre() => ObservacaoLivre = null;
}
