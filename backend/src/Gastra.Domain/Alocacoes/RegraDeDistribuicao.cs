using Gastra.Domain.Enums;

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

    /// <summary>
    /// Quanto o faturamento por turno pode se afastar da média da equipe e ainda contar como "na média": 10% para
    /// cada lado. Diferença menor que isso não explica a sugestão, e chamá-la de "abaixo" seria exagero.
    /// </summary>
    public const decimal MargemDaMedia = 0.10m;

    /// <summary>
    /// A faixa de cada garçom do turno, comparado com a média dos colegas que faturaram na janela. É a tradução do
    /// fator de desequilíbrio da RN03 para quem não pode ver o valor (o Metre): "abaixo da equipe" é quem a
    /// programação linear tende a mandar para a praça de maior movimento.
    /// </summary>
    public static IReadOnlyDictionary<int, FaixaDeFaturamento> FaixasDeFaturamento(
        IEnumerable<int> garcomIds, IReadOnlyDictionary<int, decimal> faturamentoPorTurno)
    {
        var ids = garcomIds.Distinct().ToList();
        var comHistorico = ids.Where(id => faturamentoPorTurno.GetValueOrDefault(id) > 0).ToList();
        var media = comHistorico.Count == 0 ? 0m : comHistorico.Average(id => faturamentoPorTurno[id]);

        return ids.ToDictionary(id => id, id =>
        {
            var valor = faturamentoPorTurno.GetValueOrDefault(id);
            if (valor <= 0)
                return FaixaDeFaturamento.SemHistorico;
            if (valor < media * (1 - MargemDaMedia))
                return FaixaDeFaturamento.AbaixoDaEquipe;
            return valor > media * (1 + MargemDaMedia) ? FaixaDeFaturamento.AcimaDaEquipe : FaixaDeFaturamento.NaMediaDaEquipe;
        });
    }
}
