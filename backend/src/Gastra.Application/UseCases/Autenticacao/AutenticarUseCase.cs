using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Autenticacao;

// UC01 — Autenticar-se (RF15, RF16, RN06)
public interface IAutenticarUseCase
{
    Task<LoginResponse> Executar(LoginRequest request);
}

public class AutenticarUseCase(
    IRepositorioUsuario repositorio,
    ICriptografiaSenha criptografia,
    IGeradorToken geradorToken) : IAutenticarUseCase
{
    // Hash de uma senha aleatória, gerado uma única vez: usado quando o e-mail não existe (ver abaixo).
    private static string? _hashFicticio;

    public async Task<LoginResponse> Executar(LoginRequest request)
    {
        Validar(request);

        var usuario = await repositorio.ObterPorEmail(request.Email);

        // A verificação do hash roda mesmo quando o e-mail não existe: assim o tempo de resposta
        // não revela quais e-mails estão cadastrados (enumeração de usuários).
        _hashFicticio ??= criptografia.GerarHash(Guid.NewGuid().ToString());
        var senhaConfere = criptografia.Verificar(request.Senha, usuario?.SenhaHash ?? _hashFicticio);

        if (usuario is null || !usuario.Ativo || !senhaConfere)
            throw new NaoAutenticadoException(MensagensErro.CredenciaisInvalidas);

        if (usuario.ExigeSegundoFator())
        {
            return new LoginResponse
            {
                RequerSegundoFator = true,
                RequerConfiguracaoSegundoFator = !usuario.SegundoFatorConfigurado,
                TokenSegundoFator = geradorToken.GerarTokenSegundoFator(usuario),
            };
        }

        return RespostaComAcesso.Criar(usuario, geradorToken);
    }

    private static void Validar(LoginRequest request)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
            erros.Add(MensagensErro.EmailObrigatorio);

        if (string.IsNullOrWhiteSpace(request.Senha))
            erros.Add(MensagensErro.SenhaObrigatoria);

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }
}
