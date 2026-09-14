using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess;

/// <summary>
/// Contexto do Entity Framework: representa o banco de dados do GASTRA.
/// As entidades ficam no Domain, sem atributos do EF; o mapeamento de cada uma fica nesta camada.
/// </summary>
public class GastraDbContext(DbContextOptions<GastraDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GastraDbContext).Assembly);
    }
}
