using Gastra.Communication.Responses;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;
using Mapster;

namespace Gastra.Application.UseCases.Usuarios;

// UC04 — Gerenciar contas de usuário: consultar uma conta (RF18)
public interface IObterUsuarioUseCase
{
    Task<UsuarioResponse> Executar(int id);
}

public class ObterUsuarioUseCase(IRepositorioUsuario repositorio) : IObterUsuarioUseCase
{
    public async Task<UsuarioResponse> Executar(int id)
    {
        var usuario = await repositorio.ObterPorId(id)
                      ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        return usuario.Adapt<UsuarioResponse>();
    }
}
