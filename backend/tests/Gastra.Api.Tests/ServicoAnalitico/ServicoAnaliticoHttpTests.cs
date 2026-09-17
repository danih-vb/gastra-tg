using System.Net;
using System.Text;
using System.Text.Json;
using Gastra.Domain.Servicos;
using Gastra.Infrastructure.ServicoAnalitico;
using Microsoft.Extensions.Logging.Abstractions;

namespace Gastra.Api.Tests.ServicoAnalitico;

/// <summary>
/// Cliente HTTP da camada analítica, com o Python substituído por um handler que responde o que o teste quer.
/// </summary>
public class ServicoAnaliticoHttpTests
{
    private sealed class HandlerFalso(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
        : HttpMessageHandler
    {
        public string? CorpoRecebido { get; private set; }
        public Uri? UrlRecebida { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            UrlRecebida = request.RequestUri;
            CorpoRecebido = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return await responder(request, cancellationToken);
        }
    }

    private static (ServicoAnaliticoHttp Servico, HandlerFalso Handler) Criar(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder, int tempoLimiteMs = 2000)
    {
        var handler = new HandlerFalso(responder);
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://analitica.test/"),
            Timeout = TimeSpan.FromMilliseconds(tempoLimiteMs),
        };
        return (new ServicoAnaliticoHttp(http, NullLogger<ServicoAnaliticoHttp>.Instance), handler);
    }

    private static Task<HttpResponseMessage> Json(string corpo, HttpStatusCode status = HttpStatusCode.OK) =>
        Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(corpo, Encoding.UTF8, "application/json"),
        });

    [Fact]
    public async Task EnviaOContratoEmSnakeCase_ELeOsIdsNaOrdem()
    {
        var (servico, handler) = Criar((_, _) => Json("""
            {"sugestoes": [{"item_id": 7, "confianca": 0.9, "lift": 2.1}, {"item_id": 3, "confianca": 0.5, "lift": 1.4}],
             "regras_consideradas": 12, "origem_do_historico": "simulado"}
            """));

        var ids = await servico.SugerirCombinacoes([1, 2], [3, 7, 9], 3);

        Assert.Equal([7, 3], ids);
        Assert.Equal("http://analitica.test/recomendacao/combinacoes", handler.UrlRecebida!.ToString());
        using var corpo = JsonDocument.Parse(handler.CorpoRecebido!);
        Assert.Equal([1, 2], corpo.RootElement.GetProperty("itens").EnumerateArray().Select(e => e.GetInt32()));
        Assert.Equal([3, 7, 9], corpo.RootElement.GetProperty("itens_disponiveis").EnumerateArray().Select(e => e.GetInt32()));
        Assert.Equal(3, corpo.RootElement.GetProperty("limite").GetInt32());
    }

    [Fact]
    public async Task Alocacao_EnviaGarconsPracasEPesosEmSnakeCase_ELeAsDesignacoes()
    {
        var (servico, handler) = Criar((_, _) => Json("""
            {"designacoes": [{"garcom_id": 5, "praca_id": 2, "custo": 0.1}, {"garcom_id": 6, "praca_id": 1, "custo": 0.3}],
             "custo_total": 0.4, "peso_desequilibrio": 0.6, "peso_espera": 0.4}
            """));

        var designacoes = await servico.SugerirAlocacao(
            [new GarcomParaAlocacao(5, 9000.50m, 3), new GarcomParaAlocacao(6, 1200m, 0)],
            [new PracaParaAlocacao(1, 2, 1800m), new PracaParaAlocacao(2, 1, 600m)],
            0.6, 0.4);

        Assert.Equal([new DesignacaoSugerida(5, 2), new DesignacaoSugerida(6, 1)], designacoes);
        Assert.Equal("http://analitica.test/alocacao/sugestao", handler.UrlRecebida!.ToString());
        using var corpo = JsonDocument.Parse(handler.CorpoRecebido!);
        var garcom = corpo.RootElement.GetProperty("garcons")[0];
        Assert.Equal(5, garcom.GetProperty("id").GetInt32());
        Assert.Equal(9000.50m, garcom.GetProperty("faturamento_por_turno").GetDecimal());
        Assert.Equal(3, garcom.GetProperty("turnos_desde_praca_de_alto_potencial").GetInt32());
        var praca = corpo.RootElement.GetProperty("pracas")[1];
        Assert.Equal((2, 1, 600m), (praca.GetProperty("id").GetInt32(), praca.GetProperty("vagas").GetInt32(), praca.GetProperty("faturamento_medio_historico").GetDecimal()));
        Assert.Equal(0.6, corpo.RootElement.GetProperty("peso_desequilibrio").GetDouble());
    }

    [Fact]
    public async Task StatusDeErro_ViraServicoIndisponivel()
    {
        var (servico, _) = Criar((_, _) => Json("""{"detail": "falhou"}""", HttpStatusCode.InternalServerError));

        await Assert.ThrowsAsync<ServicoAnaliticoIndisponivelException>(() => servico.SugerirCombinacoes([1], [2], 3));
    }

    [Fact]
    public async Task ServicoForaDoAr_ViraServicoIndisponivel()
    {
        var (servico, _) = Criar((_, _) => throw new HttpRequestException("Conexão recusada"));

        await Assert.ThrowsAsync<ServicoAnaliticoIndisponivelException>(() => servico.SugerirCombinacoes([1], [2], 3));
    }

    [Fact]
    public async Task RespostaLenta_EstouraOTempoLimite_EViraServicoIndisponivel()
    {
        var (servico, _) = Criar(async (_, token) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }, tempoLimiteMs: 100);

        var inicio = DateTime.UtcNow;
        await Assert.ThrowsAsync<ServicoAnaliticoIndisponivelException>(() => servico.SugerirCombinacoes([1], [2], 3));

        Assert.True(DateTime.UtcNow - inicio < TimeSpan.FromSeconds(5), "o tempo limite não foi respeitado");
    }

    [Theory]
    [InlineData("<html>gateway</html>")]
    [InlineData("""{"sugestoes": "não é lista"}""")]
    [InlineData("null")]
    public async Task RespostaInvalida_ViraServicoIndisponivel(string corpo)
    {
        var (servico, _) = Criar((_, _) => Json(corpo));

        await Assert.ThrowsAsync<ServicoAnaliticoIndisponivelException>(() => servico.SugerirCombinacoes([1], [2], 3));
    }

    [Fact]
    public async Task CancelamentoPedidoPorQuemChamou_NaoEhTratadoComoFalhaDoServico()
    {
        var (servico, _) = Criar(async (_, token) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var cancelamento = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var erro = await Record.ExceptionAsync(() => servico.SugerirCombinacoes([1], [2], 3, cancelamento.Token));

        Assert.IsAssignableFrom<OperationCanceledException>(erro);
    }
}
