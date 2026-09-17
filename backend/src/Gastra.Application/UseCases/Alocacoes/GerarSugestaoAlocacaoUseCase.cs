using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Alocacoes;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Servicos;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Alocacoes;

// UC15 — Gerar sugestão de alocação de garçons (RF06, RN03)
public interface IGerarSugestaoAlocacaoUseCase
{
    Task<AlocacaoTurnoResponse> Executar(SugestaoAlocacaoRequest request);
}

public class GerarSugestaoAlocacaoUseCase(
    IRepositorioAlocacao repositorio,
    IRepositorioUsuario repositorioUsuario,
    IRepositorioPraca repositorioPraca,
    IRepositorioIndicadores indicadores,
    IServicoAnalitico servicoAnalitico,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IGerarSugestaoAlocacaoUseCase
{
    public async Task<AlocacaoTurnoResponse> Executar(SugestaoAlocacaoRequest request)
    {
        var periodo = LeitorDoTurno.Periodo(request.Periodo);
        if (request.Data == default)
            throw new ErroValidacaoException([MensagensErro.DataAlocacaoObrigatoria]);

        var garcomIds = request.GarcomIds.Distinct().ToList();
        if (garcomIds.Count == 0)
            throw new ErroValidacaoException([MensagensErro.AlocacaoSemGarcons]);

        var turnoAtual = await repositorio.ListarDoTurno(request.Data, periodo);
        LeitorDoTurno.GarantirNaoConfirmado(turnoAtual);

        var garcons = await repositorioUsuario.ListarPorIds(garcomIds);
        if (garcons.Count != garcomIds.Count || garcons.Any(g => !g.Ativo || g.Papel != PapelUsuario.Garcom))
            throw new RegraDeNegocioException(MensagensErro.AlocacaoGarcomInvalido);

        var pracas = await repositorioPraca.ListarTodas();
        if (pracas.Count == 0)
            throw new RegraDeNegocioException(MensagensErro.AlocacaoSemPracas);
        if (pracas.Sum(p => p.QuantidadeGarcons) < garcomIds.Count)
            throw new RegraDeNegocioException(MensagensErro.AlocacaoSemVagas);

        var (garconsDoTurno, pracasDoTurno) = await MontarFatores(request.Data, garcomIds, pracas);

        IReadOnlyList<DesignacaoSugerida> designacoes;
        try
        {
            designacoes = await servicoAnalitico.SugerirAlocacao(
                garconsDoTurno, pracasDoTurno, RegraDeDistribuicao.PesoDesequilibrio, RegraDeDistribuicao.PesoEspera);
            GarantirResultadoValido(designacoes, garcomIds, pracas);
        }
        catch (ServicoAnaliticoIndisponivelException)
        {
            // D3: sem o Python, o Metre aloca garçom por garçom (UC22). O que já havia no turno fica como estava.
            await auditoria.Registrar(EventoAuditoria.SugestaoAlocacaoGerada, ResultadoAuditoria.Falha,
                detalhes: new { request.Data, Periodo = periodo, Motivo = "servico_analitico_indisponivel" });
            await unitOfWork.Commit();

            return await LeitorDoTurno.Montar(request.Data, periodo, turnoAtual, repositorioUsuario, repositorioPraca, servicoDisponivel: false);
        }

        // A sugestão nova substitui a anterior não confirmada. Dois Commits: o índice único (data, período,
        // garçom) recusaria a inclusão se ela fosse gravada antes da remoção.
        repositorio.Remover(turnoAtual);
        await unitOfWork.Commit();

        var novas = designacoes.Select(d => new Alocacao(request.Data, periodo, d.GarcomId, d.PracaId)).ToList();
        foreach (var alocacao in novas)
            await repositorio.Adicionar(alocacao);

        await auditoria.Registrar(EventoAuditoria.SugestaoAlocacaoGerada, detalhes: new
        {
            request.Data,
            Periodo = periodo,
            Garcons = garcomIds.Count,
            RegraDeDistribuicao.PesoDesequilibrio,
            RegraDeDistribuicao.PesoEspera,
        });
        await unitOfWork.Commit();

        return await LeitorDoTurno.Montar(request.Data, periodo, novas, repositorioUsuario, repositorioPraca, servicoDisponivel: true);
    }

    /// <summary>RN03: faturamento por turno e espera de cada garçom; potencial e vagas de cada praça.</summary>
    private async Task<(List<GarcomParaAlocacao>, List<PracaParaAlocacao>)> MontarFatores(
        DateOnly data, List<int> garcomIds, List<Praca> pracas)
    {
        var faturamentoMedio = await indicadores.ObterFaturamentoMedioPorPraca();
        var faturamentoPorTurno = await indicadores.ObterFaturamentoMedioPorTurnoDoGarcom(
            data.AddDays(-RegraDeDistribuicao.DiasDaJanelaDeFaturamento), data);
        var altoPotencial = RegraDeDistribuicao.PracasDeAltoPotencial(faturamentoMedio);
        var historico = await repositorio.ListarPracasConfirmadasAntesDe(garcomIds, data);

        var garcons = garcomIds.Select(id => new GarcomParaAlocacao(
            id,
            faturamentoPorTurno.GetValueOrDefault(id),
            RegraDeDistribuicao.TurnosDesdePracaDeAltoPotencial(historico.GetValueOrDefault(id, []), altoPotencial))).ToList();

        var pracasDoTurno = pracas.Select(p => new PracaParaAlocacao(
            p.Id, p.QuantidadeGarcons, faturamentoMedio.GetValueOrDefault(p.Id))).ToList();

        return (garcons, pracasDoTurno);
    }

    /// <summary>
    /// Confere a resposta do serviço antes de gravar. Resposta incoerente é tratada como serviço indisponível:
    /// nunca vai para o banco uma alocação que deixa garçom de fora ou lota uma praça.
    /// </summary>
    private static void GarantirResultadoValido(IReadOnlyList<DesignacaoSugerida> designacoes, List<int> garcomIds, List<Praca> pracas)
    {
        var vagas = pracas.ToDictionary(p => p.Id, p => p.QuantidadeGarcons);

        var coerente = designacoes.Count == garcomIds.Count
                       && designacoes.Select(d => d.GarcomId).Order().SequenceEqual(garcomIds.Order())
                       && designacoes.All(d => vagas.ContainsKey(d.PracaId))
                       && designacoes.GroupBy(d => d.PracaId).All(g => g.Count() <= vagas[g.Key]);

        if (!coerente)
            throw new ServicoAnaliticoIndisponivelException("resposta de alocação incoerente");
    }
}
