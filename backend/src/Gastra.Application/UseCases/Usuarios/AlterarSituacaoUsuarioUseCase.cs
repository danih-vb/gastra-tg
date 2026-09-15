using Gastra.Communication.Requests;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Usuarios;

// UC04 — Gerenciar contas de usuário: inativar (soft delete) e reativar (RF18)
public interface IAlterarSituacaoUsuarioUseCase
{
    Task Executar(int id, SituacaoUsuarioRequest request);
}

public class AlterarSituacaoUsuarioUseCase(
    IRepositorioUsuario repositorio,
    IUsuarioLogado usuarioLogado,
    IUnitOfWork unitOfWork) : IAlterarSituacaoUsuarioUseCase
{
    public async Task Executar(int id, SituacaoUsuarioRequest request)
    {
        var usuario = await repositorio.ObterPorId(id)
                      ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        if (request.Ativo)
        {
            usuario.Reativar();
        }
        else
        {
            // Mesmo motivo da edição: o Gerente não pode trancar a si mesmo para fora do sistema.
            var gerenteLogado = await usuarioLogado.Obter();
            if (gerenteLogado.Id == usuario.Id)
                throw new RegraDeNegocioException(MensagensErro.InativarPropriaConta);

            // A conta continua no banco (histórico de comandas e auditoria), mas perde o acesso na hora.
            usuario.Inativar();
        }

        await unitOfWork.Commit();
    }
}
