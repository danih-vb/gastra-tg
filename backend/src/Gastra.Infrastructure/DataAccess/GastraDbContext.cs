using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GastraDbContext).Assembly);
    }
}
