using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Communication.Responses;
using static Gastra.Api.Tests.Infraestrutura.GastraApiFactory;

namespace Gastra.Api.Tests.Controllers;

public class SalaoControllerTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _cliente = factory.CreateClient();

    // UC24 — o cadastro do salão é do Gerente (RF23).
    public async Task InitializeAsync() => Autenticar(_cliente, await factory.TokenGerente(factory.CreateClient()));

    public Task DisposeAsync() => Task.CompletedTask;

    private static string Codigo(string prefixo) => $"{prefixo}-{Guid.NewGuid():N}"[..12];

    // O número da mesa tem limite de 10 caracteres (RF23).
    private static string NumeroDeMesa() => $"M{Guid.NewGuid():N}"[..8];

    private static async Task<List<string>> LerErros(HttpResponseMessage resposta) =>
        (await resposta.Content.ReadFromJsonAsync<ErroResponse>(Json))!.Erros;

    private async Task<PracaResponse> CadastrarPraca(int garcons = 2)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/pracas",
            new { codigo = Codigo("PR"), quantidadeGarcons = garcons }, Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<PracaResponse>(Json))!;
    }

    private async Task<MesaResponse> CadastrarMesa(int pracaId, int capacidade = 4)
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/mesas",
            new { numero = NumeroDeMesa(), capacidade, pracaId }, Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<MesaResponse>(Json))!;
    }

    // --- praças ---

    [Fact]
    public async Task CadastrarPraca_ComDadosValidos_Retorna201()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/pracas",
            new { codigo = Codigo("PR"), quantidadeGarcons = 3 }, Json);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var praca = await resposta.Content.ReadFromJsonAsync<PracaResponse>(Json);
        Assert.True(praca!.Id > 0);
        Assert.Equal(3, praca.QuantidadeGarcons);
    }

    [Fact]
    public async Task CadastrarPraca_SemGarcons_Retorna400()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/pracas",
            new { codigo = Codigo("PR"), quantidadeGarcons = 0 }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(["A praça precisa de pelo menos um garçom."], await LerErros(resposta));
    }

    [Fact]
    public async Task CadastrarPraca_ComCodigoRepetido_Retorna422()
    {
        var praca = await CadastrarPraca();

        var resposta = await _cliente.PostAsJsonAsync("/api/pracas",
            new { codigo = praca.Codigo, quantidadeGarcons = 2 }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task EditarPraca_AlteraAQuantidadeDeGarcons()
    {
        var praca = await CadastrarPraca(garcons: 2);

        var resposta = await _cliente.PutAsJsonAsync($"/api/pracas/{praca.Id}",
            new { codigo = praca.Codigo, quantidadeGarcons = 5 }, Json);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var pracas = await _cliente.GetFromJsonAsync<List<PracaResponse>>("/api/pracas", Json);
        Assert.Equal(5, pracas!.Single(p => p.Id == praca.Id).QuantidadeGarcons);
    }

    [Fact]
    public async Task EditarPraca_Inexistente_Retorna404()
    {
        var resposta = await _cliente.PutAsJsonAsync("/api/pracas/999999",
            new { codigo = Codigo("PR"), quantidadeGarcons = 2 }, Json);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(["Praça não encontrada."], await LerErros(resposta));
    }

    // --- mesas ---

    [Fact]
    public async Task CadastrarMesa_VinculaAPracaEApareceNaListagem()
    {
        var praca = await CadastrarPraca();

        var mesa = await CadastrarMesa(praca.Id, capacidade: 6);

        Assert.Equal(praca.Id, mesa.PracaId);
        Assert.Equal(praca.Codigo, mesa.PracaCodigo);
        var mesas = await _cliente.GetFromJsonAsync<List<MesaResponse>>("/api/mesas", Json);
        Assert.Contains(mesas!, m => m.Id == mesa.Id && m.Capacidade == 6);
    }

    [Fact]
    public async Task CadastrarMesa_EmPracaInexistente_Retorna404()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/mesas",
            new { numero = NumeroDeMesa(), capacidade = 4, pracaId = 999999 }, Json);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(["Praça não encontrada."], await LerErros(resposta));
    }

    [Fact]
    public async Task CadastrarMesa_ComNumeroRepetido_Retorna422()
    {
        var praca = await CadastrarPraca();
        var mesa = await CadastrarMesa(praca.Id);

        var resposta = await _cliente.PostAsJsonAsync("/api/mesas",
            new { numero = mesa.Numero, capacidade = 2, pracaId = praca.Id }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal(["Já existe uma mesa com este número."], await LerErros(resposta));
    }

    [Fact]
    public async Task CadastrarMesa_SemCapacidade_Retorna400()
    {
        var praca = await CadastrarPraca();

        var resposta = await _cliente.PostAsJsonAsync("/api/mesas",
            new { numero = NumeroDeMesa(), capacidade = 0, pracaId = praca.Id }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task EditarMesa_AlteraNumeroECapacidadeSemTrocarDePraca()
    {
        var praca = await CadastrarPraca();
        var mesa = await CadastrarMesa(praca.Id);
        var novoNumero = NumeroDeMesa();

        var resposta = await _cliente.PutAsJsonAsync($"/api/mesas/{mesa.Id}",
            new { numero = novoNumero, capacidade = 8 }, Json);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var atualizada = (await _cliente.GetFromJsonAsync<List<MesaResponse>>("/api/mesas", Json))!
            .Single(m => m.Id == mesa.Id);
        Assert.Equal(novoNumero, atualizada.Numero);
        Assert.Equal(8, atualizada.Capacidade);
        Assert.Equal(praca.Id, atualizada.PracaId);
    }

    // --- permissões (RNF04) ---

    [Fact]
    public async Task Garcom_NaoCadastraMesa_MasPrecisaEnxergarALista()
    {
        var praca = await CadastrarPraca();
        var garcom = factory.CreateClient();
        Autenticar(garcom, await factory.TokenGarcom(factory.CreateClient()));

        var cadastro = await garcom.PostAsJsonAsync("/api/mesas",
            new { numero = NumeroDeMesa(), capacidade = 4, pracaId = praca.Id }, Json);
        var listagem = await garcom.GetAsync("/api/mesas");

        Assert.Equal(HttpStatusCode.Forbidden, cadastro.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listagem.StatusCode);
    }

    [Fact]
    public async Task Salao_SemLogin_Retorna401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/mesas")).StatusCode);
    }

    // --- fluxo completo: o salão cadastrado serve para abrir comanda ---

    [Fact]
    public async Task MesaCadastradaPeloGerente_PermiteAoGarcomAbrirComanda()
    {
        var praca = await CadastrarPraca();
        var mesa = await CadastrarMesa(praca.Id);

        var garcom = factory.CreateClient();
        Autenticar(garcom, await factory.TokenGarcom(factory.CreateClient()));
        var resposta = await garcom.PostAsJsonAsync("/api/comandas",
            new { mesaId = mesa.Id, quantidadePessoas = 2 }, Json);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }
}
