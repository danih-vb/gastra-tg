using Gastra.Application.Auditoria;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;
using ComunicacaoEnums = Gastra.Communication.Enums;

namespace Gastra.Application.UseCases.Alocacoes;

// UC21 — Confirmar a alocação do turno (RF07, RF08). A confirmada entra no histórico da RN03.
public interface IConfirmarAlocacaoUseCase
{
    Task<AlocacaoTurnoResponse> Executar(DateOnly data, ComunicacaoEnums.PeriodoAlocacao periodo);
}

public class ConfirmarAlocacaoUseCase(
    IRepositorioAlocacao repositorio,
    IRepositorioUsuario repositorioUsuario,
    IRepositorioPraca repositorioPraca,
    IRepositorioIndicadores indicadores,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IConfirmarAlocacaoUseCase
{
    public async Task<AlocacaoTurnoResponse> Executar(DateOnly data, ComunicacaoEnums.PeriodoAlocacao periodoRequest)
    {
        var periodo = LeitorDoTurno.Periodo(periodoRequest);

        var turno = await repositorio.ListarDoTurno(data, periodo);
        if (turno.Count == 0)
            throw new NaoEncontradoException(MensagensErro.AlocacaoNaoEncontrada);

        LeitorDoTurno.GarantirNaoConfirmado(turno);

        foreach (var alocacao in turno)
            alocacao.Confirmar();

        await auditoria.Registrar(EventoAuditoria.AlocacaoConfirmada,
            detalhes: new { Data = data, Periodo = periodo, Garcons = turno.Count });
        await unitOfWork.Commit();

        var fatores = await FatoresDoTurno.Calcular(
            data, turno.Select(a => a.GarcomId).ToList(), await repositorioPraca.ListarTodas(), indicadores, repositorio);
        return await LeitorDoTurno.Montar(data, periodo, turno, repositorioUsuario, repositorioPraca, fatores);
    }
}
