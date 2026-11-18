using Gastra.Communication.Enums;

namespace Gastra.Communication.Requests;

/// <summary>UC15 — o Metre informa o turno e quem está presente.</summary>
public class SugestaoAlocacaoRequest
{
    public DateOnly Data { get; set; }
    public PeriodoAlocacao Periodo { get; set; }
    public List<int> GarcomIds { get; set; } = [];
}
