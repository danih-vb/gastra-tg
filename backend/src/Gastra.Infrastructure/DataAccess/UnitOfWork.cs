using Gastra.Domain.Repositorios;

namespace Gastra.Infrastructure.DataAccess;

public class UnitOfWork(GastraDbContext contexto) : IUnitOfWork
{
    public async Task Commit() => await contexto.SaveChangesAsync();
}
