using Gastra.Application.UseCases.Auditoria;

namespace Gastra.Api.Configuracao;

/// <summary>
/// Roda a eliminação da auditoria vencida ao subir a API e depois uma vez por dia. Desligável por
/// "Auditoria:EliminacaoAutomatica" (os testes desligam, porque o banco em memória não executa DELETE em lote).
/// </summary>
public class EliminacaoAuditoriaPorPrazo(IServiceScopeFactory escopos, ILogger<EliminacaoAuditoriaPorPrazo> logger)
    : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var relogio = new PeriodicTimer(Intervalo);

        do
        {
            try
            {
                using var escopo = escopos.CreateScope();
                var quantidade = await escopo.ServiceProvider.GetRequiredService<IEliminarAuditoriaVencidaUseCase>()
                    .Executar(DateTime.UtcNow);

                if (quantidade > 0)
                    logger.LogInformation("Auditoria: {Quantidade} registros eliminados por prazo de retenção.", quantidade);
            }
            catch (Exception excecao) when (excecao is not OperationCanceledException)
            {
                // Banco fora do ar não pode derrubar a API: tenta de novo no próximo ciclo.
                logger.LogWarning(excecao, "Não foi possível eliminar a auditoria vencida.");
            }
        }
        while (await relogio.WaitForNextTickAsync(stoppingToken));
    }
}
