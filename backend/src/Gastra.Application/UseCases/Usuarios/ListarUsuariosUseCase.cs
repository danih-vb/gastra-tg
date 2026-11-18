using Gastra.Communication.Responses;
using Gastra.Domain.Repositorios;
using Mapster;

namespace Gastra.Application.UseCases.Usuarios;

// UC04 — Gerenciar contas de usuário: listar contas ativas e inativas (RF18)
public interface IListarUsuariosUseCase
{
    Task<List<UsuarioResponse>> Executar();
}

public class ListarUsuariosUseCase(IRepositorioUsuario repositorio) : IListarUsuariosUseCase
{
    public async Task<List<UsuarioResponse>> Executar()
    {
        var usuarios = await repositorio.ListarTodos();
        return usuarios.Adapt<List<UsuarioResponse>>();
    }
}
