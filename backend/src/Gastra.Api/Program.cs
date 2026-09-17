using System.Text.Json.Serialization;
using Gastra.Api.Configuracao;
using Gastra.Api.Filtros;
using Gastra.Application;
using Gastra.Communication.Responses;
using Gastra.Exceptions;
using Gastra.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAutenticacaoJwt(builder.Configuration);
builder.Services.AddPoliticaCors(builder.Configuration);

builder.Services
    .AddControllers(opcoes => opcoes.Filters.Add<FiltroExcecao>())
    .AddJsonOptions(opcoes => opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(opcoes =>
    {
        // JSON malformado ou enum inexistente: mesmo formato de erro do resto da API.
        opcoes.InvalidModelStateResponseFactory = _ =>
            new BadRequestObjectResult(new ErroResponse(MensagensErro.RequisicaoInvalida));
    });
builder.Services.AddDocumentacaoOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Swagger em /swagger (só em desenvolvimento).
app.UseDocumentacaoOpenApi();

// i18n: o idioma da resposta vem do cabeçalho Accept-Language (pt-BR por padrão, ou en).
// Fica antes da autenticação para que as mensagens de 401/403 também saiam traduzidas.
string[] culturas = ["pt-BR", "en"];
app.UseRequestLocalization(opcoes => opcoes
    .SetDefaultCulture(culturas[0])
    .AddSupportedCultures(culturas)
    .AddSupportedUICultures(culturas));

app.UseHttpsRedirection();

// CORS antes da autenticação: a requisição de verificação (OPTIONS) do navegador não leva token.
app.UseCors(PoliticaCors.Nome);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await InicializadorAdministrador.Executar(app);

app.Run();

// Torna a classe Program visível para os testes de integração (WebApplicationFactory).
public partial class Program;
