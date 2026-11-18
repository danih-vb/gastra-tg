using Gastra.Domain.Enums;
using Gastra.Domain.Servicos;

namespace Gastra.Domain.Tests.Indicadores;

public class EngenhariaDeCardapioTests
{
    // Categoria com 4 itens, 100 unidades e R$ 5.000: a fatia de popularidade é 17,5 unidades, e o valor médio, R$ 50.
    [Theory]
    [InlineData(40, 2800, ClassificacaoDoItem.Estrela)]
    [InlineData(40, 1200, ClassificacaoDoItem.BurroDeCarga)]
    [InlineData(10, 700, ClassificacaoDoItem.QuebraCabeca)]
    [InlineData(10, 300, ClassificacaoDoItem.Cao)]
    public void Classificar_PelosDoisEixos(int quantidade, int faturamento, ClassificacaoDoItem esperada)
    {
        Assert.Equal(esperada, EngenhariaDeCardapio.Classificar(quantidade, faturamento, 4, 100, 5000));
    }

    [Fact]
    public void Classificar_ExatamenteNosLimites_ContaComoAlto()
    {
        Assert.Equal(ClassificacaoDoItem.Estrela, EngenhariaDeCardapio.Classificar(17.5m, 875m, 4, 100, 5000));
    }

    [Fact]
    public void Classificar_SemVenda_NaoDividePorZero()
    {
        Assert.Equal(ClassificacaoDoItem.Cao, EngenhariaDeCardapio.Classificar(0, 0, 4, 0, 0));
    }
}
