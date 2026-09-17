using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;

namespace Gastra.Domain.Tests.Entidades;

public class PromocaoTests
{
    private static readonly DateOnly Inicio = new(2026, 9, 10);
    private static readonly DateOnly Fim = new(2026, 9, 20);

    private static ItemDoCardapio Item(int id, decimal preco)
    {
        var item = new ItemDoCardapio($"Item {id}", CategoriaItemCardapio.PratoPrincipal, preco, "", []);
        typeof(EntidadeBase).GetProperty(nameof(EntidadeBase.Id))!.SetValue(item, id);
        return item;
    }

    private static Promocao Criar(TipoDesconto tipo, decimal valor, params ItemDoCardapio[] itens) =>
        new("Semana da moqueca", tipo, valor, Inicio, Fim, itens);

    [Theory]
    [InlineData(TipoDesconto.Percentual, 10, 89.90, 80.91)]
    [InlineData(TipoDesconto.Percentual, 33.33, 10.00, 6.67)]
    [InlineData(TipoDesconto.ValorFixo, 15, 89.90, 74.90)]
    public void AplicarDesconto_ArredondaEmCentavos(TipoDesconto tipo, decimal valor, decimal preco, decimal esperado)
    {
        var promocao = Criar(tipo, valor, Item(1, 100m));

        Assert.Equal(esperado, promocao.AplicarDesconto(preco));
    }

    [Fact]
    public void AplicarDesconto_NuncaDeixaOItemDeGraca()
    {
        var promocao = Criar(TipoDesconto.ValorFixo, 20m, Item(1, 50m));

        // O preço do item pode cair depois da promoção criada; mesmo assim o desconto não zera o preço.
        Assert.Equal(Promocao.PrecoMinimo, promocao.AplicarDesconto(15m));
    }

    [Theory]
    [InlineData("2026-09-09", false)]
    [InlineData("2026-09-10", true)]
    [InlineData("2026-09-20", true)]
    [InlineData("2026-09-21", false)]
    public void VigenteEm_IncluiOsDoisExtremos(string data, bool esperado)
    {
        Assert.Equal(esperado, Criar(TipoDesconto.Percentual, 10m, Item(1, 50m)).VigenteEm(DateOnly.Parse(data)));
    }

    [Fact]
    public void Desativada_DeixaDeValer_ENaoDesativaDuasVezes()
    {
        var promocao = Criar(TipoDesconto.Percentual, 10m, Item(1, 50m));

        promocao.Desativar();

        Assert.False(promocao.VigenteEm(Inicio));
        Assert.Throws<InvalidOperationException>(promocao.Desativar);
    }

    [Fact]
    public void PrecoPromocional_ComDuasPromocoes_ValeADeMaiorDesconto()
    {
        var moqueca = Item(1, 100m);
        var dezPorCento = Criar(TipoDesconto.Percentual, 10m, moqueca);
        var quinzeReais = Criar(TipoDesconto.ValorFixo, 15m, moqueca);

        Assert.Equal(85m, Promocao.PrecoPromocional(moqueca, [dezPorCento, quinzeReais], Inicio));
    }

    [Fact]
    public void PrecoPromocional_SemPromocaoParaOItemOuForaDoPeriodo_ENulo()
    {
        var moqueca = Item(1, 100m);
        var suco = Item(2, 10m);
        var promocao = Criar(TipoDesconto.Percentual, 10m, moqueca);

        Assert.Null(Promocao.PrecoPromocional(suco, [promocao], Inicio));
        Assert.Null(Promocao.PrecoPromocional(moqueca, [promocao], Fim.AddDays(1)));
    }

    [Theory]
    [InlineData(TipoDesconto.Percentual, 0)]
    [InlineData(TipoDesconto.Percentual, 100)]
    [InlineData(TipoDesconto.ValorFixo, 50)]
    public void Criar_ComDescontoInvalido_LancaExcecao(TipoDesconto tipo, decimal valor)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Criar(tipo, valor, Item(1, 50m)));
    }

    [Fact]
    public void Criar_ComFimAntesDoInicio_OuSemItens_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new Promocao("X", TipoDesconto.Percentual, 10m, Fim, Inicio, [Item(1, 50m)]));
        Assert.Throws<ArgumentException>(() => new Promocao("X", TipoDesconto.Percentual, 10m, Inicio, Fim, []));
    }

    [Fact]
    public void Comanda_GuardaOPrecoPromocionalNoPedido_MasNuncaAcimaDoCardapio()
    {
        var comanda = new Comanda(mesaId: 1, garcomId: 2, quantidadePessoas: 2);
        var moqueca = Item(1, 100m);

        var pedido = comanda.AdicionarItem(moqueca, 2, precoPromocional: 85m);

        Assert.Equal(85m, pedido.PrecoUnitarioNoMomento);
        Assert.Equal(170m, comanda.CalcularSubtotal());
        Assert.Throws<ArgumentOutOfRangeException>(() => comanda.AdicionarItem(moqueca, 1, precoPromocional: 120m));
    }
}
