using Gastra.Domain.Alocacoes;
using Gastra.Domain.Enums;

namespace Gastra.Domain.Tests.Alocacoes;

public class RegraDeDistribuicaoTests
{
    [Fact]
    public void PesosSomamUm()
    {
        Assert.Equal(1.0, RegraDeDistribuicao.PesoDesequilibrio + RegraDeDistribuicao.PesoEspera, precision: 10);
    }

    [Fact]
    public void AltoPotencial_SaoAsPracasAcimaDaMediaDasQueTiveramMovimento()
    {
        var faturamento = new Dictionary<int, decimal> { [1] = 1800m, [2] = 1200m, [3] = 600m, [4] = 0m };

        // Média das que tiveram movimento: 1200. A praça 4, nova, não puxa a média para baixo.
        Assert.Equal([1], RegraDeDistribuicao.PracasDeAltoPotencial(faturamento).Order());
    }

    [Fact]
    public void AltoPotencial_SemHistorico_NenhumaPraca()
    {
        var faturamento = new Dictionary<int, decimal> { [1] = 0m, [2] = 0m };

        Assert.Empty(RegraDeDistribuicao.PracasDeAltoPotencial(faturamento));
    }

    [Fact]
    public void AltoPotencial_TodasIguais_NenhumaPraca()
    {
        var faturamento = new Dictionary<int, decimal> { [1] = 900m, [2] = 900m };

        Assert.Empty(RegraDeDistribuicao.PracasDeAltoPotencial(faturamento));
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, 0)] // o último turno já foi na praça boa
    [InlineData(new[] { 2, 3, 1, 2 }, 2)] // dois turnos em praças fracas desde a última vez na boa
    [InlineData(new[] { 2, 3, 3 }, 3)] // nunca esteve na praça boa: conta todos os turnos
    [InlineData(new int[0], 0)] // garçom novo
    public void TurnosDesdePracaDeAltoPotencial(int[] pracasDoMaisRecente, int esperado)
    {
        var altoPotencial = new HashSet<int> { 1 };

        Assert.Equal(esperado, RegraDeDistribuicao.TurnosDesdePracaDeAltoPotencial(pracasDoMaisRecente, altoPotencial));
    }

    // --- Faixa de faturamento: o que o Metre vê no lugar do valor ---

    [Fact]
    public void Faixas_ComparamComAMediaDosColegasDoTurno()
    {
        // Média de quem faturou: (800 + 1000 + 1200 + 1000) / 4 = 1000; ±10% é "na média".
        var faturamento = new Dictionary<int, decimal> { [1] = 800m, [2] = 1000m, [3] = 1200m, [4] = 1000m, [9] = 5000m };

        var faixas = RegraDeDistribuicao.FaixasDeFaturamento([1, 2, 3, 4], faturamento);

        Assert.Equal(FaixaDeFaturamento.AbaixoDaEquipe, faixas[1]);
        Assert.Equal(FaixaDeFaturamento.NaMediaDaEquipe, faixas[2]);
        Assert.Equal(FaixaDeFaturamento.AcimaDaEquipe, faixas[3]);
        // O garçom 9 não está no turno: não entra na média.
        Assert.False(faixas.ContainsKey(9));
    }

    [Fact]
    public void Faixas_DiferencaPequenaAindaENaMedia()
    {
        var faturamento = new Dictionary<int, decimal> { [1] = 950m, [2] = 1050m };

        var faixas = RegraDeDistribuicao.FaixasDeFaturamento([1, 2], faturamento);

        Assert.All(faixas.Values, f => Assert.Equal(FaixaDeFaturamento.NaMediaDaEquipe, f));
    }

    [Fact]
    public void Faixas_QuemNaoFaturouNaJanelaFicaSemHistoricoENaoPuxaAMedia()
    {
        var faturamento = new Dictionary<int, decimal> { [1] = 1000m, [2] = 1000m };

        var faixas = RegraDeDistribuicao.FaixasDeFaturamento([1, 2, 3], faturamento);

        Assert.Equal(FaixaDeFaturamento.SemHistorico, faixas[3]);
        Assert.Equal(FaixaDeFaturamento.NaMediaDaEquipe, faixas[1]);
    }
}
