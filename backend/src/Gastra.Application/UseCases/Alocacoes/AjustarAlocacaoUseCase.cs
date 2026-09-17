using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;
using ComunicacaoEnums = Gastra.Communication.Enums;

namespace Gastra.Application.UseCases.Alocacoes;

// UC22 — Ajustar a alocação sugerida (RF07). Também serve para alocar à mão quando o serviço analítico está fora (D3).
public interface IAjustarAlocacaoUseCase
{
    Task<AlocacaoTurnoResponse> Executar(DateOnly data, ComunicacaoEnums.PeriodoAlocacao periodo, int garcomId, AjusteAlocacaoRequest request);
}

public class AjustarAlocacaoUseCase(
    IRepositorioAlocacao repositorio,
    IRepositorioUsuario repositorioUsuario,
    IRepositorioPraca repositorioPraca,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IAjustarAlocacaoUseCase
{
    public async Task<AlocacaoTurnoResponse> Executar(
        DateOnly data, ComunicacaoEnums.PeriodoAlocacao periodoRequest, int garcomId, AjusteAlocacaoRequest request)
    {
        var periodo = LeitorDoTurno.Periodo(periodoRequest);

        var turno = await repositorio.ListarDoTurno(data, periodo);
        LeitorDoTurno.GarantirNaoConfirmado(turno);

        var garcom = await repositorioUsuario.ObterPorId(garcomId);
        if (garcom is null || !garcom.Ativo || garcom.Papel != PapelUsuario.Garcom)
            throw new RegraDeNegocioException(MensagensErro.AlocacaoGarcomInvalido);

        var praca = await repositorioPraca.ObterPorId(request.PracaId)
                    ?? throw new NaoEncontradoException(MensagensErro.PracaNaoEncontrada);

        var ocupadas = turno.Count(a => a.PracaId == praca.Id && a.GarcomId != garcomId);
        if (ocupadas >= praca.QuantidadeGarcons)
            throw new RegraDeNegocioException(MensagensErro.PracaSemVaga);

        var alocacao = turno.FirstOrDefault(a => a.GarcomId == garcomId);
        int? pracaSugerida = alocacao?.PracaId;

        if (alocacao is null)
        {
            alocacao = new Alocacao(data, periodo, garcomId, praca.Id);
            await repositorio.Adicionar(alocacao);
            turno.Add(alocacao);
        }
        else
        {
            alocacao.Ajustar(praca.Id);
        }

        // Política de log, 4.5: prova de quem mudou a sugestão da RN03, e para onde.
        await auditoria.Registrar(EventoAuditoria.AlocacaoAjustada, detalhes: new
        {
            Data = data,
            Periodo = periodo,
            GarcomId = garcomId,
            PracaSugerida = pracaSugerida,
            PracaEscolhida = praca.Id,
        });
        await unitOfWork.Commit();

        return await LeitorDoTurno.Montar(data, periodo, turno, repositorioUsuario, repositorioPraca);
    }
}
