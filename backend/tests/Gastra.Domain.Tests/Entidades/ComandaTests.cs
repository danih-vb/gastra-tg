using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;

namespace Gastra.Domain.Tests.Entidades;

public class ComandaTests
{
    private const int Mesa = 1;
    private const int Garcom = 2;

    private static Comanda AbrirComanda(int pessoas = 2) => new(Mesa, Garcom, pessoas);

    private static ItemDoCardapio Item(decimal preco = 50m, bool disponivel = true, params FlagDietetica[] flags)
    {
        var item = new ItemDoCardapio("Moqueca", CategoriaItemCardapio.PratoPrincipal, preco, "Descrição", flags);
        if (!disponivel)
            item.MarcarDisponibilidade(false);
        return item;
    }

    // --- RN01: composição da mesa ---

    [Theory]
    [InlineData(1, ComposicaoMesa.Solo)]
    [InlineData(2, ComposicaoMesa.Casal)]
    [InlineData(3, ComposicaoMesa.GrupoPequeno)]
    [InlineData(4, ComposicaoMesa.GrupoPequeno)]
    [InlineData(5, ComposicaoMesa.GrupoGrande)]
    [InlineData(9, ComposicaoMesa.GrupoGrande)]
    public void Abrir_SugereAComposicaoPelaQuantidadeDePessoas(int pessoas, ComposicaoMesa esperada)
    {
        Assert.Equal(esperada, AbrirComanda(pessoas).Composicao);
    }

    [Fact]
    public void AdicionarItemInfantil_EmMesaDeTresOuMais_ReclassificaComoFamilia()
    {
        var comanda = AbrirComanda(3);

        comanda.AdicionarItem(Item(flags: FlagDietetica.OpcaoInfantil), 1);

        Assert.Equal(ComposicaoMesa.Familia, comanda.Composicao);
    }

    [Fact]
    public void AdicionarItemInfantil_EmMesaDeDuasPessoas_NaoViraFamilia()
    {
        var comanda = AbrirComanda(2);

        comanda.AdicionarItem(Item(flags: FlagDietetica.OpcaoInfantil), 1);

        Assert.Equal(ComposicaoMesa.Casal, comanda.Composicao);
    }

    [Fact]
    public void ConfirmarComposicao_GuardaOAjusteDoGarcom()
    {
        var comanda = AbrirComanda(4);

        comanda.ConfirmarComposicao(5, ComposicaoMesa.GrupoGrande);

        Assert.Equal(5, comanda.QuantidadePessoas);
        Assert.Equal(ComposicaoMesa.GrupoGrande, comanda.Composicao);
    }

    [Fact]
    public void Abrir_ComZeroPessoas_LancaExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AbrirComanda(0));
    }

    // --- RF03: itens do pedido ---

    [Fact]
    public void AdicionarItem_CopiaOPrecoDoMomentoENaoMudaDepois()
    {
        var comanda = AbrirComanda();
        var item = Item(50m);

        var pedido = comanda.AdicionarItem(item, 2);
        item.AtualizarPreco(80m);

        Assert.Equal(50m, pedido.PrecoUnitarioNoMomento);
        Assert.Equal(100m, comanda.CalcularSubtotal());
        Assert.Equal(StatusItemPedido.Pendente, pedido.Status);
    }

    [Fact]
    public void AdicionarItem_Indisponivel_LancaExcecao()
    {
        var comanda = AbrirComanda();

        Assert.Throws<InvalidOperationException>(() => comanda.AdicionarItem(Item(disponivel: false), 1));
    }

    [Fact]
    public void ItemCancelado_NaoEntraNaConta_ENaoImpedeOFechamento()
    {
        var comanda = AbrirComanda();
        var pedido = comanda.AdicionarItem(Item(50m), 1);

        pedido.Cancelar(MotivoCancelamento.ItemEmFalta);

        Assert.Equal(0m, comanda.CalcularSubtotal());
        Assert.False(comanda.PossuiPendencias());
        Assert.Equal(MotivoCancelamento.ItemEmFalta, pedido.MotivoCancelamento);
    }

    // --- RF04: taxa de serviço ---

    [Fact]
    public void CalcularTotal_SomaTaxaDeDezPorCento()
    {
        var comanda = AbrirComanda();
        comanda.AdicionarItem(Item(89.90m), 1);

        Assert.Equal(89.90m, comanda.CalcularSubtotal());
        Assert.Equal(8.99m, comanda.CalcularTaxaServico());
        Assert.Equal(98.89m, comanda.CalcularTotal());
    }

    [Fact]
    public void RemoverTaxaServico_ZeraATaxa()
    {
        var comanda = AbrirComanda();
        comanda.AdicionarItem(Item(100m), 1);

        comanda.RemoverTaxaServico();

        Assert.True(comanda.TaxaServicoRemovida);
        Assert.Equal(0m, comanda.CalcularTaxaServico());
        Assert.Equal(100m, comanda.CalcularTotal());
    }

    // --- RN02 e UC14: fechamento ---

    [Fact]
    public void Fechar_ComItemPendente_LancaExcecao()
    {
        var comanda = AbrirComanda();
        comanda.AdicionarItem(Item(), 1);

        Assert.True(comanda.PossuiPendencias());
        Assert.Throws<InvalidOperationException>(comanda.Fechar);
        Assert.Equal(StatusComanda.Aberta, comanda.Status);
    }

    [Fact]
    public void Fechar_ComTodosOsItensEntregues_FechaERegistraAHora()
    {
        var comanda = AbrirComanda();
        comanda.AdicionarItem(Item(), 1).MarcarEntregue();

        comanda.Fechar();

        Assert.Equal(StatusComanda.Fechada, comanda.Status);
        Assert.NotNull(comanda.DataHoraFechamento);
    }

    [Fact]
    public void Fechar_ApagaAObservacaoLivreDaRestricaoEMantemACategoria()
    {
        var comanda = AbrirComanda();
        var restricao = comanda.RegistrarRestricao(CategoriaRestricao.Alergia, "alergia a camarão");

        comanda.Fechar();

        Assert.Null(restricao.ObservacaoLivre);
        Assert.Equal(CategoriaRestricao.Alergia, restricao.Categoria);
    }

    [Fact]
    public void ComandaFechada_NaoAceitaNovoItemNemRestricao()
    {
        var comanda = AbrirComanda();
        comanda.Fechar();

        Assert.Throws<InvalidOperationException>(() => comanda.AdicionarItem(Item(), 1));
        Assert.Throws<InvalidOperationException>(() => comanda.RegistrarRestricao(CategoriaRestricao.Vegano, null));
        Assert.Throws<InvalidOperationException>(comanda.RemoverTaxaServico);
    }

    // --- RF14: restrição alimentar ---

    [Fact]
    public void RegistrarRestricao_GuardaCategoriaEObservacaoSemEspacosSobrando()
    {
        var comanda = AbrirComanda();

        var restricao = comanda.RegistrarRestricao(CategoriaRestricao.SemGluten, "  sem molho de soja  ");

        Assert.Single(comanda.Restricoes);
        Assert.Equal("sem molho de soja", restricao.ObservacaoLivre);
    }

    [Fact]
    public void RegistrarRestricao_SemObservacao_FicaNula()
    {
        var comanda = AbrirComanda();

        var restricao = comanda.RegistrarRestricao(CategoriaRestricao.Vegano, "   ");

        Assert.Null(restricao.ObservacaoLivre);
    }
}
