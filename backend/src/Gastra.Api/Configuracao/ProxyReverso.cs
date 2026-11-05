using Microsoft.AspNetCore.HttpOverrides;

namespace Gastra.Api.Configuracao;

/// <summary>
/// Atrás do nginx (D12), a conexão que chega à API é a do nginx, não a do navegador. O IP de verdade vem no
/// cabeçalho X-Forwarded-For, que o nginx completa com o endereço de quem o chamou. A API só acredita nesse
/// cabeçalho quando a conexão vem de uma rede listada em "Proxy:RedesConfiaveis" (por padrão, a rede interna do
/// Docker, onde o nginx está) e lê só a última entrada, a que o próprio nginx escreveu: um IP inventado pelo
/// cliente no começo do cabeçalho é ignorado.
/// Sem isso, a auditoria (IP do login) e o limite por IP (#230) veriam todo mundo com o mesmo endereço.
/// </summary>
public static class ProxyReverso
{
    public static IServiceCollection AddProxyReverso(this IServiceCollection services, IConfiguration configuration)
    {
        var redes = configuration.GetSection("Proxy:RedesConfiaveis").Get<string[]>() ?? [];

        services.Configure<ForwardedHeadersOptions>(opcoes =>
        {
            opcoes.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            opcoes.ForwardLimit = 1;

            // O padrão confia só em 127.0.0.1; as redes da configuração entram no lugar da lista padrão de redes.
            opcoes.KnownIPNetworks.Clear();
            foreach (var rede in redes)
                opcoes.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(rede));
        });

        return services;
    }
}
