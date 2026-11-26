namespace Gastra.Domain.Entidades;

/// <summary>
/// Reconhecimento que o Gerente dá a um garçom por algo que o sistema não mede: quem faltou menos, quem trouxe a
/// melhor ideia. Não tem regra própria: o Gerente escolhe um ícone e uma cor da lista, escreve o título e a descrição,
/// e a menção aparece junto das medalhas do garçom.
/// </summary>
public class MencaoHonrosa : EntidadeBase
{
    public const int TamanhoMaximoDoTitulo = 60;
    public const int TamanhoMaximoDaDescricao = 200;

    public static readonly IReadOnlyList<string> Icones =
        ["trofeu", "estrela", "coracao", "faisca", "alvo", "relogio", "pessoas", "check"];

    public static readonly IReadOnlyList<string> Cores = ["laranja", "marinho", "verde", "vermelho", "roxo", "dourado"];

    public int GarcomId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public string Icone { get; private set; } = string.Empty;
    public string Cor { get; private set; } = string.Empty;

    /// <summary>Primeiro dia do mês a que a menção se refere.</summary>
    public DateOnly Mes { get; private set; }

    public int ConcedidaPorId { get; private set; }
    public DateTime DataHoraConcessao { get; private set; }

    // Usado pelo Entity Framework ao ler do banco.
    private MencaoHonrosa()
    {
    }

    /// <param name="referencia">Qualquer dia do mês a que a menção se refere.</param>
    public MencaoHonrosa(int garcomId, string titulo, string? descricao, string icone, string cor, DateOnly referencia,
        int concedidaPorId)
    {
        titulo = titulo?.Trim() ?? string.Empty;
        descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();

        if (titulo.Length is 0 or > TamanhoMaximoDoTitulo)
            throw new ArgumentException("O título precisa ter de 1 a 60 caracteres.", nameof(titulo));
        if (descricao?.Length > TamanhoMaximoDaDescricao)
            throw new ArgumentException("A descrição tem até 200 caracteres.", nameof(descricao));
        if (!Icones.Contains(icone))
            throw new ArgumentOutOfRangeException(nameof(icone), "Ícone fora da lista.");
        if (!Cores.Contains(cor))
            throw new ArgumentOutOfRangeException(nameof(cor), "Cor fora da lista.");

        GarcomId = garcomId;
        Titulo = titulo;
        Descricao = descricao;
        Icone = icone;
        Cor = cor;
        Mes = new DateOnly(referencia.Year, referencia.Month, 1);
        ConcedidaPorId = concedidaPorId;
        DataHoraConcessao = DateTime.UtcNow;
    }
}
