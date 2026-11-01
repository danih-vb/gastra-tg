using Gastra.Application.Auditoria;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

// O cliente chama o garçom pelo celular, com o mesmo código de acesso da conta
public interface IChamarGarcomUseCase
{
    Task Executar(string codigoAcesso);
}

public class ChamarGarcomUseCase(
    IRepositorioComanda repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IChamarGarcomUseCase
{
    public async Task Executar(string codigoAcesso)
    {
        var comanda = await repositorio.ObterParaChamada(codigoAcesso)
                      ?? throw new NaoEncontradoException(MensagensErro.ComandaNaoEncontrada);

        if (comanda.Status != StatusComanda.Aberta)
            throw new RegraDeNegocioException(MensagensErro.ComandaNaoEstaAberta);

        // Tocar várias vezes no botão não enche a auditoria: só o primeiro toque é um chamado.
        if (comanda.AtendimentoChamadoEm is not null)
            return;

        comanda.ChamarGarcom(DateTime.UtcNow);
        await auditoria.Registrar(EventoAuditoria.GarcomChamado, alvo: (nameof(Comanda), comanda.Id));
        await unitOfWork.Commit();
    }
}

public interface IAtenderChamadoUseCase
{
    Task Executar(int comandaId);
}

public class AtenderChamadoUseCase(
    IRepositorioComanda repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IAtenderChamadoUseCase
{
    public async Task Executar(int comandaId)
    {
        var comanda = await BuscadorDeComanda.Aberta(repositorio, comandaId);

        comanda.AtenderChamado();

        await auditoria.Registrar(EventoAuditoria.ChamadoAtendido, alvo: (nameof(Comanda), comanda.Id));
        await unitOfWork.Commit();
    }
}
