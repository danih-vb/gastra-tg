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

    /// <summary>
    /// Praças de maior movimento, as que a RN03 reparte com mais cuidado. Só para Metre e Gerente; nulo para os demais.
    /// </summary>
    public List<int>? PracasDeAltoPotencial { get; set; }
}

public class DesignacaoResponse
{
    public int GarcomId { get; set; }
    public string GarcomNome { get; set; } = string.Empty;
    public int PracaId { get; set; }
    public string PracaCodigo { get; set; } = string.Empty;

    /// <summary>
    /// Por que a RN03 mandou o garçom para cá: onde o faturamento por turno dele fica em relação à equipe do turno
    /// (a faixa, nunca o valor) e há quantos turnos ele não pega praça de alto potencial. Só para Metre e Gerente.
    /// </summary>
    public FaixaDeFaturamento? FaixaDeFaturamento { get; set; }

    public int? TurnosDesdePracaDeAltoPotencial { get; set; }
}
