using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

// UC25 — Avaliar o atendimento (RF25), pelo cliente, sem login
public interface IAvaliarAtendimentoUseCase
{
    Task Executar(string codigoAcesso, AvaliacaoRequest request);
}

/// <summary>
/// Única escrita que o código de acesso do cliente permite. Por isso tudo aqui é restritivo: a conta
/// precisa estar fechada, a avaliação é uma só e o prazo é curto (RN08).
/// </summary>
public class AvaliarAtendimentoUseCase(
    IRepositorioComanda repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IAvaliarAtendimentoUseCase
{
    public async Task Executar(string codigoAcesso, AvaliacaoRequest request)
    {
        ValidadorComanda.ValidarAvaliacao(request);

        // A mesma mensagem para código inexistente e comanda que não pode ser avaliada seria mais
        // discreta, mas piora demais a vida de quem avaliou de boa-fé e recebeu um "não encontrado".
        var comanda = await repositorio.ObterParaAvaliacao(codigoAcesso)
                      ?? throw new NaoEncontradoException(MensagensErro.ComandaNaoEncontrada);

        if (comanda.Avaliacao is not null)
            throw new RegraDeNegocioException(MensagensErro.ComandaJaAvaliada);

        if (!comanda.PodeSerAvaliada(DateTime.UtcNow))
            throw new RegraDeNegocioException(MensagensErro.AvaliacaoForaDoPrazo);

        comanda.Avaliar(request.Nota, request.Comentario, DateTime.UtcNow);

        // Sem ator: quem avalia não se identifica, e é assim que tem de ficar no log. A nota entra
        // porque não identifica ninguém; o comentário não entra, porque é texto livre do cliente.
        await auditoria.Registrar(
            EventoAuditoria.AvaliacaoRecebida,
            alvo: (nameof(Comanda), comanda.Id),
            detalhes: new { nota = request.Nota });

        await unitOfWork.Commit();
    }
}
