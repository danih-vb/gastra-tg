using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Application.UseCases.Promocoes;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using static Gastra.Api.Tests.Infraestrutura.GastraApiFactory;

namespace Gastra.Api.Tests.Controllers;

/// <summary>UC08 e UC09 — promoções, e o efeito delas no cardápio e na comanda (RF22).</summary>
public class PromocaoControllerTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>, IAsyncLifetime
{
    private const string Rota = "/api/promocoes";

    private readonly HttpClient _gerente = factory.CreateClient();
    private static DateOnly Hoje => HojeNoRestaurante.Data();

    public async Task InitializeAsync() => Autenticar(_gerente, await factory.TokenGerente(factory.CreateClient()));

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<PromocaoResponse> Criar(decimal valor, string tipo, params int[] itens)
    {
        var resposta = await _gerente.PostAsJsonAsync(Rota, new
        {
            descricao = "Semana da moqueca",
            tipoDesconto = tipo,
            valorDesconto = valor,
            dataInicio = Hoje.AddDays(-1),
            dataFim = Hoje.AddDays(1),
            itemCardapioIds = itens,
        }, Json);
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<PromocaoResponse>(Json))!;
    }

    [Fact]
    public async Task Criar_DevolvePrecoComDescontoDeCadaItem_ERegistraNaAuditoria()
    {
        var moqueca = await factory.CriarItemCardapio(preco: 90m);
        var suco = await factory.CriarItemCardapio(preco: 12m);

        var promocao = await Criar(10m, "Percentual", moqueca, suco);

        Assert.True(promocao.Ativa);
        Assert.True(promocao.VigenteHoje);
        Assert.Equal([81m, 10.80m], promocao.Itens.OrderBy(i => i.ItemCardapioId).Select(i => i.PrecoComDesconto));
        var registro = Assert.Single(await factory.Auditoria(EventoAuditoria.PromocaoCriada), r => r.IdEntidade == promocao.Id);
        Assert.Equal(nameof(Promocao), registro.Entidade);
        Assert.Contains($"\"itens_vinculados\":[{moqueca},{suco}]", registro.Detalhes);
    }

    [Fact]
    public async Task CardapioDigital_MostraOPrecoPromocional_SoDoItemEmPromocao()
    {
        var comPromocao = await factory.CriarItemCardapio(preco: 50m);
        var semPromocao = await factory.CriarItemCardapio(preco: 50m);
        await Criar(5m, "ValorFixo", comPromocao);

        var cardapio = await factory.CreateClient().GetFromJsonAsync<List<ItemCardapioResponse>>("/api/cardapio/digital", Json);

        Assert.Equal(45m, cardapio!.Single(i => i.Id == comPromocao).PrecoPromocional);
        Assert.Null(cardapio.Single(i => i.Id == semPromocao).PrecoPromocional);
    }

    [Fact]
    public async Task ItemLancadoNaComanda_SaiComOPrecoPromocional()
    {
        var itemId = await factory.CriarItemCardapio(preco: 100m);
        await Criar(20m, "Percentual", itemId);
        var garcom = factory.CreateClient();
        Autenticar(garcom, await factory.TokenGarcom(factory.CreateClient()));
        var comanda = await (await garcom.PostAsJsonAsync("/api/comandas", new { mesaId = await factory.CriarMesa(), quantidadePessoas = 2 }, Json))
            .Content.ReadFromJsonAsync<ComandaResponse>(Json);

        var item = await (await garcom.PostAsJsonAsync($"/api/comandas/{comanda!.Id}/itens", new { itemCardapioId = itemId, quantidade = 2 }, Json))
            .Content.ReadFromJsonAsync<ItemPedidoResponse>(Json);

        Assert.Equal(80m, item!.PrecoUnitarioNoMomento);
        Assert.Equal(160m, item.Valor);
    }

    [Fact]
    public async Task Remover_DesativaSemApagar_EOPrecoVoltaAoNormal()
    {
        var itemId = await factory.CriarItemCardapio(preco: 40m);
        var promocao = await Criar(10m, "Percentual", itemId);

        var remocao = await _gerente.DeleteAsync($"{Rota}/{promocao.Id}");
        var denovo = await _gerente.DeleteAsync($"{Rota}/{promocao.Id}");

        Assert.Equal(HttpStatusCode.NoContent, remocao.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, denovo.StatusCode);
        var item = await factory.CreateClient().GetFromJsonAsync<ItemCardapioResponse>($"/api/cardapio/{itemId}", Json);
        Assert.Null(item!.PrecoPromocional);
        var ativas = await _gerente.GetFromJsonAsync<List<PromocaoResponse>>(Rota, Json);
        var todas = await _gerente.GetFromJsonAsync<List<PromocaoResponse>>($"{Rota}?todas=true", Json);
        Assert.DoesNotContain(ativas!, p => p.Id == promocao.Id);
        Assert.Contains(todas!, p => p.Id == promocao.Id && !p.Ativa);
        Assert.Single(await factory.Auditoria(EventoAuditoria.PromocaoRemovida), r => r.IdEntidade == promocao.Id);
    }

    [Fact]
    public async Task Criar_ComDescontoFixoMaiorQueOPreco_Retorna422()
    {
        var itemId = await factory.CriarItemCardapio(preco: 10m);

        var resposta = await _gerente.PostAsJsonAsync(Rota, new
        {
            descricao = "Grátis?", tipoDesconto = "ValorFixo", valorDesconto = 10m,
            dataInicio = Hoje, dataFim = Hoje, itemCardapioIds = new[] { itemId },
        }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Theory]
    [InlineData("Percentual", 100, 0, 1)]
    [InlineData("Percentual", 10, 2, 1)]
    [InlineData("Percentual", 10, 0, 0)]
    public async Task Criar_ComDadosInvalidos_Retorna400(string tipo, decimal valor, int diasAntesDoInicio, int quantidadeDeItens)
    {
        var itemId = await factory.CriarItemCardapio();

        var resposta = await _gerente.PostAsJsonAsync(Rota, new
        {
            descricao = "Inválida", tipoDesconto = tipo, valorDesconto = valor,
            dataInicio = Hoje, dataFim = Hoje.AddDays(-diasAntesDoInicio),
            itemCardapioIds = Enumerable.Repeat(itemId, quantidadeDeItens).ToArray(),
        }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Criar_ComItemInexistente_Retorna404()
    {
        var resposta = await _gerente.PostAsJsonAsync(Rota, new
        {
            descricao = "Fantasma", tipoDesconto = "Percentual", valorDesconto = 10m,
            dataInicio = Hoje, dataFim = Hoje, itemCardapioIds = new[] { 999_999 },
        }, Json);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Garcom_NaoGerenciaPromocoes()
    {
        var garcom = factory.CreateClient();
        Autenticar(garcom, await factory.TokenGarcom(factory.CreateClient()));

        Assert.Equal(HttpStatusCode.Forbidden, (await garcom.GetAsync(Rota)).StatusCode);
    }
}
