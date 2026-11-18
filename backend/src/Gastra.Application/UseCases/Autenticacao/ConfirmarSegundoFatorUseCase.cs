using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Autenticacao;

// UC02 — Confirmar segundo fator (RF16, RN07, RN09)
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

        var agora = DateTime.UtcNow;

        // RN09: aqui quem pergunta já passou pela senha, então dizer que a conta está bloqueada não revela nada.
        if (usuario.EstaBloqueada(agora))
        {
            await auditoria.Registrar(EventoAuditoria.SegundoFatorRecusado, ResultadoAuditoria.Falha,
                detalhes: new { Motivo = "conta_bloqueada" }, ator: usuario, comIp: true);
            await unitOfWork.Commit();

            throw new NaoAutenticadoException(MensagensErro.AcessoBloqueadoTemporariamente);
        }

        // O código digitado nunca vai para a auditoria (RN07).
        if (!validadorTotp.Validar(usuario.SegredoTotp!, request.Codigo))
        {
            var bloqueouAgora = usuario.RegistrarTentativaFalha(agora);
            await auditoria.Registrar(EventoAuditoria.SegundoFatorRecusado, ResultadoAuditoria.Falha,
                detalhes: bloqueouAgora ? new { ContaBloqueada = true } : null, ator: usuario, comIp: true);
            await unitOfWork.Commit();

            throw new NaoAutenticadoException(bloqueouAgora
                ? MensagensErro.AcessoBloqueadoTemporariamente
                : MensagensErro.CodigoSegundoFatorInvalido);
        }

        usuario.RegistrarAcessoCompleto();
        await auditoria.Registrar(EventoAuditoria.SegundoFatorConfirmado, ator: usuario, comIp: true);
        await unitOfWork.Commit();

        return RespostaComAcesso.Criar(usuario, geradorToken);
    }
}
