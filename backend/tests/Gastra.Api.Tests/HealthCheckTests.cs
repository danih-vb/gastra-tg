using System.Net;
using Gastra.Api.Tests.Infraestrutura;

namespace Gastra.Api.Tests;

public class HealthCheckTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>
{
    [Fact]
    public async Task Health_RespondeOkQuandoAApiSobe()
    {
        var client = factory.CreateClient();

        var resposta = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Healthy", await resposta.Content.ReadAsStringAsync());
    }
}
