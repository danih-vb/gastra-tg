namespace Gastra.Domain.Alocacoes;

/// <summary>
/// RN03: como os dados do histórico viram os fatores da programação linear. O cálculo da distribuição em si
/// é feito pelo serviço analítico em Python; aqui fica só o que o backend precisa decidir antes de chamá-lo.
/// </summary>
public static class RegraDeDistribuicao
{
    /// <summary>
    /// Peso do equilíbrio de faturamento (w1). O da espera é o complemento (w2 = 1 − w1). Calibrado por simulação na #55:
    /// menor peso com desigualdade até 50% acima da menor (docs/analises/GASTRA_Calibracao_Pesos_RN03.md).
    /// </summary>
    public const double PesoDesequilibrio = 0.6;

    public static double PesoEspera => Math.Round(1 - PesoDesequilibrio, 10);

    /// <summary>
    /// Janela do faturamento de cada garçom, contada para trás a partir do turno. O fator é a média por turno
    /// trabalhado nessa janela, e não a soma: com a soma, quem faltou mais parece ter faturado menos e ganha
    /// praça boa por isso (calibração #55, docs/analises/GASTRA_Calibracao_Pesos_RN03.md).
    /// </summary>
    public const int DiasDaJanelaDeFaturamento = 30;

    /// <summary>
    /// Praças de alto potencial: faturamento médio por turno acima da média das praças que já tiveram
    /// movimento. Sem histórico (ou com todas iguais), nenhuma praça é de alto potencial, e a espera não
    /// desempata nada.
    /// </summary>
    public static IReadOnlySet<int> PracasDeAltoPotencial(IReadOnlyDictionary<int, decimal> faturamentoMedioPorPraca)
    {
        var comMovimento = faturamentoMedioPorPraca.Where(p => p.Value > 0).ToList();
        if (comMovimento.Count == 0)
            return new HashSet<int>();

        var media = comMovimento.Average(p => p.Value);
        return comMovimento.Where(p => p.Value > media).Select(p => p.Key).ToHashSet();
    }

    /// <summary>
    /// Quantos turnos confirmados o garçom trabalhou desde a última vez numa praça de alto potencial.
    /// Quem nunca esteve numa delas acumula todos os turnos que já trabalhou.
    /// </summary>
    /// <param name="pracasDosTurnosAnteriores">Praças dos turnos confirmados do garçom, do mais recente para o mais antigo.</param>
    public static int TurnosDesdePracaDeAltoPotencial(IEnumerable<int> pracasDosTurnosAnteriores, IReadOnlySet<int> altoPotencial)
    {
        var turnos = 0;
        foreach (var praca in pracasDosTurnosAnteriores)
        {
            if (altoPotencial.Contains(praca))
                return turnos;
            turnos++;
        }

        return turnos;
    }
}
