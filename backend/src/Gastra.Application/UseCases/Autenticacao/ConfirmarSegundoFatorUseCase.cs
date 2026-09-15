using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
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
    IValidadorTotp validadorTotp) : IConfirmarSegundoFatorUseCase
{
    public async Task<LoginResponse> Executar(SegundoFatorRequest request)
    {
        var usuario = await UsuarioDoTokenSegundoFator.Obter(request.TokenSegundoFator, geradorToken, repositorio);

        if (!usuario.SegundoFatorConfigurado)
            throw new RegraDeNegocioException(MensagensErro.SegundoFatorNaoConfigurado);

        if (!validadorTotp.Validar(usuario.SegredoTotp!, request.Codigo))
            throw new NaoAutenticadoException(MensagensErro.CodigoSegundoFatorInvalido);

        return RespostaComAcesso.Criar(usuario, geradorToken);
    }
}
