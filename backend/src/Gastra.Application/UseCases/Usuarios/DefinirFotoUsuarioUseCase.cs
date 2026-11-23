using Gastra.Application.Auditoria;
using Gastra.Application.UseCases.Cardapio;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;
using Mapster;

namespace Gastra.Application.UseCases.Usuarios;

/// <summary>
/// O Gerente põe ou tira a foto de um funcionário, que aparece no crachá do sistema. A foto é dado pessoal: serve
/// só para identificar quem está conectado, não vai para relatório nenhum e sai quando a conta é inativada.
/// </summary>
public interface IDefinirFotoUsuarioUseCase
{
    Task<UsuarioResponse> Executar(int id, FotoUsuarioRequest request);
}

public class DefinirFotoUsuarioUseCase(
    IRepositorioUsuario repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IDefinirFotoUsuarioUseCase
{
    private const string PrefixoDasFotos = "/api/cardapio/fotos/";

    public async Task<UsuarioResponse> Executar(int id, FotoUsuarioRequest request)
    {
        // Só foto enviada ao próprio sistema: um endereço externo deixaria outro site saber quando a tela é aberta.
        var foto = request.Foto?.Trim();
        if (!string.IsNullOrEmpty(foto) && !(ValidadorItemCardapio.ImagemValida(foto) && foto.StartsWith(PrefixoDasFotos)))
            throw new ErroValidacaoException([MensagensErro.ImagemInvalida]);

        var usuario = await repositorio.ObterPorId(id)
                      ?? throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        usuario.DefinirFoto(request.Foto);

        // Só se a conta passou a ter ou deixou de ter foto: o endereço da imagem não precisa ir para o log.
        await auditoria.Registrar(EventoAuditoria.FotoDoUsuarioAlterada, alvo: (nameof(Usuario), usuario.Id),
            detalhes: new { TemFoto = usuario.Foto is not null });
        await unitOfWork.Commit();

        return usuario.Adapt<UsuarioResponse>();
    }
}
