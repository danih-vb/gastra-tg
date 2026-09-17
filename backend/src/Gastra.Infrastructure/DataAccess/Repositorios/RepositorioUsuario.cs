using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Gastra.Infrastructure.DataAccess.Repositorios;

public class RepositorioUsuario(GastraDbContext contexto) : IRepositorioUsuario
{
    public async Task Adicionar(Usuario usuario) => await contexto.Usuarios.AddAsync(usuario);

    public async Task<Usuario?> ObterPorId(int id) =>
        await contexto.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Usuario?> ObterPorEmail(string email)
    {
        var normalizado = Usuario.NormalizarEmail(email);
        return await contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == normalizado);
    }

    public async Task<List<Usuario>> ListarTodos() =>
        await contexto.Usuarios.AsNoTracking().OrderBy(u => u.Nome).ToListAsync();

    public async Task<List<Usuario>> ListarPorIds(IEnumerable<int> ids)
    {
        var lista = ids.Distinct().ToList();
        return await contexto.Usuarios.AsNoTracking().Where(u => lista.Contains(u.Id)).ToListAsync();
    }

    public async Task<bool> ExisteAlgum() => await contexto.Usuarios.AnyAsync();
}
