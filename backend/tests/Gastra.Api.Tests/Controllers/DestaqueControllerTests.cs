using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Application.UseCases.Promocoes;
using Gastra.Communication.Enums;
using Gastra.Communication.Responses;
using static Gastra.Api.Tests.Infraestrutura.GastraApiFactory;

namespace Gastra.Api.Tests.Controllers;

/// <summary>Destaques do dia e metas do mês: a gestão cria, o garçom lê e vê quanto a equipe já vendeu.</summary>
public class DestaqueControllerTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>, IAsyncLifetime
{
    private const string Rota = "/api/destaques";

    private readonly HttpClient _gerente = factory.CreateClient();
    private readonly HttpClient _garcom = factory.CreateClient();
    private static DateOnly Hoje => HojeNoRestaurante.Data();

    public async Task InitializeAsync()
    {
        Autenticar(_gerente, await factory.TokenGerente(factory.CreateClient()));
        Autenticar(_garcom, await factory.TokenGarcom(factory.CreateClient()));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<HttpResponseMessage> Criar(int itemId, string tipo, int? meta = null, DateOnly? data = null) =>
        await _gerente.PostAsJsonAsync(Rota, new { itemCardapioId = itemId, tipo, meta, data }, Json);

    private async Task<DestaqueResponse?> DoItem(HttpClient cliente, int itemId) =>
        (await cliente.GetFromJsonAsync<List<DestaqueResponse>>(Rota, Json))!.SingleOrDefault(d => d.ItemCardapioId == itemId);

    [Fact]
    public async Task MetaDoMes_ContaAsUnidadesVendidas_SemCortesia()
    {
        var itemId = await factory.CriarItemCardapio(preco: 22m);
        Assert.Equal(HttpStatusCode.Created, (await Criar(itemId, "MetaDoMes", meta: 40)).StatusCode);

        var mesaId = await factory.CriarMesa();
        var abrir = await _garcom.PostAsJsonAsync("/api/comandas", new { mesaId, quantidadePessoas = 2 }, Json);
        var comanda = (await abrir.Content.ReadFromJsonAsync<ComandaResponse>(Json))!;
        await _garcom.PostAsJsonAsync($"/api/comandas/{comanda.Id}/itens", new { itemCardapioId = itemId, quantidade = 3 }, Json);
        await _gerente.PostAsJsonAsync($"/api/comandas/{comanda.Id}/cortesias",
            new { itemCardapioId = itemId, quantidade = 1, motivo = "Aniversário" }, Json);

        var meta = await DoItem(_garcom, itemId);

        Assert.NotNull(meta);
        Assert.Equal(TipoDestaque.MetaDoMes, meta.Tipo);
        Assert.Equal(40, meta.Meta);
        Assert.Equal(3, meta.Vendidos);
        Assert.True(meta.VigenteHoje);
        Assert.Equal(new DateOnly(Hoje.Year, Hoje.Month, 1), meta.DataInicio);
    }

    [Fact]
    public async Task DestaqueDoDia_PeloGerente_ApareceParaOGarcom()
    {
        var itemId = await factory.CriarItemCardapio();

        var resposta = await Criar(itemId, "DestaqueDoDia");

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var destaque = await DoItem(_garcom, itemId);
        Assert.Equal(Hoje, destaque!.DataInicio);
        Assert.Null(destaque.Meta);
    }

    [Fact]
    public async Task Criar_PeloGarcom_Retorna403()
    {
        var itemId = await factory.CriarItemCardapio();

        var resposta = await _garcom.PostAsJsonAsync(Rota, new { itemCardapioId = itemId, tipo = "DestaqueDoDia" }, Json);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task MetaSemQuantidade_Retorna400_EDiaPassado_Retorna400()
    {
        var itemId = await factory.CriarItemCardapio();

        Assert.Equal(HttpStatusCode.BadRequest, (await Criar(itemId, "MetaDoMes")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Criar(itemId, "DestaqueDoDia", data: Hoje.AddDays(-1))).StatusCode);
    }

    [Fact]
    public async Task MesmoDestaqueDuasVezes_Retorna422()
    {
        var itemId = await factory.CriarItemCardapio();
        await Criar(itemId, "DestaqueDoDia");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await Criar(itemId, "DestaqueDoDia")).StatusCode);
    }

    [Fact]
    public async Task Remover_TiraDaLista()
    {
        var itemId = await factory.CriarItemCardapio();
        var criado = (await (await Criar(itemId, "DestaqueDoDia")).Content.ReadFromJsonAsync<DestaqueResponse>(Json))!;

        var resposta = await _gerente.DeleteAsync($"{Rota}/{criado.Id}");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Null(await DoItem(_gerente, itemId));
    }
}
