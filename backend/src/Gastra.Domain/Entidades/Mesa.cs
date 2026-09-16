namespace Gastra.Domain.Entidades;

/// <summary>
/// Mesa do salão. Toda mesa pertence a exatamente uma praça (decisão da dupla, REL01).
/// </summary>
public class Mesa : EntidadeBase
{
    public string Numero { get; private set; } = string.Empty;
    public int Capacidade { get; private set; }
    public int PracaId { get; private set; }

    // Usado pelo Entity Framework ao ler do banco.
    private Mesa()
    {
    }

    public Mesa(string numero, int capacidade, int pracaId)
    {
        Numero = numero;
        PracaId = pracaId;
        DefinirCapacidade(capacidade);
    }

    /// <summary>
    /// A praça não entra: o vínculo mesa-praça é fixo (REL01), senão o histórico de faturamento por
    /// praça deixaria de fazer sentido.
    /// </summary>
    public void Atualizar(string numero, int capacidade)
    {
        Numero = numero;
        DefinirCapacidade(capacidade);
    }

    private void DefinirCapacidade(int capacidade)
    {
        if (capacidade < 1)
            throw new ArgumentOutOfRangeException(nameof(capacidade), "A mesa precisa comportar pelo menos uma pessoa.");

        Capacidade = capacidade;
    }
}
