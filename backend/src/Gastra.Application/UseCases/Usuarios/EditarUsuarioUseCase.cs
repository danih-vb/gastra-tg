using Gastra.Communication.Requests;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;
using Mapster;
using DominioEnums = Gastra.Domain.Enums;

namespace Gastra.Application.UseCases.Usuarios;

// UC04 — Gerenciar contas de usuário: editar nome, e-mail e papel (RF18)
public interface IEditarUsuarioUseCase
{
    Task Executar(int id, EditarUsuarioRequest request);
}

public class EditarUsuarioUseCase(
    IRepositorioUsuario repositorio,
    IUsuarioLogado usuarioLogado,
    IUnitOfWork unitOfWork) : IEditarUsuarioUseCase
{
    public async Task Executar(int id, EditarUsuarioRequest request)
    {
        ValidadorUsuario.ValidarDados(request.Nome, request.Email, request.Papel);

        var usuario = await repositorio.ObterPorId(id)
                      ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        var papel = request.Papel.Adapt<DominioEnums.PapelUsuario>();

        // Se o Gerente pudesse rebaixar a si mesmo, o restaurante poderia ficar sem nenhum Gerente.
        var gerenteLogado = await usuarioLogado.Obter();
        if (gerenteLogado.Id == usuario.Id && papel != usuario.Papel)
            throw new RegraDeNegocioException(MensagensErro.AlterarProprioPapel);

        var donoDoEmail = await repositorio.ObterPorEmail(request.Email);
        if (donoDoEmail is not null && donoDoEmail.Id != usuario.Id)
            throw new RegraDeNegocioException(MensagensErro.EmailJaCadastrado);

        usuario.AtualizarDados(request.Nome.Trim(), request.Email, papel);
        await unitOfWork.Commit();
    }
}
