using System.Net.Http.Json;
using System.Text.Json;
using Gastra.Domain.Servicos;
using Microsoft.Extensions.Logging;

namespace Gastra.Infrastructure.ServicoAnalitico;

/// <summary>
/// Chama a API FastAPI da camada analítica. Qualquer falha (fora do ar, tempo esgotado, status de erro,
/// JSON inesperado) vira <see cref="ServicoAnaliticoIndisponivelException"/>, para o caso de uso decidir
/// como seguir sem a sugestão.
/// </summary>
public class ServicoAnaliticoHttp(HttpClient http, ILogger<ServicoAnaliticoHttp> logger) : IServicoAnalitico
{
    // O Python usa snake_case nos contratos (data-science/src/gastra_analitica/api/modelos.py).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public async Task<IReadOnlyList<int>> SugerirCombinacoes(
        IReadOnlyCollection<int> itensPedidos,
        IReadOnlyCollection<int> itensPermitidos,
        int limite,
        CancellationToken cancellationToken = default)
    {
        var pedido = new PedidoDeRecomendacao(itensPedidos, itensPermitidos, limite);

        var resposta = await Enviar<RespostaDeRecomendacao>("recomendacao/combinacoes", pedido, cancellationToken);

        return resposta.Sugestoes.Select(s => s.ItemId).ToList();
    }

    private async Task<TResposta> Enviar<TResposta>(string rota, object corpo, CancellationToken cancellationToken)
    {
        try
        {
            using var resposta = await http.PostAsJsonAsync(rota, corpo, Json, cancellationToken);
            resposta.EnsureSuccessStatusCode();

            return await resposta.Content.ReadFromJsonAsync<TResposta>(Json, cancellationToken)
                   ?? throw new JsonException("Resposta vazia.");
        }
        catch (Exception erro) when (Falha(erro, cancellationToken))
        {
            // Sem o corpo da requisição no log: só ids, mas o log técnico não precisa deles.
            logger.LogWarning(erro, "Serviço analítico indisponível em {Rota}", rota);
            throw new ServicoAnaliticoIndisponivelException(erro.GetType().Name, erro);
        }
    }

    // Cancelamento pedido por quem chamou (ex.: o garçom saiu da tela) não é falha do serviço.
    private static bool Falha(Exception erro, CancellationToken cancellationToken) => erro switch
    {
        OperationCanceledException => !cancellationToken.IsCancellationRequested,
        HttpRequestException or JsonException or NotSupportedException => true,
        _ => false,
    };

    private record PedidoDeRecomendacao(
        IReadOnlyCollection<int> Itens,
        IReadOnlyCollection<int> ItensDisponiveis,
        int Limite);

    private record RespostaDeRecomendacao(List<ItemSugerido> Sugestoes);

    private record ItemSugerido(int ItemId);
}
