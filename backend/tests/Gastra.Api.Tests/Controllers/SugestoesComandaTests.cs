using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Communication.Enums;
using Gastra.Communication.Responses;
using static Gastra.Api.Tests.Infraestrutura.GastraApiFactory;
using FlagDietetica = Gastra.Domain.Enums.FlagDietetica;

namespace Gastra.Api.Tests.Controllers;

/// <summary>
/// UC18 — sugestão de combinação de pratos (RF09). O Python é substituído por
/// <see cref="ServicoAnaliticoFalso"/>; o contrato HTTP real é testado em ServicoAnaliticoHttpTests.
/// </summary>
public class SugestoesComandaTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>, IAsyncLifetime
{
    private const string Rota = "/api/comandas";

    private readonly HttpClient _cliente = factory.CreateClient();

    public async Task InitializeAsync()
    {
        factory.ServicoAnalitico.Reiniciar();
        Autenticar(_cliente, await factory.TokenGarcom(factory.CreateClient()));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<int> AbrirComanda()
    {
        var resposta = await _cliente.PostAsJsonAsync(Rota,
            new { mesaId = await factory.CriarMesa(), quantidadePessoas = 2 }, Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<ComandaResponse>(Json))!.Id;
    }

    private async Task<int> LancarItem(int comandaId, int itemCardapioId)
    {
        var resposta = await _cliente.PostAsJsonAsync($"{Rota}/{comandaId}/itens",
            new { itemCardapioId, quantidade = 1 }, Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<ItemPedidoResponse>(Json))!.Id;
    }

    private Task RegistrarRestricao(int comandaId, string categoria) =>
        _cliente.PostAsJsonAsync($"{Rota}/{comandaId}/restricoes", new { categoria }, Json);

    private async Task<SugestoesComandaResponse> Sugestoes(int comandaId)
    {
        var resposta = await _cliente.GetAsync($"{Rota}/{comandaId}/sugestoes");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<SugestoesComandaResponse>(Json))!;
    }

    [Fact]
    public async Task DevolveOsItensNaOrdemDoServico_ComNomeCategoriaEPreco()
    {
        var comanda = await AbrirComanda();
        await LancarItem(comanda, await factory.CriarItemCardapio(preco: 90m));
        var arroz = await factory.CriarItemCardapio(preco: 18m);
        var suco = await factory.CriarItemCardapio(preco: 12m);
        factory.ServicoAnalitico.Responder = _ => [suco, arroz];

        var sugestoes = await Sugestoes(comanda);

        Assert.True(sugestoes.ServicoDisponivel);
        Assert.Equal([suco, arroz], sugestoes.Itens.Select(i => i.ItemDoCardapioId));
        Assert.Equal(12m, sugestoes.Itens[0].Preco);
        Assert.Equal(CategoriaItemCardapio.PratoPrincipal, sugestoes.Itens[0].Categoria);
        Assert.StartsWith("Prato", sugestoes.Itens[0].Nome);
    }

    [Fact]
    public async Task EnviaAoServicoSoOQuePodeSerOferecido()
    {
        var comanda = await AbrirComanda();
        var pedido = await factory.CriarItemCardapio(flags: FlagDietetica.Vegano);
        await LancarItem(comanda, pedido);
        await RegistrarRestricao(comanda, "Vegano");

        var vegano = await factory.CriarItemCardapio(flags: FlagDietetica.Vegano);
        var comCarne = await factory.CriarItemCardapio();
        var veganoEsgotado = await factory.CriarItemCardapio(disponivel: false, flags: FlagDietetica.Vegano);

        await Sugestoes(comanda);

        var chamada = Assert.Single(factory.ServicoAnalitico.Chamadas);
        Assert.Equal([pedido], chamada.ItensPedidos);
        Assert.Contains(vegano, chamada.ItensPermitidos);
        Assert.DoesNotContain(pedido, chamada.ItensPermitidos);          // já está na mesa
        Assert.DoesNotContain(comCarne, chamada.ItensPermitidos);        // conflita com a restrição
        Assert.DoesNotContain(veganoEsgotado, chamada.ItensPermitidos);  // indisponível hoje (RF21)
        Assert.Equal(3, chamada.Limite);
    }

    [Fact]
    public async Task DescartaIdQueOServicoNaoPodiaSugerir()
    {
        var comanda = await AbrirComanda();
        var pedido = await factory.CriarItemCardapio();
        await LancarItem(comanda, pedido);
        var permitido = await factory.CriarItemCardapio();
        var esgotado = await factory.CriarItemCardapio(disponivel: false);
        factory.ServicoAnalitico.Responder = _ => [esgotado, pedido, permitido, 999_999];

        var sugestoes = await Sugestoes(comanda);

        Assert.Equal([permitido], sugestoes.Itens.Select(i => i.ItemDoCardapioId));
    }

    [Fact]
    public async Task ServicoFora_Responde200ComListaVazia_ESemTravarAComanda()
    {
        var comanda = await AbrirComanda();
        await LancarItem(comanda, await factory.CriarItemCardapio());
        factory.ServicoAnalitico.Indisponivel = true;

        var sugestoes = await Sugestoes(comanda);
        var novoItem = await _cliente.PostAsJsonAsync($"{Rota}/{comanda}/itens",
            new { itemCardapioId = await factory.CriarItemCardapio(), quantidade = 1 }, Json);

        Assert.False(sugestoes.ServicoDisponivel);
        Assert.Empty(sugestoes.Itens);
        Assert.Equal(HttpStatusCode.Created, novoItem.StatusCode);
    }

    [Fact]
    public async Task SemItemValidoNaComanda_NemChamaOServico()
    {
        var comanda = await AbrirComanda();
        var cancelado = await LancarItem(comanda, await factory.CriarItemCardapio());
        await _cliente.PatchAsJsonAsync($"{Rota}/{comanda}/itens/{cancelado}/situacao",
            new { situacao = "Cancelado", motivoCancelamento = "ErroDeLancamento" }, Json);

        var sugestoes = await Sugestoes(comanda);

        Assert.True(sugestoes.ServicoDisponivel);
        Assert.Empty(sugestoes.Itens);
        Assert.Empty(factory.ServicoAnalitico.Chamadas);
    }

    [Theory]
    [InlineData("Alergia", true)]
    [InlineData("Outro", true)]
    [InlineData("SemGluten", false)]
    public async Task RestricaoQueOCardapioNaoConfere_PedeConfirmacaoComOCliente(string categoria, bool confirmar)
    {
        var comanda = await AbrirComanda();
        await LancarItem(comanda, await factory.CriarItemCardapio());
        await RegistrarRestricao(comanda, categoria);

        Assert.Equal(confirmar, (await Sugestoes(comanda)).ConfirmarRestricaoComCliente);
    }

    [Fact]
    public async Task ComandaFechada_Retorna422()
    {
        var comanda = await AbrirComanda();
        await _cliente.PostAsync($"{Rota}/{comanda}/fechamento", null);

        var resposta = await _cliente.GetAsync($"{Rota}/{comanda}/sugestoes");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task ComandaInexistente_Retorna404()
    {
        var resposta = await _cliente.GetAsync($"{Rota}/999999/sugestoes");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Gerente_NaoRecebeSugestao_Retorna403()
    {
        var comanda = await AbrirComanda();
        var gerente = factory.CreateClient();
        Autenticar(gerente, await factory.TokenGerente(factory.CreateClient()));

        var resposta = await gerente.GetAsync($"{Rota}/{comanda}/sugestoes");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }
}
