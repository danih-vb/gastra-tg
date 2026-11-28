using System.Text;
using System.Text.RegularExpressions;
using Gastra.Api.Tests.Infraestrutura;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;

namespace Gastra.Api.Tests.Seguranca;

/// <summary>
/// Inventário endpoint × caso de uso × requisito × papel (#158), gerado do documento OpenAPI da própria API (o mesmo
/// do <c>/openapi/v1.json</c>) e dos atributos de acesso. O teste falha se o arquivo em <c>docs/arquitetura/</c> ficar
/// diferente do código; para regerar: <c>GASTRA_ATUALIZAR_INVENTARIO=1 dotnet test --filter InventarioDeEndpoints</c>.
/// </summary>
public partial class InventarioDeEndpointsTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>
{
    private const string Arquivo = "docs/arquitetura/GASTRA_Inventario_Endpoints.md";

    /// <summary>
    /// Rastreio que o resumo do endpoint não cita (ou cita só em parte). O <c>GET /api/mesas</c> é também o UC30: a folha
    /// de QR codes é montada no frontend a partir da lista de mesas.
    /// </summary>
    private static readonly Dictionary<string, string> Rastreio = new()
    {
        ["GET /api/alocacoes/{data}/{periodo}"] = "UC15, UC21, RF06",
        ["GET /api/cardapio"] = "UC05, UC12, RF19",
        ["GET /api/cardapio/fotos/{nome}"] = "UC19, RF05",
        ["GET /api/cardapio/{id}"] = "UC19, RF05",
        ["GET /api/comandas"] = "UC10, RF01",
        ["GET /api/comandas/{id}"] = "UC10, RF01",
        ["POST /api/comandas/consulta/{codigoAcesso}/chamado"] = "UC20",
        ["DELETE /api/comandas/{id}/chamado"] = "UC20",
        ["POST /api/comandas/{id}/cortesias"] = "UC26, RF26",
        ["PATCH /api/comandas/{id}/itens/{itemId}/preco"] = "UC26, RF26",
        ["POST /api/comandas/{id}/itens/{itemId}/confirmacao"] = "UC27, RF27",
        ["GET /api/destaques"] = "UC28, RF28",
        ["POST /api/destaques"] = "UC28, RF28",
        ["DELETE /api/destaques/{id}"] = "UC28, RF28",
        ["GET /api/indicadores/cardapio"] = "UC16, RF10",
        ["GET /api/indicadores/garcons"] = "UC16, RF10",
        ["GET /api/indicadores/horarios"] = "UC16, RF10",
        ["GET /api/indicadores/pracas"] = "UC16, RF10",
        ["GET /api/indicadores/medalhas"] = "UC29, RF29",
        ["POST /api/mencoes"] = "UC29, RF29",
        ["DELETE /api/mencoes/{id}"] = "UC29, RF29",
        ["POST /api/mesas"] = "UC24, RF23",
        ["PUT /api/mesas/{id}"] = "UC24, RF23",
        ["GET /api/mesas"] = "UC24, UC30, RF23, RF30",
        ["POST /api/pracas"] = "UC24, RF23",
        ["PUT /api/pracas/{id}"] = "UC24, RF23",
        ["GET /api/promocoes"] = "UC08, UC09, RF22",
        ["GET /api/usuarios/fotos/{nome}"] = "UC04, RF18",
        ["PUT /api/usuarios/{id}/foto"] = "UC04, RF18",
        ["DELETE /api/usuarios/{id}/foto"] = "UC04, RF18",
    };

    [Fact]
    public async Task Inventario_EstaEmDiaComOCodigo_ECobreTodosOsCasosDeUso()
    {
        var linhas = await Linhas();
        var conteudo = Markdown(linhas);
        var caminho = Path.Combine(RaizDoRepositorio(), Arquivo);

        if (Environment.GetEnvironmentVariable("GASTRA_ATUALIZAR_INVENTARIO") == "1")
            await File.WriteAllTextAsync(caminho, conteudo);

        var semRastreio = linhas.Where(l => l.Rastreio.Length == 0).Select(l => $"{l.Metodo} {l.Rota}").ToList();
        Assert.True(semRastreio.Count == 0, "Sem caso de uso nem requisito: " + string.Join(", ", semRastreio));

        var casosCobertos = linhas.SelectMany(l => CasoDeUso().Matches(l.Rastreio).Select(m => m.Value)).ToHashSet();
        var faltando = Enumerable.Range(1, 30).Select(n => $"UC{n:00}").Where(uc => !casosCobertos.Contains(uc)).ToList();
        Assert.True(faltando.Count == 0, "Casos de uso sem endpoint: " + string.Join(", ", faltando));

        var noArquivo = File.Exists(caminho) ? (await File.ReadAllTextAsync(caminho)).ReplaceLineEndings("\n") : "";
        Assert.True(noArquivo == conteudo,
            $"{Arquivo} está diferente da API. Rode com GASTRA_ATUALIZAR_INVENTARIO=1 para regerar.");
    }

