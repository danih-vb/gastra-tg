using Gastra.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Api.Configuracao;

/// <summary>
/// Aplica as migrations pendentes ao subir a API, só quando "Banco:AplicarMigrationsAoIniciar" é verdadeiro. É o caso
/// do sistema inteiro em Docker (perfil "app" do compose), em que não há ninguém para rodar <c>dotnet ef</c>. No
/// desenvolvimento, a opção fica desligada e as migrations continuam sendo aplicadas à mão.
/// </summary>
public static class AplicadorDeMigrations
{
    public static async Task Executar(WebApplication app)
    {
        if (!app.Configuration.GetValue("Banco:AplicarMigrationsAoIniciar", false))
            return;

        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AplicadorDeMigrations));

        using var escopo = app.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<GastraDbContext>();

        var pendentes = (await contexto.Database.GetPendingMigrationsAsync()).ToList();
        if (pendentes.Count == 0)
            return;

        // Sem try/catch de propósito: se o banco não aceitar as migrations, a API não deve subir pela metade.
        await contexto.Database.MigrateAsync();
        logger.LogInformation("Migrations aplicadas: {Migrations}", string.Join(", ", pendentes));
    }
}
