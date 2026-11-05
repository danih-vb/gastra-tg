using System.Net;
using Gastra.Api.Tests.Infraestrutura;

namespace Gastra.Api.Tests;

/// <summary>O frontend em outra origem só consegue chamar a API se a origem estiver liberada.</summary>
public class CorsTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>
{
    private async Task<HttpResponseMessage> Preflight(string origem)
    {
        var requisicao = new HttpRequestMessage(HttpMethod.Options, "/api/autenticacao/login");
        requisicao.Headers.Add("Origin", origem);
        requisicao.Headers.Add("Access-Control-Request-Method", "POST");
        requisicao.Headers.Add("Access-Control-Request-Headers", "content-type");
        return await factory.CreateClient().SendAsync(requisicao);
    }

    [Fact]
    public async Task FrontendDeDesenvolvimento_EstaLiberado()
    {
        var resposta = await Preflight("http://localhost:4200");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal("http://localhost:4200", resposta.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task OrigemDesconhecida_NaoRecebeLiberacao()
    {
        var resposta = await Preflight("https://site-malicioso.example");

        Assert.False(resposta.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
