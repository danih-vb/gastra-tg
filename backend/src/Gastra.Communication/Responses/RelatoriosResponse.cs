namespace Gastra.Communication.Responses;

// UC16 e UC17 — relatórios de BI (RF10) e índice de desempenho (RF11). Datas do período são inclusivas.

public class PeriodoResponse
{
    public DateOnly Inicio { get; set; }
    public DateOnly Fim { get; set; }
}

public class RelatorioGarconsResponse
{
    public PeriodoResponse Periodo { get; set; } = new();
    public List<IndicadorGarcomResponse> Garcons { get; set; } = [];
    public TotaisResponse Totais { get; set; } = new();
}

public class IndicadorGarcomResponse
{
    public int GarcomId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Faturamento { get; set; }
    public int Comandas { get; set; }
    public int Turnos { get; set; }
    public decimal FaturamentoPorTurno { get; set; }
    public int MesasAtendidas { get; set; }
    public decimal TicketMedio { get; set; }
    public decimal TempoMedioAtendimentoMinutos { get; set; }
}

public class TotaisResponse
{
    public decimal Faturamento { get; set; }
    public int Comandas { get; set; }
    public decimal TicketMedio { get; set; }
    public decimal TempoMedioAtendimentoMinutos { get; set; }
}

public class RelatorioPracasResponse
{
    public PeriodoResponse Periodo { get; set; } = new();
    public List<IndicadorPracaResponse> Pracas { get; set; } = [];
}

public class IndicadorPracaResponse
{
    public int PracaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public decimal Faturamento { get; set; }
    public int Comandas { get; set; }
    public int TurnosComMovimento { get; set; }
    public decimal FaturamentoPorTurno { get; set; }
    public decimal TicketMedio { get; set; }

    /// <summary>Média de todo o histórico, não só do período: é o potencial usado pela alocação (RN03).</summary>
    public decimal FaturamentoMedioHistorico { get; set; }
}

public class RelatorioCardapioResponse
{
    public PeriodoResponse Periodo { get; set; } = new();
    public List<IndicadorItemResponse> Itens { get; set; } = [];
    public List<IndicadorCategoriaResponse> Categorias { get; set; } = [];
}

public class IndicadorItemResponse
{
    public int ItemCardapioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal Faturamento { get; set; }
    public decimal ParticipacaoPercentual { get; set; }
}

public class IndicadorCategoriaResponse
{
    public string Categoria { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal Faturamento { get; set; }
    public decimal ParticipacaoPercentual { get; set; }
}

public class RelatorioHorariosResponse
{
    public PeriodoResponse Periodo { get; set; } = new();
    public List<FaturamentoNaHoraResponse> PorHora { get; set; } = [];
    public List<FaturamentoNoDiaResponse> PorDiaDaSemana { get; set; } = [];
}

public class FaturamentoNaHoraResponse
{
    public int PracaId { get; set; }
    public string PracaCodigo { get; set; } = string.Empty;
    public int Hora { get; set; }
    public decimal Faturamento { get; set; }
    public int Comandas { get; set; }
}

public class FaturamentoNoDiaResponse
{
    public int PracaId { get; set; }
    public string PracaCodigo { get; set; } = string.Empty;

    /// <summary>1 = domingo … 7 = sábado.</summary>
    public int DiaDaSemana { get; set; }

    public string NomeDoDia { get; set; } = string.Empty;
    public decimal Faturamento { get; set; }
    public int Comandas { get; set; }
}

public class RankingDesempenhoResponse
{
    public PeriodoResponse Periodo { get; set; } = new();
    public decimal PesoFaturamento { get; set; }
    public decimal PesoMesasAtendidas { get; set; }

    /// <summary>Quantos garçons trabalharam no período e entraram no ranking.</summary>
    public int TotalNoRanking { get; set; }

    /// <summary>Gerente: todos. Garçom: só a própria linha (a posição dos colegas não é exposta).</summary>
    public List<PosicaoRankingResponse> Posicoes { get; set; } = [];
}

public class PosicaoRankingResponse
{
    public int Posicao { get; set; }
    public int GarcomId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Indice { get; set; }
    public decimal FaturamentoPorTurno { get; set; }
    public decimal MesasPorTurno { get; set; }
    public int Turnos { get; set; }
}

/// <summary>
/// Avaliações do atendimento no período (RF25). Tudo agregado: a resposta não traz comentário, comanda
/// nem garçom. Ver a nota de uma mesa isolada seria, na prática, apontar para o atendimento dela.
/// </summary>
public class RelatorioAvaliacoesResponse
{
    public PeriodoResponse Periodo { get; set; } = new();

    /// <summary>Quantas avaliações chegaram no período.</summary>
    public int Quantidade { get; set; }

    /// <summary>Média das notas, com uma casa. Zero quando ninguém avaliou.</summary>
    public decimal Media { get; set; }

    /// <summary>Uma linha por nota de 1 a 5, inclusive as que ninguém deu.</summary>
    public List<FaixaDeNotaResponse> Distribuicao { get; set; } = [];
}

public class FaixaDeNotaResponse
{
    public int Nota { get; set; }
    public int Quantidade { get; set; }

    /// <summary>Participação da nota no total, em porcentagem.</summary>
    public decimal Percentual { get; set; }
}
