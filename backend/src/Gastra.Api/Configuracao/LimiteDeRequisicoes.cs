using System.Globalization;
using System.Threading.RateLimiting;
using Gastra.Communication.Responses;
using Gastra.Exceptions;

namespace Gastra.Api.Configuracao;

/// <summary>
/// Segunda camada contra tentativa e erro (#230), por IP, nas rotas que não pedem login. A primeira é o bloqueio da
/// conta (RN09), que protege uma conta de muitos palpites; esta protege contra quem testa muitas contas, ou muitos
/// códigos de cliente, a partir do mesmo endereço. Estourou o limite do minuto: 429 com a mensagem traduzida.
/// </summary>
public static class LimiteDeRequisicoes
{
    /// <summary>Login e segundo fator.</summary>
    public const string Autenticacao = "autenticacao";

    /// <summary>Consulta e avaliação pelo código de acesso do cliente (UC20, UC25).</summary>
    public const string ConsultaCliente = "consulta-cliente";

    public static IServiceCollection AddLimiteDeRequisicoes(this IServiceCollection services, IConfiguration configuration)
    {
        var secao = configuration.GetSection("LimiteRequisicoes");
        var habilitado = secao.GetValue("Habilitado", true);

        services.AddRateLimiter(opcoes =>
        {
            opcoes.AddPolicy(Autenticacao, contexto =>
                PorIp(contexto, habilitado, secao.GetValue("AutenticacaoPorMinuto", 10)));

            // A tela do cliente se atualiza a cada 15 segundos (4 por minuto); o limite deixa folga para
            // vários celulares atrás do mesmo roteador.
            opcoes.AddPolicy(ConsultaCliente, contexto =>
                PorIp(contexto, habilitado, secao.GetValue("ConsultaClientePorMinuto", 30)));

            opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opcoes.OnRejected = async (contexto, cancelamento) =>
            {
                if (contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out var espera))
                    contexto.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(espera.TotalSeconds)).ToString(CultureInfo.InvariantCulture);

                await contexto.HttpContext.Response.WriteAsJsonAsync(
                    new ErroResponse(MensagensErro.MuitasRequisicoes), cancelamento);
            };
        });

        return services;
    }

    private static RateLimitPartition<string> PorIp(HttpContext contexto, bool habilitado, int porMinuto)
    {
        var ip = contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

        if (!habilitado)
            return RateLimitPartition.GetNoLimiter(ip);

        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = porMinuto,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });
    }
}
