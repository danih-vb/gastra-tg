namespace Gastra.Api.Configuracao;

/// <summary>
/// Libera o frontend Angular para chamar a API a partir de outra origem (porta 4200 no desenvolvimento). Só as
/// origens listadas em "Cors:OrigensPermitidas" são aceitas; sem nenhuma na configuração, nada é liberado.
/// </summary>
public static class PoliticaCors
{
    public const string Nome = "frontend";

    public static IServiceCollection AddPoliticaCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origens = configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];

        services.AddCors(opcoes => opcoes.AddPolicy(Nome, politica => politica
            .WithOrigins(origens)
            .WithHeaders("Authorization", "Content-Type", "Accept-Language")
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")));

        return services;
    }
}
