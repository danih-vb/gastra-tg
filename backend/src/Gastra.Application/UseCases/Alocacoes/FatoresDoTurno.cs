using Gastra.Domain.Alocacoes;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Servicos;

namespace Gastra.Application.UseCases.Alocacoes;

/// <summary>
/// RN03: os fatores de um turno, lidos do histórico até a véspera. Servem duas vezes: vão para a programação linear
/// (valores) e voltam para a tela do Metre como explicação da sugestão (faixa de faturamento, turnos desde a praça
/// de alto potencial e quais praças são de alto potencial). Como dependem só do histórico anterior à data, dá para
/// recalcular a qualquer momento e a explicação não se perde ao recarregar a tela.
/// </summary>
internal sealed record FatoresDoTurno(
    List<GarcomParaAlocacao> Garcons,
    List<PracaParaAlocacao> Pracas,
    IReadOnlySet<int> PracasDeAltoPotencial,
    IReadOnlyDictionary<int, FaixaDeFaturamento> Faixas)
{
    public static async Task<FatoresDoTurno> Calcular(
        DateOnly data,
        IReadOnlyCollection<int> garcomIds,
        IReadOnlyCollection<Praca> pracas,
        IRepositorioIndicadores indicadores,
        IRepositorioAlocacao repositorio)
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

        return new FatoresDoTurno(garcons, pracasDoTurno, altoPotencial,
            RegraDeDistribuicao.FaixasDeFaturamento(garcomIds, faturamentoPorTurno));
    }
}
