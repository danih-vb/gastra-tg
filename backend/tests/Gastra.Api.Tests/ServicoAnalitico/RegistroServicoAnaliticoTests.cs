using Gastra.Domain.Servicos;
using Gastra.Infrastructure;
using Gastra.Infrastructure.ServicoAnalitico;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gastra.Api.Tests.ServicoAnalitico;

/// <summary>
/// A fábrica dos testes da API troca o serviço por um falso; aqui se confere o registro verdadeiro.
/// </summary>
public class RegistroServicoAnaliticoTests
{
    private static ServiceProvider Montar(Dictionary<string, string?> secaoAnalitica)
    {
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:ChaveAssinatura"] = "chave-exclusiva-dos-testes-de-registro-do-gastra",
            })
            .AddInMemoryCollection(secaoAnalitica)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuracao);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void UsaOClienteHttpComEnderecoETempoLimiteDaConfiguracao()
    {
        using var provedor = Montar(new()
        {
            ["ServicoAnalitico:UrlBase"] = "http://analitica:9000",
            ["ServicoAnalitico:TempoLimiteMilissegundos"] = "1500",
        });

        Assert.IsType<ServicoAnaliticoHttp>(provedor.GetRequiredService<IServicoAnalitico>());

        var http = provedor.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(IServicoAnalitico));
        Assert.Equal(new Uri("http://analitica:9000/"), http.BaseAddress);
        Assert.Equal(TimeSpan.FromMilliseconds(1500), http.Timeout);
    }

    [Theory]
    [InlineData("ServicoAnalitico:UrlBase", "sem-protocolo")]
    [InlineData("ServicoAnalitico:TempoLimiteMilissegundos", "0")]
    public void ConfiguracaoInvalida_ImpedeASubidaDaApi(string chave, string valor)
    {
        Assert.Throws<InvalidOperationException>(() => Montar(new() { [chave] = valor }));
    }
}
