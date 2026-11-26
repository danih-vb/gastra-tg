using Gastra.Application.Auditoria;
using Gastra.Application.UseCases.Promocoes;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Enums;
using Gastra.Domain.Indicadores;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Indicadores;

/// <summary>
/// Medalhas do mês e histórico de conquistas de um garçom. O garçom vê as próprias; o Gerente vê as de qualquer
/// garçom, no perfil dele. Como o índice, é avaliação de desempenho: dado pessoal, e cada consulta vai para o log.
/// </summary>
public interface IMedalhasUseCase
{
    Task<MedalhasResponse> Executar(int? garcomId);
}

public class MedalhasUseCase(
    IRepositorioIndicadores indicadores,
    IRepositorioDestaque destaques,
    IRepositorioUsuario repositorioUsuario,
    IUsuarioLogado usuarioLogado,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IMedalhasUseCase
{
    public const int MesesNoHistorico = 6;

    public async Task<MedalhasResponse> Executar(int? garcomId)
    {
        var quem = usuarioLogado.ObterIdentificacao()
                   ?? throw new InvalidOperationException("Consulta de medalhas sem usuário autenticado.");

        // O garçom só vê as próprias medalhas, mesmo que peça as de outro.
        var alvoId = quem.Papel == PapelUsuario.Garcom ? quem.Id : garcomId ?? quem.Id;
        var garcom = await repositorioUsuario.ObterPorId(alvoId);
        if (garcom is null || garcom.Papel != PapelUsuario.Garcom)
            throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        var hoje = HojeNoRestaurante.Data();
        var mesAtual = new DateOnly(hoje.Year, hoje.Month, 1);

        var resposta = new MedalhasResponse { GarcomId = garcom.Id, Nome = garcom.Nome, Mes = mesAtual };
        for (var i = 0; i < MesesNoHistorico; i++)
        {
            var inicio = mesAtual.AddMonths(-i);
            var medalhas = Medalhas.Calcular(await DesempenhoNoMes(garcom.Id, inicio, inicio.AddMonths(1)));
            if (i == 0)
                resposta.Medalhas = medalhas.Select(m => new MedalhaResponse
                {
                    Codigo = m.Codigo, Conquistada = m.Conquistada, Progresso = m.Progresso, Alvo = m.Alvo,
                }).ToList();
            resposta.Historico.Add(new ConquistasDoMesResponse
            {
                Mes = inicio,
                Conquistadas = medalhas.Where(m => m.Conquistada).Select(m => m.Codigo).ToList(),
            });
        }

        await auditoria.Registrar(EventoAuditoria.MedalhasConsultadas, detalhes: new { GarcomConsultado = garcom.Id });
        await unitOfWork.Commit();

        return resposta;
    }

    private async Task<DesempenhoDoMes> DesempenhoNoMes(int garcomId, DateOnly inicio, DateOnly fim)
    {
        var linhas = await indicadores.ObterIndicadoresPorGarcom(inicio, fim);
        var avaliacoes = (await indicadores.ObterAvaliacoesPorGarcom(inicio, fim)).ToDictionary(a => a.GarcomId);
        var ranking = IndiceDeDesempenho.Calcular(linhas.Select(l => new DesempenhoNoPeriodo(
            l.GarcomId, l.Faturamento, l.Turnos, l.MesasAtendidas,
            avaliacoes.GetValueOrDefault(l.GarcomId)?.Quantidade ?? 0,
            avaliacoes.GetValueOrDefault(l.GarcomId)?.SomaDasNotas ?? 0)));

        var minha = linhas.FirstOrDefault(l => l.GarcomId == garcomId);
        var unidades = await indicadores.ObterUnidadesDeMetaPorGarcom(inicio, fim);

        var equipeBateuMeta = false;
        foreach (var meta in await destaques.ListarMetasDoPeriodo(inicio, fim))
            equipeBateuMeta |= await destaques.QuantidadeVendida(meta.ItemDoCardapioId, meta.DataInicio, meta.DataFim) >= meta.Meta;

        return new DesempenhoDoMes(
            ranking.Posicoes.FirstOrDefault(p => p.GarcomId == garcomId)?.Posicao,
            minha?.MesasAtendidas ?? 0,
            minha?.Turnos ?? 0,
            avaliacoes.GetValueOrDefault(garcomId)?.Quantidade ?? 0,
            avaliacoes.GetValueOrDefault(garcomId)?.SomaDasNotas ?? 0,
            unidades.GetValueOrDefault(garcomId),
            equipeBateuMeta);
    }
}