    private sealed record Linha(string Metodo, string Rota, string Resumo, string Rastreio, string Quem);

    private async Task<List<Linha>> Linhas()
    {
        var documento = await factory.Services.GetRequiredKeyedService<IOpenApiDocumentProvider>("v1")
            .GetOpenApiDocumentAsync();
        var linhas = new List<Linha>();

        foreach (var (caminho, item) in documento.Paths!)
        {
            foreach (var (metodo, operacao) in item.Operations!)
            {
                var verbo = metodo.Method.ToUpperInvariant();
                var rotaDaApi = MapaDeEndpoints.Todos.SingleOrDefault(r =>
                    r.Metodo == verbo && "/" + SemRestricao().Replace(r.Rota, "{$1}") == caminho);
                if (rotaDaApi is null)
                    continue; // o /health não é controller e não está no mapa

                var resumo = Limpar(operacao.Summary ?? operacao.Description ?? "");
                var chave = $"{verbo} {caminho}";
                var rastreio = string.Join(", ", Rastreavel().Matches(resumo + " " + Rastreio.GetValueOrDefault(chave, ""))
                    .Select(m => m.Value).Distinct().Order());
                var quem = rotaDaApi.Publico ? "público"
                    : rotaDaApi.Papeis is null ? "qualquer usuário logado"
                    : string.Join(", ", MapaDeEndpoints.TodosOsPapeis.Where(rotaDaApi.Papeis.Contains)
                        .Select(p => p == "Garcom" ? "Garçom" : p));
                linhas.Add(new Linha(verbo, caminho, resumo, rastreio, quem));
            }
        }

        return linhas.OrderBy(l => l.Rota, StringComparer.Ordinal).ThenBy(l => l.Metodo, StringComparer.Ordinal).ToList();
    }

    private static string Markdown(List<Linha> linhas)
    {
        var texto = new StringBuilder();
        texto.Append("# Inventário de endpoints — GASTRA\n\n");
        texto.Append("Gerado pelo teste `InventarioDeEndpointsTests` a partir do documento OpenAPI da API (o mesmo de\n");
        texto.Append("`/openapi/v1.json`) e dos atributos de acesso dos controllers. **Não edite à mão:** o teste falha se este\n");
        texto.Append("arquivo ficar diferente do código. Para regerar:\n\n");
        texto.Append("```bash\nGASTRA_ATUALIZAR_INVENTARIO=1 dotnet test --filter InventarioDeEndpoints\n```\n\n");
        texto.Append($"{linhas.Count} endpoints. A coluna **Quem** é conferida em execução por `MatrizDePapeisTests`: sem token,\n");
        texto.Append("401; papel fora da lista, 403.\n\n");
        texto.Append("| Método | Rota | O que faz | Rastreio | Quem |\n|---|---|---|---|---|\n");
        foreach (var l in linhas)
            texto.Append($"| {l.Metodo} | `{l.Rota}` | {l.Resumo.Replace("|", "\\|")} | {l.Rastreio} | {l.Quem} |\n");
        return texto.ToString();
    }

    private static string Limpar(string texto) => Regex.Replace(texto, @"\s+", " ").Trim();

    private static string RaizDoRepositorio()
    {
        var pasta = new DirectoryInfo(AppContext.BaseDirectory);
        while (pasta is not null && !(Directory.Exists(Path.Combine(pasta.FullName, "docs"))
                                     && Directory.Exists(Path.Combine(pasta.FullName, "backend"))))
            pasta = pasta.Parent;
        return pasta?.FullName ?? throw new InvalidOperationException("Raiz do repositório não encontrada.");
    }

    [GeneratedRegex(@"\{([a-zA-Z]+)(?::[^}]*)?\}")]
    private static partial Regex SemRestricao();

    [GeneratedRegex(@"\b(UC|RF|RNF|RN)\d{2}\b")]
    private static partial Regex Rastreavel();

    [GeneratedRegex(@"\bUC\d{2}\b")]
    private static partial Regex CasoDeUso();
}
