using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Communication.Enums;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static Gastra.Api.Tests.Infraestrutura.GastraApiFactory;
using FlagDietetica = Gastra.Domain.Enums.FlagDietetica;

namespace Gastra.Api.Tests.Controllers;

public class ComandaControllerTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>, IAsyncLifetime
{
    private const string Rota = "/api/comandas";

    private readonly HttpClient _cliente = factory.CreateClient();

    // O núcleo de comandas é do Garçom (RF01–RF04).
    public async Task InitializeAsync() => Autenticar(_cliente, await factory.TokenGarcom(factory.CreateClient()));

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task<List<string>> LerErros(HttpResponseMessage resposta) =>
        (await resposta.Content.ReadFromJsonAsync<ErroResponse>(Json))!.Erros;

    private async Task<ComandaResponse> AbrirComanda(int pessoas = 2)
    {
        var mesaId = await factory.CriarMesa();
        var resposta = await _cliente.PostAsJsonAsync(Rota, new { mesaId, quantidadePessoas = pessoas }, Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<ComandaResponse>(Json))!;
    }

    private async Task<ItemPedidoResponse> LancarItem(int comandaId, int itemId, int quantidade = 1)
    {
        var resposta = await _cliente.PostAsJsonAsync($"{Rota}/{comandaId}/itens",
            new { itemCardapioId = itemId, quantidade }, Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<ItemPedidoResponse>(Json))!;
    }

    private async Task<ComandaResponse> Obter(int comandaId) =>
        (await _cliente.GetFromJsonAsync<ComandaResponse>($"{Rota}/{comandaId}", Json))!;

    // --- UC10: abrir comanda ---

    [Fact]
    public async Task Abrir_ComMesaEPessoas_Retorna201ComComposicaoSugeridaECodigoDeAcesso()
    {
        var mesaId = await factory.CriarMesa();

        var resposta = await _cliente.PostAsJsonAsync(Rota, new { mesaId, quantidadePessoas = 4 }, Json);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var comanda = await resposta.Content.ReadFromJsonAsync<ComandaResponse>(Json);
        Assert.Equal(ComposicaoMesa.GrupoPequeno, comanda!.Composicao);
        Assert.Equal(StatusComanda.Aberta, comanda.Status);
        Assert.False(string.IsNullOrWhiteSpace(comanda.CodigoAcessoCliente));
    }

    [Fact]
    public async Task Abrir_ComMesaInexistente_Retorna404()
    {
        var resposta = await _cliente.PostAsJsonAsync(Rota, new { mesaId = 999999, quantidadePessoas = 2 }, Json);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(["Mesa não encontrada."], await LerErros(resposta));
    }

