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
    IRepositorioIndicadores indicadores,
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

        var alocacao = turno.FirstOrDefault(a => a.GarcomId == garcomId);
        int? pracaDeOrigem = alocacao?.PracaId;

        // UC22 com troca (#140): no turno em que há um garçom para cada vaga, todas as praças ficam cheias, e mover
        // um garçom sozinho seria sempre recusado. Na troca, a ocupação das praças não muda, por isso não há
        // verificação de vaga.
        var trocado = request.TrocarComGarcomId is null ? null : Trocar(turno, garcomId, praca.Id, pracaDeOrigem, request.TrocarComGarcomId.Value);

        if (trocado is null)
        {
            var ocupadas = turno.Count(a => a.PracaId == praca.Id && a.GarcomId != garcomId);
            if (ocupadas >= praca.QuantidadeGarcons)
                throw new RegraDeNegocioException(MensagensErro.PracaSemVaga);
        }

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
            PracaSugerida = pracaDeOrigem,
            PracaEscolhida = praca.Id,
            TrocaComGarcomId = trocado?.GarcomId,
        });

        // A troca move duas pessoas: as duas mudanças ficam registradas, no mesmo commit.
        if (trocado is not null)
        {
            await auditoria.Registrar(EventoAuditoria.AlocacaoAjustada, detalhes: new
            {
                Data = data,
                Periodo = periodo,
                GarcomId = trocado.GarcomId,
                PracaSugerida = praca.Id,
                PracaEscolhida = pracaDeOrigem,
                TrocaComGarcomId = garcomId,
            });
        }

        await unitOfWork.Commit();

        var fatores = await FatoresDoTurno.Calcular(
            data, turno.Select(a => a.GarcomId).ToList(), await repositorioPraca.ListarTodas(), indicadores, repositorio);
        return await LeitorDoTurno.Montar(data, periodo, turno, repositorioUsuario, repositorioPraca, fatores);
    }

    /// <summary>
    /// Põe o outro garçom no lugar deixado por este: a praça de origem dele ou, quando ele ainda não tinha praça,
    /// nenhuma. Devolve a alocação do outro garçom, já ajustada.
    /// </summary>
    private Alocacao Trocar(List<Alocacao> turno, int garcomId, int pracaDeDestino, int? pracaDeOrigem, int outroGarcomId)
    {
        if (outroGarcomId == garcomId)
            throw new RegraDeNegocioException(MensagensErro.TrocaComOProprioGarcom);

        if (pracaDeOrigem == pracaDeDestino)
            throw new RegraDeNegocioException(MensagensErro.TrocaNaMesmaPraca);

        var outro = turno.FirstOrDefault(a => a.GarcomId == outroGarcomId && a.PracaId == pracaDeDestino)
                    ?? throw new RegraDeNegocioException(MensagensErro.GarcomDaTrocaForaDaPraca);

        if (pracaDeOrigem is { } origem)
        {
            outro.Ajustar(origem);
        }
        else
        {
            repositorio.Remover([outro]);
            turno.Remove(outro);
        }

        return outro;
    }
}
