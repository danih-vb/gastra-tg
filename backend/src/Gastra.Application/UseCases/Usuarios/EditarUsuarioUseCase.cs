using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
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
    IRegistradorAuditoria auditoria,
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

        // Política de log, 4.1: só os nomes dos campos alterados. O papel vai com valor antigo e novo.
        var nomeAnterior = usuario.Nome;
        var emailAnterior = usuario.Email;
        var papelAnterior = usuario.Papel;

        usuario.AtualizarDados(request.Nome.Trim(), request.Email, papel);

        var camposAlterados = new List<string>();
        if (usuario.Nome != nomeAnterior) camposAlterados.Add("nome");
        if (usuario.Email != emailAnterior) camposAlterados.Add("email");
        var papelMudou = usuario.Papel != papelAnterior;
        if (papelMudou) camposAlterados.Add("papel");

        await auditoria.Registrar(EventoAuditoria.ContaEditada, alvo: (nameof(Usuario), usuario.Id),
            detalhes: new
            {
                CamposAlterados = camposAlterados,
                PapelAnterior = papelMudou ? papelAnterior : (PapelUsuario?)null,
                PapelNovo = papelMudou ? usuario.Papel : (PapelUsuario?)null,
            });
        await unitOfWork.Commit();
    }
}
