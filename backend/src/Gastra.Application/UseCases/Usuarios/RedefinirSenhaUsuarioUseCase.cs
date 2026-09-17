using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Usuarios;

// UC04 — O Gerente define uma senha nova para quem esqueceu a sua (#141).
public interface IRedefinirSenhaUsuarioUseCase
{
    Task Executar(int id, RedefinirSenhaRequest request);
}

public class RedefinirSenhaUsuarioUseCase(
    IRepositorioUsuario repositorio,
    ICriptografiaSenha criptografia,
    IUsuarioLogado usuarioLogado,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IRedefinirSenhaUsuarioUseCase
{
    public async Task Executar(int id, RedefinirSenhaRequest request)
    {
        ValidadorUsuario.ValidarSenha(request.Senha);

        var usuario = await repositorio.ObterPorId(id)
                      ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        // Pelo mesmo motivo da inativação: quem age não mexe na própria conta por esta porta.
        var gerenteLogado = await usuarioLogado.Obter();
        if (gerenteLogado.Id == usuario.Id)
            throw new RegraDeNegocioException(MensagensErro.RedefinirPropriaSenha);

        // RN06: só o hash é guardado. A senha em si nunca vai para o banco nem para o log.
        usuario.RedefinirSenha(criptografia.GerarHash(request.Senha));

        await auditoria.Registrar(EventoAuditoria.SenhaRedefinida, alvo: (nameof(Usuario), usuario.Id));
        await unitOfWork.Commit();
    }
}
