using Gastra.Application.Auditoria;
using Gastra.Application.Fotos;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Servicos;
using Gastra.Exceptions;
using Mapster;

namespace Gastra.Application.UseCases.Usuarios;

/// <summary>
/// O Gerente põe, troca ou tira a foto de um funcionário, que aparece no crachá do sistema. A foto é dado pessoal:
/// serve só para identificar quem está conectado, não vai para relatório nenhum e sai quando a conta é inativada.
/// Por isso o arquivo fica fora das fotos públicas do cardápio, e o antigo é apagado a cada troca.
/// </summary>
public interface IDefinirFotoUsuarioUseCase
{
    /// <param name="conteudo">O arquivo da foto; nulo tira a foto.</param>
    Task<UsuarioResponse> Executar(int id, byte[]? conteudo);
}

public class DefinirFotoUsuarioUseCase(
    IRepositorioUsuario repositorio,
    IArmazenamentoDeFotosDeFuncionarios armazenamento,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IDefinirFotoUsuarioUseCase
{
    public async Task<UsuarioResponse> Executar(int id, byte[]? conteudo)
    {
        string? extensao = null;
        if (conteudo is not null)
            extensao = FormatoDeFoto.Extensao(conteudo) ?? throw new ErroValidacaoException([MensagensErro.FotoInvalida]);

        var usuario = await repositorio.ObterPorId(id)
                      ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        // A foto só serve para o funcionário em atividade; a inativação já a tirou.
        if (conteudo is not null && !usuario.Ativo)
            throw new RegraDeNegocioException(MensagensErro.FotoDeContaInativa);

        var antiga = FotoDoFuncionario.Nome(usuario.Foto);
        var nova = conteudo is null ? null : await armazenamento.Salvar(conteudo, extensao!);

        usuario.DefinirFoto(nova is null ? null : FotoDoFuncionario.Endereco(nova));

        // Só se a conta passou a ter ou deixou de ter foto: o endereço da imagem não precisa ir para o log.
        await auditoria.Registrar(EventoAuditoria.FotoDoUsuarioAlterada, alvo: (nameof(Usuario), usuario.Id),
            detalhes: new { TemFoto = usuario.Foto is not null });

        try
        {
            await unitOfWork.Commit();
        }
        catch
        {
            // O banco não aceitou: a foto nova não ficou ligada a ninguém.
            if (nova is not null)
                armazenamento.Apagar(nova);
            throw;
        }

        // Só depois de gravar: se o banco falhasse, a conta continuaria apontando para a antiga.
        if (antiga is not null)
            armazenamento.Apagar(antiga);

        return usuario.Adapt<UsuarioResponse>();
    }
}
