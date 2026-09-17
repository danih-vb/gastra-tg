using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Autenticacao;

// UC02 — Confirmar segundo fator (RF16, RN07)
public interface IConfirmarSegundoFatorUseCase
{
    Task<LoginResponse> Executar(SegundoFatorRequest request);
}

public class ConfirmarSegundoFatorUseCase(
    IGeradorToken geradorToken,
    IRepositorioUsuario repositorio,
    IValidadorTotp validadorTotp,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IConfirmarSegundoFatorUseCase
{
    public async Task<LoginResponse> Executar(SegundoFatorRequest request)
    {
        var usuario = await UsuarioDoTokenSegundoFator.Obter(request.TokenSegundoFator, geradorToken, repositorio);

        if (!usuario.SegundoFatorConfigurado)
            throw new RegraDeNegocioException(MensagensErro.SegundoFatorNaoConfigurado);

        // O código digitado nunca vai para a auditoria (RN07).
        if (!validadorTotp.Validar(usuario.SegredoTotp!, request.Codigo))
        {
            await auditoria.Registrar(EventoAuditoria.SegundoFatorRecusado, ResultadoAuditoria.Falha, ator: usuario, comIp: true);
            await unitOfWork.Commit();

            throw new NaoAutenticadoException(MensagensErro.CodigoSegundoFatorInvalido);
        }

        await auditoria.Registrar(EventoAuditoria.SegundoFatorConfirmado, ator: usuario, comIp: true);
        await unitOfWork.Commit();

        return RespostaComAcesso.Criar(usuario, geradorToken);
    }
}
