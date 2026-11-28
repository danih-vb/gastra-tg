using Gastra.Application.UseCases.Usuarios;

namespace Gastra.Api.Configuracao;

/// <summary>Tira das fotos públicas as fotos de funcionário gravadas até a versão 1.0 (ver o caso de uso).</summary>
public static class RecolhedorDeFotosAntigas
{
    public static async Task Executar(WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(RecolhedorDeFotosAntigas));

        try
        {
            using var escopo = app.Services.CreateScope();
            var ajustadas = await escopo.ServiceProvider
                .GetRequiredService<IRecolherFotosAntigasDeFuncionariosUseCase>().Executar();

            if (ajustadas > 0)
                logger.LogInformation("Fotos de funcionário tiradas da pasta pública: {Contas} conta(s).", ajustadas);
        }
        catch (Exception excecao)
        {
            // Mesmo critério do primeiro Gerente: sem banco, a API sobe e tenta de novo na próxima vez.
            logger.LogWarning(excecao, "Não foi possível conferir as fotos antigas dos funcionários. O banco está disponível?");
        }
    }
}
