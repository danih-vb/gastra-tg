using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Communication.Responses;

namespace Gastra.Api.Tests.Controllers;

public class CardapioControllerTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>
{
    private const string Rota = "/api/cardapio";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _cliente = factory.CreateClient();

    private static object ItemValido(string nome = "Risoto de cogumelos") => new
    {
        nome,
        categoria = "PratoPrincipal",
        preco = 89.90m,
        descricao = "Arroz arbóreo com shimeji",
        flagsDieteticas = new[] { "Vegetariano", "SemGluten" },
    };

    private async Task<ItemCardapioResponse> Cadastrar(string nome)
    {
        var resposta = await _cliente.PostAsJsonAsync(Rota, ItemValido(nome), Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<ItemCardapioResponse>(Json))!;
    }

    private static async Task<List<string>> LerErros(HttpResponseMessage resposta) =>
        (await resposta.Content.ReadFromJsonAsync<ErroResponse>(Json))!.Erros;

    [Fact]
    public async Task Cadastrar_ComDadosValidos_Retorna201ComOItem()
    {
        var resposta = await _cliente.PostAsJsonAsync(Rota, ItemValido(), Json);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(resposta.Headers.Location);

        var item = await resposta.Content.ReadFromJsonAsync<ItemCardapioResponse>(Json);
        Assert.True(item!.Id > 0);
        Assert.True(item.Disponivel);
        Assert.Equal(89.90m, item.Preco);
        Assert.Equal(2, item.FlagsDieteticas.Count);
    }

    [Fact]
    public async Task Cadastrar_ComNomeVazioEPrecoZero_Retorna400ComOsDoisErrosEmPortugues()
    {
        var invalido = new { nome = "", categoria = "Bebida", preco = 0, descricao = "", flagsDieteticas = Array.Empty<string>() };

        var resposta = await _cliente.PostAsJsonAsync(Rota, invalido, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var erros = await LerErros(resposta);
        Assert.Contains("O nome é obrigatório.", erros);
        Assert.Contains("O preço deve ser maior que zero.", erros);
    }

    [Fact]
    public async Task Cadastrar_ComDadosInvalidosEmIngles_RetornaMensagensEmIngles()
    {
        var invalido = new { nome = "", categoria = "Bebida", preco = 10, descricao = "", flagsDieteticas = Array.Empty<string>() };
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, Rota) { Content = JsonContent.Create(invalido, options: Json) };
        requisicao.Headers.AcceptLanguage.ParseAdd("en-US");

        var resposta = await _cliente.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(["Name is required."], await LerErros(resposta));
    }

    [Fact]
    public async Task Cadastrar_ComCategoriaInexistente_Retorna400NoFormatoPadrao()
    {
        var invalido = new { nome = "Pizza", categoria = "Pizza", preco = 50, descricao = "", flagsDieteticas = Array.Empty<string>() };

        var resposta = await _cliente.PostAsJsonAsync(Rota, invalido, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(["Requisição inválida."], await LerErros(resposta));
    }

    [Fact]
    public async Task AtualizarPreco_DeItemExistente_Retorna204EOPrecoMuda()
    {
        var item = await Cadastrar("Pudim");

        var resposta = await _cliente.PatchAsJsonAsync($"{Rota}/{item.Id}/preco", new { preco = 15.50m }, Json);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var atualizado = await _cliente.GetFromJsonAsync<ItemCardapioResponse>($"{Rota}/{item.Id}", Json);
        Assert.Equal(15.50m, atualizado!.Preco);
    }

    [Fact]
    public async Task AtualizarPreco_DeItemInexistente_Retorna404ComMensagem()
    {
        var resposta = await _cliente.PatchAsJsonAsync($"{Rota}/999999/preco", new { preco = 10m }, Json);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(["Item do cardápio não encontrado."], await LerErros(resposta));
    }

    [Fact]
    public async Task MarcarIndisponivel_ItemSaiDoCardapioDigitalMasContinuaNaGestao()
    {
        var item = await Cadastrar("Suco de caju");

        var resposta = await _cliente.PatchAsJsonAsync($"{Rota}/{item.Id}/disponibilidade", new { disponivel = false }, Json);
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);

        var disponiveis = await _cliente.GetFromJsonAsync<List<ItemCardapioResponse>>($"{Rota}?somenteDisponiveis=true", Json);
        var todos = await _cliente.GetFromJsonAsync<List<ItemCardapioResponse>>(Rota, Json);
        Assert.DoesNotContain(disponiveis!, i => i.Id == item.Id);
        Assert.Contains(todos!, i => i.Id == item.Id && !i.Disponivel);
    }
}
