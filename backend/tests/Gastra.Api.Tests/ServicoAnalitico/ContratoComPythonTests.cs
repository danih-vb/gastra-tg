using Gastra.Infrastructure.ServicoAnalitico;
using Microsoft.Extensions.Logging.Abstractions;

namespace Gastra.Api.Tests.ServicoAnalitico;

/// <summary>Só roda com o serviço Python no ar: confere que os dois lados falam o mesmo contrato.</summary>
public sealed class FactComPythonAttribute : FactAttribute
{
    public const string VariavelDeAmbiente = "GASTRA_TESTES_ANALITICA";

    public FactComPythonAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(VariavelDeAmbiente)))
            Skip = $"Suba o serviço Python e defina {VariavelDeAmbiente} (ex.: http://localhost:8000).";
    }
}

public class ContratoComPythonTests
{
    [FactComPython]
    public async Task ServicoReal_SugereOArrozDeCocoParaQuemPediuMoqueca()
    {
        var url = Environment.GetEnvironmentVariable(FactComPythonAttribute.VariavelDeAmbiente)!;
        var http = new HttpClient { BaseAddress = new Uri(url.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(30) };
        var servico = new ServicoAnaliticoHttp(http, NullLogger<ServicoAnaliticoHttp>.Instance);

        // Sem histórico real, o Python usa o simulado, onde a moqueca (1) puxa o arroz de coco (2).
        var ids = await servico.SugerirCombinacoes([1], [2, 3, 4, 5, 6], 3);

        Assert.NotEmpty(ids);
        Assert.Equal(2, ids[0]);
        Assert.All(ids, id => Assert.Contains(id, new[] { 2, 3, 4, 5, 6 }));
    }
}
