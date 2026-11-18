using Gastra.Application.Auditoria;
using Gastra.Application.UseCases.Auditoria;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Enums;
using Gastra.Domain.Seguranca;
using Gastra.Infrastructure.DataAccess;
using Gastra.Infrastructure.DataAccess.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Api.Tests.BancoDeDados;

/// <summary>
/// A eliminação por prazo roda contra o MySQL real: o DELETE em lote não existe no banco em memória, e o
/// trigger de retenção precisa aceitar exatamente o que a aplicação apaga.
/// </summary>
public class EliminacaoAuditoriaTests : IAsyncLifetime
{
    private readonly BancoMySqlDeTeste _banco = new();

    public Task InitializeAsync() => _banco.InitializeAsync();

    public Task DisposeAsync() => _banco.DisposeAsync();

    private sealed class SemRequisicao : IUsuarioLogado, IContextoRequisicao
    {
        public Task<Domain.Entidades.Usuario> Obter() => throw new InvalidOperationException();
        public PapelUsuario ObterPapel() => throw new InvalidOperationException();
        public (int Id, PapelUsuario Papel)? ObterIdentificacao() => null;
        public string? ObterIp() => null;
        public string? ObterIdCorrelacao() => null;
    }

    private Task InserirComIdade(string intervalo, string evento) => _banco.Executar(
        $"INSERT INTO registro_auditoria (data_hora_utc, evento, resultado) VALUES (UTC_TIMESTAMP(6) - INTERVAL {intervalo}, '{evento}', 'Sucesso');");

    [FactComMySql]
    public async Task Elimina_so_o_vencido_respeitando_a_folga_do_trigger_e_registra_a_eliminacao()
    {
        await InserirComIdade("2 YEAR", "VENCIDO_HA_MUITO");
        await InserirComIdade("6 MONTH - INTERVAL 3 DAY", "VENCIDO_HA_POUCO");
        // Passou dos 6 meses do trigger, mas está dentro do dia de folga da aplicação: fica para amanhã.
        await InserirComIdade("6 MONTH - INTERVAL 12 HOUR", "NA_FOLGA");
        await InserirComIdade("5 MONTH", "DENTRO_DO_PRAZO");

        await using var contexto = _banco.CriarContexto();
        var contextoRequisicao = new SemRequisicao();
        var casoDeUso = new EliminarAuditoriaVencidaUseCase(
            new RepositorioAuditoria(contexto),
            new RegistradorAuditoria(new RepositorioAuditoria(contexto), contextoRequisicao, contextoRequisicao),
            new UnitOfWork(contexto));

        var eliminados = await casoDeUso.Executar(DateTime.UtcNow);

        Assert.Equal(2, eliminados);
        var eventos = await contexto.RegistrosAuditoria.AsNoTracking().Select(r => r.Evento).ToListAsync();
        Assert.Equal(["NA_FOLGA", "DENTRO_DO_PRAZO", EventoAuditoria.AuditoriaEliminadaPorPrazo], eventos);
        var registro = await contexto.RegistrosAuditoria.AsNoTracking().SingleAsync(r => r.Evento == EventoAuditoria.AuditoriaEliminadaPorPrazo);
        Assert.Contains("\"quantidade\":2", registro.Detalhes);
        Assert.Null(registro.UsuarioId);
    }

    [FactComMySql]
    public async Task Sem_nada_vencido_nao_apaga_nem_registra()
    {
        await InserirComIdade("1 DAY", "RECENTE");

        await using var contexto = _banco.CriarContexto();
        var semRequisicao = new SemRequisicao();
        var eliminados = await new EliminarAuditoriaVencidaUseCase(
            new RepositorioAuditoria(contexto),
            new RegistradorAuditoria(new RepositorioAuditoria(contexto), semRequisicao, semRequisicao),
            new UnitOfWork(contexto)).Executar(DateTime.UtcNow);

        Assert.Equal(0, eliminados);
        Assert.Equal(1, await contexto.RegistrosAuditoria.CountAsync());
    }
}
