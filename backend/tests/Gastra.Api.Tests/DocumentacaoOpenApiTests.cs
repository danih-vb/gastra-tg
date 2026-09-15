using System.Net;
using Gastra.Api.Tests.Infraestrutura;

namespace Gastra.Api.Tests;

public class DocumentacaoOpenApiTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>
{
    // Os testes rodam no ambiente "Testes": fora de Development a API não expõe o próprio mapa de endpoints.
    [Theory]
    [InlineData("/swagger/index.html")]
    [InlineData("/openapi/v1.json")]
    public async Task Documentacao_ForaDoAmbienteDeDesenvolvimento_NaoFicaExposta(string rota)
    {
        var resposta = await factory.CreateClient().GetAsync(rota);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }
}
