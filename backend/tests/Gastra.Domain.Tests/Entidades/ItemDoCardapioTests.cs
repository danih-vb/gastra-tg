using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;

namespace Gastra.Domain.Tests.Entidades;

public class ItemDoCardapioTests
{
    private static ItemDoCardapio CriarItem(decimal preco = 89.90m, params FlagDietetica[] flags) =>
        new("Risoto de cogumelos", CategoriaItemCardapio.PratoPrincipal, preco, "Arroz arbóreo com shimeji", flags);

    [Fact]
    public void Criar_ComDadosValidos_FicaDisponivel()
    {
        var item = CriarItem(89.90m, FlagDietetica.Vegetariano);

        Assert.True(item.Disponivel);
        Assert.Equal(89.90m, item.Preco);
        Assert.Equal([FlagDietetica.Vegetariano], item.FlagsDieteticas);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Criar_ComPrecoZeroOuNegativo_LancaExcecao(decimal preco)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CriarItem(preco));
    }

    [Fact]
    public void Criar_ComFlagRepetida_GuardaUmaVezSo()
    {
        var item = CriarItem(50m, FlagDietetica.Vegano, FlagDietetica.Vegano, FlagDietetica.SemGluten);

        Assert.Equal(2, item.FlagsDieteticas.Count);
    }

    [Fact]
    public void AtualizarPreco_ComValorValido_AlteraPreco()
    {
        var item = CriarItem(89.90m);

        item.AtualizarPreco(95m);

        Assert.Equal(95m, item.Preco);
    }

    [Fact]
    public void AtualizarPreco_ComValorInvalido_MantemPrecoAnterior()
    {
        var item = CriarItem(89.90m);

        Assert.Throws<ArgumentOutOfRangeException>(() => item.AtualizarPreco(-1m));
        Assert.Equal(89.90m, item.Preco);
    }

    [Fact]
    public void MarcarDisponibilidade_Falso_TornaItemIndisponivel()
    {
        var item = CriarItem();

        item.MarcarDisponibilidade(false);

        Assert.False(item.Disponivel);
    }

    [Fact]
    public void FlagsDieteticas_NaoPodeSerAlteradaDeFora()
    {
        var item = CriarItem(50m, FlagDietetica.Vegano);

        Assert.IsAssignableFrom<IReadOnlyCollection<FlagDietetica>>(item.FlagsDieteticas);
        Assert.Null(typeof(ItemDoCardapio).GetProperty(nameof(ItemDoCardapio.Preco))!.GetSetMethod());
    }
}
