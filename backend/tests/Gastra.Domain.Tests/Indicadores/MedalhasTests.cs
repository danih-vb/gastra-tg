using Gastra.Domain.Indicadores;

namespace Gastra.Domain.Tests.Indicadores;

public class MedalhasTests
{
    private static Medalha Medalha(DesempenhoDoMes mes, string codigo) =>
        Medalhas.Calcular(mes).Single(m => m.Codigo == codigo);

    private static readonly DesempenhoDoMes Vazio = new(null, 0, 0, 0, 0, 0, false);

    [Fact]
    public void SeisMedalhas_SemNenhumaConquistadaNumMesVazio()
    {
        var medalhas = Medalhas.Calcular(Vazio);

        Assert.Equal(6, medalhas.Count);
        Assert.DoesNotContain(medalhas, m => m.Conquistada);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    public void Top3_SoAteATerceiraPosicao(int posicao, bool conquistada)
    {
        Assert.Equal(conquistada, Medalha(Vazio with { Posicao = posicao }, "top-3").Conquistada);
    }

    [Fact]
    public void MesaCheia_MostraOProgressoAteOAlvo()
    {
        var parcial = Medalha(Vazio with { Mesas = 64 }, "mesa-cheia");
        var cheia = Medalha(Vazio with { Mesas = 130 }, "mesa-cheia");

        Assert.Equal((false, 64m, 100m), (parcial.Conquistada, parcial.Progresso, parcial.Alvo));
        Assert.Equal((true, 100m), (cheia.Conquistada, cheia.Progresso));
    }

    [Fact]
    public void BemAvaliado_ExigeOMinimoDeAvaliacoesEAMedia()
    {
        Assert.False(Medalha(Vazio with { Avaliacoes = 4, SomaDasNotas = 20 }, "bem-avaliado").Conquistada);
        Assert.False(Medalha(Vazio with { Avaliacoes = 5, SomaDasNotas = 21 }, "bem-avaliado").Conquistada);
        Assert.True(Medalha(Vazio with { Avaliacoes = 5, SomaDasNotas = 23 }, "bem-avaliado").Conquistada);
    }

    [Fact]
    public void MetaBatida_EDaEquipeToda()
    {
        Assert.True(Medalha(Vazio with { EquipeBateuMeta = true }, "meta-batida").Conquistada);
    }
}
