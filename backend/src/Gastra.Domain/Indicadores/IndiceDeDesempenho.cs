namespace Gastra.Domain.Indicadores;

/// <summary>
/// Base do índice de um garçom no período, lida das views. <paramref name="Avaliacoes"/> e
/// <paramref name="SomaDasNotas"/> vêm das avaliações do atendimento (RF25) das comandas que ele atendeu.
/// </summary>
public record DesempenhoNoPeriodo(
    int GarcomId, decimal Faturamento, int Turnos, int MesasAtendidas, int Avaliacoes = 0, int SomaDasNotas = 0);

/// <summary>
/// Posição de um garçom no ranking do período. <see cref="NotaConsiderada"/> é nula quando ele teve menos avaliações
/// que o mínimo: nesse caso o índice usou a média geral, e a nota dele não é mostrada.
/// </summary>
public record PosicaoNoRanking(
    int GarcomId, int Posicao, decimal Indice, decimal FaturamentoPorTurno, decimal MesasPorTurno, int Turnos,
    decimal? NotaConsiderada = null);

/// <summary>Pesos usados no período; somam 1.</summary>
public record PesosDoIndice(decimal Faturamento, decimal MesasAtendidas, decimal Avaliacao);

public record RankingDeDesempenho(PesosDoIndice Pesos, IReadOnlyList<PosicaoNoRanking> Posicoes);

/// <summary>
/// RF11 — índice composto de desempenho: combina faturamento, mesas atendidas e a avaliação do cliente (RF25),
/// "não apenas venda".
/// </summary>
/// <remarks>
/// <para>
/// Faturamento e mesas são <b>por turno trabalhado</b>. Com totais, quem trabalhou mais turnos ficaria na frente só
/// por ter trabalhado mais; é o mesmo problema que a calibração da RN03 mostrou na alocação (#55). Cada uma é dividida
/// pelo maior valor do período e vai de 0 a 1.
/// </para>
/// <para>
/// A avaliação usa a <b>escala da nota</b> (1 vira 0, 5 vira 1), e não a divisão pelo maior: entre 4,6 e 4,5 a
/// diferença é pequena, e dividir pelo maior a faria parecer grande.
/// </para>
/// <para>
/// A nota de cada garçom é uma <b>média bayesiana</b>: soma-se às avaliações dele <see cref="PesoDaMediaGeral"/>
/// avaliações "imaginárias" iguais à média geral do período. Com poucas avaliações, a nota fica perto da média geral;
/// com muitas, fica perto da dele. Sem isso, cinco notas 5 valeriam mais que quarenta notas 4,8.
/// </para>
/// <para>
/// <b>Privacidade (RN08):</b> abaixo de <see cref="MinimoDeAvaliacoes"/> no período, a nota do garçom não é usada nem
/// mostrada: ele recebe a média geral, neutra. Com uma só avaliação, a "média do garçom" seria a nota daquela mesa,
/// e quem sabe que mesa ele atendeu saberia a nota que ela deu.
/// </para>
/// <para>
/// Sem nenhum garçom com o mínimo de avaliações, a avaliação sai do índice e os pesos voltam a ser metade faturamento,
/// metade mesas. O índice só compara garçons dentro do mesmo período, nunca entre períodos.
/// </para>
/// </remarks>
public static class IndiceDeDesempenho
{
    public static readonly PesosDoIndice ComAvaliacao = new(Faturamento: 0.4m, MesasAtendidas: 0.3m, Avaliacao: 0.3m);

    /// <summary>Quando ninguém tem avaliações suficientes: metade e metade, nenhum dos dois domina.</summary>
    public static readonly PesosDoIndice SemAvaliacao = new(Faturamento: 0.5m, MesasAtendidas: 0.5m, Avaliacao: 0m);

    /// <summary>Abaixo disto, a nota do garçom no período não é usada nem mostrada (RN08).</summary>
    public const int MinimoDeAvaliacoes = 5;

    /// <summary>Quantas avaliações "imaginárias" na média geral entram na média bayesiana de cada garçom.</summary>
    public const int PesoDaMediaGeral = 5;

    public static RankingDeDesempenho Calcular(IEnumerable<DesempenhoNoPeriodo> desempenhos)
    {
        var comTurnos = desempenhos.Where(d => d.Turnos > 0).ToList();

        if (comTurnos.Count == 0)
            return new RankingDeDesempenho(SemAvaliacao, []);

        var totalAvaliacoes = comTurnos.Sum(d => d.Avaliacoes);
        var mediaGeral = totalAvaliacoes == 0 ? 0m : (decimal)comTurnos.Sum(d => d.SomaDasNotas) / totalAvaliacoes;
        var pesos = comTurnos.Any(d => d.Avaliacoes >= MinimoDeAvaliacoes) ? ComAvaliacao : SemAvaliacao;

        decimal? NotaDe(DesempenhoNoPeriodo d) => d.Avaliacoes < MinimoDeAvaliacoes
            ? null
            : Math.Round((PesoDaMediaGeral * mediaGeral + d.SomaDasNotas) / (PesoDaMediaGeral + d.Avaliacoes), 2);

        var porTurno = comTurnos
            .Select(d => (d.GarcomId, d.Turnos, Faturamento: d.Faturamento / d.Turnos,
                Mesas: (decimal)d.MesasAtendidas / d.Turnos, Nota: NotaDe(d)))
            .ToList();

        var maiorFaturamento = porTurno.Max(d => d.Faturamento);
        var maiorMesas = porTurno.Max(d => d.Mesas);

        var ordenados = porTurno
            .Select(d => (d.GarcomId, d.Turnos, d.Faturamento, d.Mesas, d.Nota,
                Indice: Math.Round(100 * (pesos.Faturamento * Proporcao(d.Faturamento, maiorFaturamento)
                                          + pesos.MesasAtendidas * Proporcao(d.Mesas, maiorMesas)
                                          + pesos.Avaliacao * NaEscala(d.Nota ?? mediaGeral)), 1)))
            .OrderByDescending(d => d.Indice).ThenBy(d => d.GarcomId)
            .ToList();

        // Empate no índice divide a posição (1, 1, 3), como em qualquer classificação.
        var posicoes = ordenados
            .Select(d => new PosicaoNoRanking(
                d.GarcomId,
                ordenados.Count(outro => outro.Indice > d.Indice) + 1,
                d.Indice,
                Math.Round(d.Faturamento, 2),
                Math.Round(d.Mesas, 2),
                d.Turnos,
                d.Nota))
            .ToList();

        return new RankingDeDesempenho(pesos, posicoes);
    }

    private static decimal Proporcao(decimal valor, decimal maior) => maior == 0 ? 0 : valor / maior;

    /// <summary>Nota de 1 a 5 levada para 0 a 1.</summary>
    private static decimal NaEscala(decimal nota) => nota == 0 ? 0 : (nota - 1) / 4;
}
