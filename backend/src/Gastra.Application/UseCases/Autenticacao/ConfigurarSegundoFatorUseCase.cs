using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Autenticacao;

// Vinculação do app autenticador (RN07) — primeiro acesso de Gerente/Coordenador
public interface IConfigurarSegundoFatorUseCase
{
    Task<ConfiguracaoSegundoFatorResponse> Executar(SegundoFatorRequest request);
}

public class ConfigurarSegundoFatorUseCase(
    IGeradorToken geradorToken,
    IRepositorioUsuario repositorio,
    IValidadorTotp validadorTotp,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IConfigurarSegundoFatorUseCase
{
    public async Task<ConfiguracaoSegundoFatorResponse> Executar(SegundoFatorRequest request)
    {
        var usuario = await UsuarioDoTokenSegundoFator.Obter(request.TokenSegundoFator, geradorToken, repositorio);

        // RN07: o segredo é gerado uma única vez e nunca é reexibido.
        if (usuario.SegundoFatorConfigurado)
            throw new RegraDeNegocioException(MensagensErro.SegundoFatorJaConfigurado);

        var segredo = validadorTotp.GerarSegredo(usuario.Email);
        usuario.DefinirSegredoTotp(segredo.SegredoProtegido);
        await auditoria.Registrar(EventoAuditoria.AutenticadorVinculado, ator: usuario, comIp: true);
        await unitOfWork.Commit();

        return new ConfiguracaoSegundoFatorResponse
        {
            UriConfiguracao = segredo.UriConfiguracao,
            ChaveManual = segredo.ChaveManual,
        };
    }
}

internal static class UsuarioDoTokenSegundoFator
{
    public static async Task<Usuario> Obter(string token, IGeradorToken geradorToken, IRepositorioUsuario repositorio)
    {
        var id = string.IsNullOrWhiteSpace(token) ? null : await geradorToken.ValidarTokenSegundoFator(token);
        var usuario = id is null ? null : await repositorio.ObterPorId(id.Value);

        if (usuario is null || !usuario.Ativo || !usuario.ExigeSegundoFator())
            throw new NaoAutenticadoException(MensagensErro.TokenSegundoFatorInvalido);

        return usuario;
    }
}