    [Fact]
    public async Task Abrir_SemPessoas_Retorna400()
    {
        var mesaId = await factory.CriarMesa();

        var resposta = await _cliente.PostAsJsonAsync(Rota, new { mesaId, quantidadePessoas = 0 }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    // --- UC11 e RN01: composição ---

    [Fact]
    public async Task LancarItemInfantil_EmMesaDeTres_MudaAComposicaoParaFamilia()
    {
        var comanda = await AbrirComanda(pessoas: 3);
        var itemInfantil = await factory.CriarItemCardapio(flags: FlagDietetica.OpcaoInfantil);

        await LancarItem(comanda.Id, itemInfantil);

        Assert.Equal(ComposicaoMesa.Familia, (await Obter(comanda.Id)).Composicao);
    }

    [Fact]
    public async Task AjustarComposicaoNaMao_ImpedeQueOSistemaReclassifique()
    {
        var comanda = await AbrirComanda(pessoas: 4);
        var itemInfantil = await factory.CriarItemCardapio(flags: FlagDietetica.OpcaoInfantil);

        var ajuste = await _cliente.PatchAsJsonAsync($"{Rota}/{comanda.Id}/composicao",
            new { quantidadePessoas = 4, composicao = "GrupoGrande" }, Json);
        await LancarItem(comanda.Id, itemInfantil);

        Assert.Equal(HttpStatusCode.NoContent, ajuste.StatusCode);
        var atual = await Obter(comanda.Id);
        Assert.Equal(ComposicaoMesa.GrupoGrande, atual.Composicao);
        Assert.True(atual.ComposicaoAjustadaManualmente);
    }

    // --- UC12: itens ---

    [Fact]
    public async Task LancarItem_CopiaOPrecoDoCardapioESomaNoSubtotal()
    {
        var comanda = await AbrirComanda();
        var itemId = await factory.CriarItemCardapio(preco: 30m);

        var item = await LancarItem(comanda.Id, itemId, quantidade: 2);

        Assert.Equal(30m, item.PrecoUnitarioNoMomento);
        Assert.Equal(60m, item.Valor);
        Assert.Equal(StatusItemPedido.Pendente, item.Status);
        Assert.Equal(60m, (await Obter(comanda.Id)).Subtotal);
    }

    [Fact]
    public async Task LancarItem_Indisponivel_Retorna422()
    {
        var comanda = await AbrirComanda();
        var itemId = await factory.CriarItemCardapio(disponivel: false);

        var resposta = await _cliente.PostAsJsonAsync($"{Rota}/{comanda.Id}/itens",
            new { itemCardapioId = itemId, quantidade = 1 }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal(["O item está indisponível no cardápio."], await LerErros(resposta));
    }

    // --- UC23 e RN02: situação do item ---

    [Fact]
    public async Task CancelarItem_SemMotivo_Retorna400()
    {
        var comanda = await AbrirComanda();
        var item = await LancarItem(comanda.Id, await factory.CriarItemCardapio());

        var resposta = await _cliente.PatchAsJsonAsync($"{Rota}/{comanda.Id}/itens/{item.Id}/situacao",
            new { situacao = "Cancelado" }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(["O cancelamento exige um motivo."], await LerErros(resposta));
    }

    [Fact]
    public async Task CancelarItem_ComMotivo_TiraOItemDaConta()
    {
        var comanda = await AbrirComanda();
        var item = await LancarItem(comanda.Id, await factory.CriarItemCardapio(preco: 40m));

        var resposta = await _cliente.PatchAsJsonAsync($"{Rota}/{comanda.Id}/itens/{item.Id}/situacao",
            new { situacao = "Cancelado", motivoCancelamento = "ItemEmFalta" }, Json);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(0m, (await Obter(comanda.Id)).Subtotal);
    }

    // --- UC14: fechamento ---

    [Fact]
    public async Task Fechar_ComItemPendente_Retorna422()
    {
        var comanda = await AbrirComanda();
        await LancarItem(comanda.Id, await factory.CriarItemCardapio());

        var resposta = await _cliente.PostAsync($"{Rota}/{comanda.Id}/fechamento", null);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal(
            ["Há itens pendentes: entregue ou cancele cada um antes de fechar."],
            await LerErros(resposta));
    }

    [Fact]
    public async Task Fechar_ComItemEntregue_CalculaTotalComTaxaDeDezPorCento()
    {
        var comanda = await AbrirComanda();
        var item = await LancarItem(comanda.Id, await factory.CriarItemCardapio(preco: 89.90m));
        await _cliente.PatchAsJsonAsync($"{Rota}/{comanda.Id}/itens/{item.Id}/situacao",
            new { situacao = "Entregue" }, Json);

        var resposta = await _cliente.PostAsync($"{Rota}/{comanda.Id}/fechamento", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var fechada = await resposta.Content.ReadFromJsonAsync<ComandaResponse>(Json);
        Assert.Equal(StatusComanda.Fechada, fechada!.Status);
        Assert.Equal(89.90m, fechada.Subtotal);
        Assert.Equal(8.99m, fechada.TaxaServico);
        Assert.Equal(98.89m, fechada.Total);
        Assert.NotNull(fechada.DataHoraFechamento);
    }

    [Fact]
    public async Task RemoverTaxaServico_ZeraATaxaNoFechamento()
    {
        var comanda = await AbrirComanda();
        var item = await LancarItem(comanda.Id, await factory.CriarItemCardapio(preco: 100m));
        await _cliente.PatchAsJsonAsync($"{Rota}/{comanda.Id}/itens/{item.Id}/situacao",
            new { situacao = "Entregue" }, Json);

        var remocao = await _cliente.DeleteAsync($"{Rota}/{comanda.Id}/taxa-servico");
        var fechamento = await _cliente.PostAsync($"{Rota}/{comanda.Id}/fechamento", null);

        Assert.Equal(HttpStatusCode.NoContent, remocao.StatusCode);
        var fechada = await fechamento.Content.ReadFromJsonAsync<ComandaResponse>(Json);
        Assert.Equal(0m, fechada!.TaxaServico);
        Assert.Equal(100m, fechada.Total);
    }

    [Fact]
    public async Task ComandaFechada_NaoAceitaNovoItem()
    {
        var comanda = await AbrirComanda();
        await _cliente.PostAsync($"{Rota}/{comanda.Id}/fechamento", null);

        var resposta = await _cliente.PostAsJsonAsync($"{Rota}/{comanda.Id}/itens",
            new { itemCardapioId = await factory.CriarItemCardapio(), quantidade = 1 }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal(["A comanda não está aberta."], await LerErros(resposta));
    }

    // --- UC13 e LGPD: restrição alimentar ---

    [Fact]
    public async Task Fechar_ApagaAObservacaoLivreDaRestricaoEMantemACategoria()
    {
        var comanda = await AbrirComanda();
        await _cliente.PostAsJsonAsync($"{Rota}/{comanda.Id}/restricoes",
            new { categoria = "Alergia", observacaoLivre = "alergia a camarão" }, Json);

        await _cliente.PostAsync($"{Rota}/{comanda.Id}/fechamento", null);

        // Depois do fechamento a API não mostra mais a restrição (RN04): a conferência é direto no banco.
        using var escopo = factory.Services.CreateScope();
        var guardada = await escopo.ServiceProvider.GetRequiredService<GastraDbContext>()
            .Set<RestricaoAlimentar>().AsNoTracking().SingleAsync(r => r.ComandaId == comanda.Id);
        Assert.Equal(Gastra.Domain.Enums.CategoriaRestricao.Alergia, guardada.Categoria);
        Assert.Null(guardada.ObservacaoLivre);
    }

    // --- RN04: quem vê a restrição alimentar ---

    private async Task<ComandaResponse> ComandaComRestricao()
    {
        var comanda = await AbrirComanda();
        await _cliente.PostAsJsonAsync($"{Rota}/{comanda.Id}/restricoes",
            new { categoria = "SemGluten", observacaoLivre = "doença celíaca" }, Json);
        return comanda;
    }

    [Fact]
    public async Task Restricao_GarcomVeComAComandaAberta()
    {
        var comanda = await ComandaComRestricao();

        var restricao = Assert.Single((await Obter(comanda.Id)).Restricoes);
        Assert.Equal("doença celíaca", restricao.ObservacaoLivre);
    }

    [Fact]
    public async Task Restricao_MetreVeComAComandaAberta()
    {
        var comanda = await ComandaComRestricao();
        var metre = factory.CreateClient();
        var email = $"metre-{Guid.NewGuid():N}@gastra.test";
        await factory.CriarUsuario(email, Gastra.Domain.Enums.PapelUsuario.Metre);
        Autenticar(metre, await factory.Login(factory.CreateClient(), email));

        var resposta = await metre.GetFromJsonAsync<ComandaResponse>($"{Rota}/{comanda.Id}", Json);

        Assert.Single(resposta!.Restricoes);
    }

    [Fact]
    public async Task Restricao_GerenteNaoVe_NemNaConsultaNemNoPainel()
    {
        var comanda = await ComandaComRestricao();
        var gerente = factory.CreateClient();
        Autenticar(gerente, await factory.TokenGerente(factory.CreateClient()));

        var consulta = await gerente.GetFromJsonAsync<ComandaResponse>($"{Rota}/{comanda.Id}", Json);
        var painel = await gerente.GetFromJsonAsync<List<ComandaResponse>>(Rota, Json);
        var json = await gerente.GetStringAsync($"{Rota}/{comanda.Id}");

        Assert.Empty(consulta!.Restricoes);
        Assert.Empty(painel!.Single(c => c.Id == comanda.Id).Restricoes);
        Assert.DoesNotContain("celíaca", json);
    }

    [Fact]
    public async Task Restricao_DepoisDoFechamento_NemOGarcomVe()
    {
        var comanda = await ComandaComRestricao();

        var fechamento = await _cliente.PostAsync($"{Rota}/{comanda.Id}/fechamento", null);

        Assert.Empty((await fechamento.Content.ReadFromJsonAsync<ComandaResponse>(Json))!.Restricoes);
        Assert.Empty((await Obter(comanda.Id)).Restricoes);
    }

    // --- UC20: consulta do cliente ---

    [Fact]
    public async Task ConsultaDoCliente_PeloCodigoDeAcesso_FuncionaSemLoginENaoExpoeRestricao()
    {
        var comanda = await AbrirComanda();
        await LancarItem(comanda.Id, await factory.CriarItemCardapio(preco: 25m), quantidade: 2);
        await _cliente.PostAsJsonAsync($"{Rota}/{comanda.Id}/restricoes",
            new { categoria = "Alergia", observacaoLivre = "alergia a camarão" }, Json);

        var semLogin = factory.CreateClient();
        var resposta = await semLogin.GetAsync($"{Rota}/consulta/{comanda.CodigoAcessoCliente}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var json = await resposta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("camarão", json);
        var conta = await resposta.Content.ReadFromJsonAsync<ComandaClienteResponse>(Json);
        Assert.Equal(50m, conta!.Subtotal);
        Assert.Equal(55m, conta.Total);
        Assert.Single(conta.Itens);
    }

    [Fact]
    public async Task ConsultaDoCliente_ComCodigoInexistente_Retorna404()
    {
        var resposta = await factory.CreateClient().GetAsync($"{Rota}/consulta/codigo-que-nao-existe");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    // --- Permissões (RNF04) ---

    [Fact]
    public async Task AbrirComanda_ComoGerente_Retorna403_MasLeituraEPermitida()
    {
        var comanda = await AbrirComanda();
        var gerente = factory.CreateClient();
        Autenticar(gerente, await factory.TokenGerente(factory.CreateClient()));

        var abertura = await gerente.PostAsJsonAsync(Rota,
            new { mesaId = await factory.CriarMesa(), quantidadePessoas = 2 }, Json);
        var leitura = await gerente.GetAsync($"{Rota}/{comanda.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, abertura.StatusCode);
        Assert.Equal(HttpStatusCode.OK, leitura.StatusCode);
    }

    [Fact]
    public async Task Comandas_SemLogin_Retorna401()
    {
        var resposta = await factory.CreateClient().GetAsync(Rota);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    // #141: o salão precisa saber quem atende cada mesa.
    [Fact]
    public async Task Comanda_TrazOGarcomQueAbriu_NaAberturaNaConsultaENaLista()
    {
        var aberta = await AbrirComanda();

        Assert.NotEqual(0, aberta.GarcomId);
        Assert.NotEmpty(aberta.GarcomNome);

        var consultada = await Obter(aberta.Id);
        Assert.Equal((aberta.GarcomId, aberta.GarcomNome), (consultada.GarcomId, consultada.GarcomNome));

        var abertas = await _cliente.GetFromJsonAsync<List<ComandaResponse>>(Rota, Json);
        var naLista = abertas!.Single(c => c.Id == aberta.Id);
        Assert.Equal((aberta.GarcomId, aberta.GarcomNome), (naLista.GarcomId, naLista.GarcomNome));
    }
}
