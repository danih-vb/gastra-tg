using Gastra.Domain.Alocacoes;

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
}
