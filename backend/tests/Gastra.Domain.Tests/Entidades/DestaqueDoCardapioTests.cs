using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;

namespace Gastra.Domain.Tests.Entidades;

public class DestaqueDoCardapioTests
{
    [Fact]
    public void MetaDoMes_ValeDoPrimeiroAoUltimoDiaDoMes()
    {
        var meta = new DestaqueDoCardapio(7, TipoDestaque.MetaDoMes, new DateOnly(2026, 11, 21), 40);

        Assert.Equal(new DateOnly(2026, 11, 1), meta.DataInicio);
        Assert.Equal(new DateOnly(2026, 11, 30), meta.DataFim);
        Assert.Equal(40, meta.Meta);
        Assert.True(meta.VigenteEm(new DateOnly(2026, 11, 30)));
        Assert.False(meta.VigenteEm(new DateOnly(2026, 12, 1)));
    }

    [Fact]
    public void DestaqueDoDia_ValeSoNaqueleDia_ESemMeta()
    {
        var destaque = new DestaqueDoCardapio(7, TipoDestaque.DestaqueDoDia, new DateOnly(2026, 11, 23), 99);

        Assert.Equal(destaque.DataInicio, destaque.DataFim);
        Assert.Null(destaque.Meta);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(10_001)]
    public void MetaDoMes_SemMetaValida_LancaExcecao(int? meta)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DestaqueDoCardapio(7, TipoDestaque.MetaDoMes, new DateOnly(2026, 11, 1), meta));
    }

    [Fact]
    public void ColideCom_MesmoItemMesmoTipoNoMesmoPeriodo()
    {
        var meta = new DestaqueDoCardapio(7, TipoDestaque.MetaDoMes, new DateOnly(2026, 11, 2), 40);

        Assert.True(meta.ColideCom(new DestaqueDoCardapio(7, TipoDestaque.MetaDoMes, new DateOnly(2026, 11, 28), 10)));
        Assert.False(meta.ColideCom(new DestaqueDoCardapio(7, TipoDestaque.DestaqueDoDia, new DateOnly(2026, 11, 28), null)));
        Assert.False(meta.ColideCom(new DestaqueDoCardapio(8, TipoDestaque.MetaDoMes, new DateOnly(2026, 11, 28), 10)));
        Assert.False(meta.ColideCom(new DestaqueDoCardapio(7, TipoDestaque.MetaDoMes, new DateOnly(2026, 12, 1), 10)));
    }
}
