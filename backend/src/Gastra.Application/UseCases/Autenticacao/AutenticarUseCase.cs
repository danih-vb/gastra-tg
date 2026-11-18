using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Autenticacao;

// UC01 — Autenticar-se (RF15, RF16, RN06, RN09)
public interface IAutenticarUseCase
{
    Task<LoginResponse> Executar(LoginRequest request);
}

public class AutenticarUseCase(
    IRepositorioUsuario repositorio,
    ICriptografiaSenha criptografia,
    IGeradorToken geradorToken,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IAutenticarUseCase
{
    // Hash de uma senha aleatória, gerado uma única vez: usado quando o e-mail não existe (ver abaixo).
    private static string? _hashFicticio;

    public async Task<LoginResponse> Executar(LoginRequest request)
    {
        Validar(request);

        var agora = DateTime.UtcNow;
        var usuario = await repositorio.ObterPorEmail(request.Email);

        // A verificação do hash roda mesmo quando o e-mail não existe: assim o tempo de resposta
        // não revela quais e-mails estão cadastrados (enumeração de usuários).
        _hashFicticio ??= criptografia.GerarHash(Guid.NewGuid().ToString());
        var senhaConfere = criptografia.Verificar(request.Senha, usuario?.SenhaHash ?? _hashFicticio);

        // RN09: bloqueada, a conta recusa até a senha certa — senão o bloqueio não pararia quem já acertou.
        var bloqueada = usuario is not null && usuario.EstaBloqueada(agora);

        if (usuario is null || !usuario.Ativo || bloqueada || !senhaConfere)
        {
            // Política de log, 4.1: o e-mail digitado nunca vai para a auditoria. Sem conta, só o motivo.
            var motivo = usuario is null ? "conta_nao_encontrada"
                : !usuario.Ativo ? "conta_inativa"
                : bloqueada ? "conta_bloqueada"
                : "senha_incorreta";
            var bloqueouAgora = motivo == "senha_incorreta" && usuario!.RegistrarTentativaFalha(agora);

            await auditoria.Registrar(EventoAuditoria.LoginFalha, ResultadoAuditoria.Falha,
                detalhes: bloqueouAgora ? new { Motivo = motivo, ContaBloqueada = true } : new { Motivo = motivo },
                ator: usuario, comIp: true);
            await unitOfWork.Commit();

            // A mesma mensagem para os quatro motivos: se "conta bloqueada" tivesse texto próprio, cinco senhas
            // erradas revelariam quais e-mails existem (só conta existente bloqueia). O texto já avisa da regra.
            throw new NaoAutenticadoException(MensagensErro.CredenciaisInvalidas);
        }

        // Com segundo fator, o acesso só está completo depois do código (RN09): a contagem fica para lá.
        if (!usuario.ExigeSegundoFator())
            usuario.RegistrarAcessoCompleto();

        await auditoria.Registrar(EventoAuditoria.LoginSucesso,
            detalhes: usuario.ExigeSegundoFator() ? new { SegundoFatorPendente = true } : null,
            ator: usuario, comIp: true);
        await unitOfWork.Commit();

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
