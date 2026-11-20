using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Gastra.Infrastructure.DataAccess;

/// <summary>
/// Contexto do Entity Framework: representa o banco de dados do GASTRA.
/// As entidades ficam no Domain, sem atributos do EF; o mapeamento de cada uma fica nesta camada.
/// </summary>
public class GastraDbContext(DbContextOptions<GastraDbContext> options) : DbContext(options)
{
    public DbSet<ItemDoCardapio> ItensCardapio => Set<ItemDoCardapio>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Praca> Pracas => Set<Praca>();
    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<Comanda> Comandas => Set<Comanda>();
    public DbSet<Alocacao> Alocacoes => Set<Alocacao>();
    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();
    public DbSet<Promocao> Promocoes => Set<Promocao>();
    public DbSet<AvaliacaoAtendimento> Avaliacoes => Set<AvaliacaoAtendimento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GastraDbContext).Assembly);
    }

    /// <summary>
    /// Todo horário do banco é gravado em UTC (DateTime.UtcNow), mas o MySQL não guarda o fuso e o EF devolve o valor
    /// sem dizer que é UTC. A API mandava "11:55" sem o "Z", e o navegador mostrava como hora local: três horas a mais
    /// na conta do cliente e "há 0 min" em tudo por três horas. Na leitura, o valor passa a sair marcado como UTC.
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<HorarioEmUtc>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<HorarioEmUtcAnulavel>();
    }

    private sealed class HorarioEmUtc() : ValueConverter<DateTime, DateTime>(
        gravado => gravado,
        lido => DateTime.SpecifyKind(lido, DateTimeKind.Utc));

    private sealed class HorarioEmUtcAnulavel() : ValueConverter<DateTime?, DateTime?>(
        gravado => gravado,
        lido => lido.HasValue ? DateTime.SpecifyKind(lido.Value, DateTimeKind.Utc) : lido);
}
