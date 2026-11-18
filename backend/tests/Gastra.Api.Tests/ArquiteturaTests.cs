using System.Reflection;

namespace Gastra.Api.Tests;

/// <summary>
/// Garante a regra de dependência da arquitetura em camadas:
/// as dependências apontam para dentro e o domínio não depende de nenhuma outra camada.
/// </summary>
public class ArquiteturaTests
{
    private static readonly string[] Camadas =
    [
        "Gastra.Api",
        "Gastra.Application",
        "Gastra.Communication",
        "Gastra.Domain",
        "Gastra.Exceptions",
        "Gastra.Infrastructure",
    ];

    [Theory]
    [InlineData("Gastra.Domain")]
    [InlineData("Gastra.Communication")]
    [InlineData("Gastra.Exceptions")]
    public void CamadaBase_NaoDependeDeOutraCamada(string camada)
    {
        Assert.Empty(CamadasReferenciadasPor(camada));
    }

    [Fact]
    public void Infrastructure_DependeApenasDoDomain()
    {
        Assert.All(CamadasReferenciadasPor("Gastra.Infrastructure"),
            referencia => Assert.Equal("Gastra.Domain", referencia));
    }

    [Fact]
    public void Application_NaoDependeDeInfrastructureNemDaApi()
    {
        var referencias = CamadasReferenciadasPor("Gastra.Application");

        Assert.DoesNotContain("Gastra.Infrastructure", referencias);
        Assert.DoesNotContain("Gastra.Api", referencias);
    }

    private static IEnumerable<string> CamadasReferenciadasPor(string camada)
    {
        return Assembly.Load(camada)
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name!)
            .Where(nome => Camadas.Contains(nome))
            .ToList();
    }
}
