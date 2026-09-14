using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Gastra.Api.Tests;

public class HealthCheckTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
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
