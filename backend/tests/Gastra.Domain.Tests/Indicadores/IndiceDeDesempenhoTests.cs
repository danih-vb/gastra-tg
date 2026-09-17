using Gastra.Domain.Indicadores;

namespace Gastra.Domain.Tests.Indicadores;

public class IndiceDeDesempenhoTests
{
    [Fact]
    public void QuemLideraFaturamentoEMesasPorTurno_Faz100()
    {
        var ranking = IndiceDeDesempenho.Calcular([
            new DesempenhoNoPeriodo(1, Faturamento: 1000m, Turnos: 2, MesasAtendidas: 10),
            new DesempenhoNoPeriodo(2, Faturamento: 500m, Turnos: 2, MesasAtendidas: 5),
        ]);

        Assert.Equal((1, 1, 100m), (ranking[0].GarcomId, ranking[0].Posicao, ranking[0].Indice));
        Assert.Equal((2, 2, 50m), (ranking[1].GarcomId, ranking[1].Posicao, ranking[1].Indice));
    }

    [Fact]
    public void ComparaPorTurno_QuemTrabalhouMaisNaoGanhaSoPorIsso()
    {
        // O garçom 1 faturou o dobro, mas em quatro vezes mais turnos.
        var ranking = IndiceDeDesempenho.Calcular([
            new DesempenhoNoPeriodo(1, Faturamento: 2000m, Turnos: 8, MesasAtendidas: 16),
            new DesempenhoNoPeriodo(2, Faturamento: 1000m, Turnos: 2, MesasAtendidas: 4),
        ]);

        Assert.Equal(2, ranking[0].GarcomId);
        Assert.Equal(500m, ranking[0].FaturamentoPorTurno);
        Assert.Equal(250m, ranking[1].FaturamentoPorTurno);
    }

    [Fact]
    public void NaoEApenasVenda_MesasAtendidasPesamMetade()
    {
        // O 1 vende mais por turno; o 2 atende o dobro de mesas. Com 50/50, o 2 fica na frente.
        var ranking = IndiceDeDesempenho.Calcular([
            new DesempenhoNoPeriodo(1, Faturamento: 1000m, Turnos: 1, MesasAtendidas: 2),
            new DesempenhoNoPeriodo(2, Faturamento: 800m, Turnos: 1, MesasAtendidas: 4),
        ]);

        // Garçom 2: 0,5 × (800/1000) + 0,5 × (4/4) = 0,90. Garçom 1: 0,5 × 1 + 0,5 × (2/4) = 0,75.
        Assert.Equal((2, 90m), (ranking[0].GarcomId, ranking[0].Indice));
        Assert.Equal((1, 75m), (ranking[1].GarcomId, ranking[1].Indice));
    }

    [Fact]
    public void EmpateNoIndice_DivideAPosicao()
    {
        var ranking = IndiceDeDesempenho.Calcular([
            new DesempenhoNoPeriodo(1, 600m, 1, 3),
            new DesempenhoNoPeriodo(2, 600m, 1, 3),
            new DesempenhoNoPeriodo(3, 300m, 1, 1),
        ]);

        Assert.Equal([1, 1, 3], ranking.Select(p => p.Posicao));
    }

    [Fact]
    public void SemTurnos_FicaForaDoRanking()
    {
        Assert.Empty(IndiceDeDesempenho.Calcular([new DesempenhoNoPeriodo(1, 0m, 0, 0)]));
    }
}
