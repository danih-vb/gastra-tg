namespace Gastra.Domain.Entidades;

/// <summary>
/// Área do salão que agrupa mesas. É a unidade da alocação de garçons (RN03).
/// </summary>
public class Praca : EntidadeBase
{
    public string Codigo { get; private set; } = string.Empty;

    /// <summary>Quantos garçons a praça comporta por turno: entrada da alocação (RF06).</summary>
    public int QuantidadeGarcons { get; private set; }

    // Usado pelo Entity Framework ao ler do banco.
    private Praca()
    {
    }

    public Praca(string codigo, int quantidadeGarcons)
    {
        Codigo = codigo;
        DefinirQuantidadeGarcons(quantidadeGarcons);
    }

    public void Atualizar(string codigo, int quantidadeGarcons)
    {
        Codigo = codigo;
        DefinirQuantidadeGarcons(quantidadeGarcons);
    }

    private void DefinirQuantidadeGarcons(int quantidade)
    {
        if (quantidade < 1)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A praça precisa de pelo menos um garçom.");

        QuantidadeGarcons = quantidade;
    }
}
