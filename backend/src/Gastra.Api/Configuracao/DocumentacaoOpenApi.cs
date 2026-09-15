using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Gastra.Api.Configuracao;

/// <summary>
/// Documentação interativa da API. O ASP.NET Core gera a especificação OpenAPI
/// (<c>/openapi/v1.json</c>) e o Swagger UI desenha a tela em <c>/swagger</c>. Só existe em
/// desenvolvimento: em produção a API não expõe o próprio mapa de endpoints.
/// </summary>
public static class DocumentacaoOpenApi
{
    private const string EsquemaJwt = "Bearer";

    public static IServiceCollection AddDocumentacaoOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(opcoes =>
        {
            opcoes.AddDocumentTransformer((documento, _, _) =>
            {
                documento.Info.Title = "GASTRA API";
                documento.Info.Description =
                    "Gestão Analítica de Restaurantes. Para os endpoints protegidos, faça login em " +
                    "/api/autenticacao/login (e confirme o segundo fator, se for Gerente ou Coordenador), " +
                    "clique em Authorize e cole o tokenAcesso.";

                documento.Components ??= new OpenApiComponents();
                documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                documento.Components.SecuritySchemes[EsquemaJwt] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                };
                return Task.CompletedTask;
            });

            // O cadeado aparece só nos endpoints que exigem login: os públicos (login, cardápio
            // digital) continuam abertos no Swagger, como na API.
            opcoes.AddOperationTransformer((operacao, contexto, _) =>
            {
                var metadados = contexto.Description.ActionDescriptor.EndpointMetadata;
                var exigeLogin = metadados.OfType<IAuthorizeData>().Any() && !metadados.OfType<IAllowAnonymous>().Any();

                if (exigeLogin)
                {
                    operacao.Security ??= [];
                    operacao.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(EsquemaJwt, contexto.Document)] = [],
                    });
                }
                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication UseDocumentacaoOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.MapOpenApi();
        app.UseSwaggerUI(opcoes =>
        {
            opcoes.SwaggerEndpoint("/openapi/v1.json", "GASTRA API v1");
            opcoes.DocumentTitle = "GASTRA API";
        });

        return app;
    }
}
