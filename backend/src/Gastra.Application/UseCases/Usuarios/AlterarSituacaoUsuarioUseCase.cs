using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Domain.Servicos;
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
    IArmazenamentoDeFotosDeFuncionarios fotos,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IAlterarSituacaoUsuarioUseCase
{
    public async Task Executar(int id, SituacaoUsuarioRequest request)
    {
        var usuario = await repositorio.ObterPorId(id)
                      ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        var foto = FotoDoFuncionario.Nome(usuario.Foto);

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

        await auditoria.Registrar(request.Ativo ? EventoAuditoria.ContaReativada : EventoAuditoria.ContaInativada,
            alvo: (nameof(Usuario), usuario.Id));
        await unitOfWork.Commit();

        // LGPD: a inativação tirou a foto da conta; o arquivo sai do disco junto, depois de gravado no banco.
        if (!request.Ativo && foto is not null)
            fotos.Apagar(foto);
    }
}
