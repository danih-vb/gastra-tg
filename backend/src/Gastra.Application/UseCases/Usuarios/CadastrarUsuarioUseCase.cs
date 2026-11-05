using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;
using Mapster;
using DominioEnums = Gastra.Domain.Enums;

namespace Gastra.Application.UseCases.Usuarios;

// UC04 — Gerenciar contas de usuário: cadastrar (RF18)
public interface ICadastrarUsuarioUseCase
{
    Task<UsuarioResponse> Executar(CadastrarUsuarioRequest request);
}

public class CadastrarUsuarioUseCase(
    IRepositorioUsuario repositorio,
    ICriptografiaSenha criptografia,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : ICadastrarUsuarioUseCase
{
    public async Task<UsuarioResponse> Executar(CadastrarUsuarioRequest request)
    {
        ValidadorUsuario.ValidarCadastro(request.Nome, request.Email, request.Senha, request.Papel);

        if (await repositorio.ObterPorEmail(request.Email) is not null)
            throw new RegraDeNegocioException(MensagensErro.EmailJaCadastrado);

        // RN06: a senha é guardada só como hash.
        var usuario = new Usuario(
            request.Nome.Trim(),
            request.Email,
            criptografia.GerarHash(request.Senha),
            request.Papel.Adapt<DominioEnums.PapelUsuario>());

        await repositorio.Adicionar(usuario);
        await unitOfWork.Commit();

        // Segundo Commit: o id da conta só existe depois de gravada. Nome e e-mail não vão (política, 4.1).
        await auditoria.Registrar(EventoAuditoria.ContaCriada, alvo: (nameof(Usuario), usuario.Id),
            detalhes: new { usuario.Papel });
        await unitOfWork.Commit();

        return usuario.Adapt<UsuarioResponse>();
    }
}
