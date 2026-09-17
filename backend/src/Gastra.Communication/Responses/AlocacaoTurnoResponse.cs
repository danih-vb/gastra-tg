using Gastra.Communication.Enums;

namespace Gastra.Communication.Responses;

public class AlocacaoTurnoResponse
{
    public DateOnly Data { get; set; }
    public PeriodoAlocacao Periodo { get; set; }

    /// <summary>Verdadeiro quando o Metre já confirmou o turno (UC21): não aceita mais ajuste.</summary>
    public bool Confirmada { get; set; }

    /// <summary>
    /// Só na geração da sugestão: falso quando o serviço analítico não respondeu. A lista vem vazia e o Metre
    /// aloca manualmente, garçom por garçom (D3).
    /// </summary>
    public bool? ServicoDisponivel { get; set; }

    public List<DesignacaoResponse> Designacoes { get; set; } = [];
}

public class DesignacaoResponse
{
    public int GarcomId { get; set; }
    public string GarcomNome { get; set; } = string.Empty;
    public int PracaId { get; set; }
    public string PracaCodigo { get; set; } = string.Empty;
}
