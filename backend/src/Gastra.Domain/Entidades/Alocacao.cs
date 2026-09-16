using Gastra.Domain.Enums;

namespace Gastra.Domain.Entidades;

/// <summary>
/// Garçom designado a uma praça num turno (RF06, RF07). O histórico de alocações confirmadas
/// alimenta a própria regra de distribuição (RN03) e o BI (RF08).
/// </summary>
public class Alocacao : EntidadeBase
{
    public DateOnly Data { get; private set; }
    public PeriodoAlocacao Periodo { get; private set; }
    public int GarcomId { get; private set; }
    public int PracaId { get; private set; }
    public bool Confirmada { get; private set; }

    // Usado pelo Entity Framework ao ler do banco.
    private Alocacao()
    {
    }

    public Alocacao(DateOnly data, PeriodoAlocacao periodo, int garcomId, int pracaId)
    {
        if (!Enum.IsDefined(periodo))
            throw new ArgumentOutOfRangeException(nameof(periodo), "Período inválido.");

        Data = data;
        Periodo = periodo;
        GarcomId = garcomId;
        PracaId = pracaId;
        Confirmada = false;
    }

    /// <summary>RF07: o metre pode trocar a praça sugerida, mas só antes de confirmar.</summary>
    public void Ajustar(int novaPracaId)
    {
        if (Confirmada)
            throw new InvalidOperationException("Uma alocação confirmada não pode ser ajustada.");

        PracaId = novaPracaId;
    }

    public void Confirmar()
    {
        if (Confirmada)
            throw new InvalidOperationException("A alocação já está confirmada.");

        Confirmada = true;
    }
}
