using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess.Repositorios;

public class RepositorioAuditoria(GastraDbContext contexto) : IRepositorioAuditoria
{
    public async Task Adicionar(RegistroAuditoria registro) => await contexto.RegistrosAuditoria.AddAsync(registro);

    // DELETE direto no banco, sem carregar os registros: a tabela cresce a cada login.
    public Task<int> EliminarAnterioresA(DateTime limiteUtc) =>
        contexto.RegistrosAuditoria.Where(r => r.DataHoraUtc < limiteUtc).ExecuteDeleteAsync();
}
