using Gastra.Application.Auditoria;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Servicos;

namespace Gastra.Application.UseCases.Indicadores;

// UC16 — perfis de consumo encontrados pela clusterização (RF09, RF10)
public interface IRelatorioPerfisDeConsumoUseCase
{
    Task<PerfisDeConsumoResponse> Executar();
}

/// <summary>
/// O que a clusterização encontrou no histórico, com os nomes do cardápio no lugar dos ids. Dois casos não mostram
/// perfil nenhum, e a tela explica por quê: o serviço analítico fora do ar (D3) e o histórico ainda simulado — aí os
/// ids são os do simulador, e não do cardápio, e os perfis não descreveriam este restaurante.
/// </summary>
public class RelatorioPerfisDeConsumoUseCase(
    IServicoAnalitico servicoAnalitico,
    IRepositorioItemCardapio repositorioItem,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IRelatorioPerfisDeConsumoUseCase
{
    public async Task<PerfisDeConsumoResponse> Executar()
    {
        // Política de log, 4.5: quem consultou qual relatório. Os perfis olham o último ano, sem filtro de período.
        await auditoria.Registrar(EventoAuditoria.RelatorioBiConsultado, detalhes: new { Relatorio = "perfis" });
        await unitOfWork.Commit();

        PerfisDeConsumo perfis;
        try
        {
            perfis = await servicoAnalitico.ObterPerfisDeConsumo();
        }
        catch (ServicoAnaliticoIndisponivelException)
        {
            return new PerfisDeConsumoResponse { ServicoDisponivel = false };
        }

        if (!perfis.HistoricoReal)
            return new PerfisDeConsumoResponse { ServicoDisponivel = true, HistoricoSuficiente = false };

        var nomes = (await repositorioItem.ListarPorIds(perfis.Perfis.SelectMany(p => p.Itens).Select(i => i.ItemId)))
            .ToDictionary(i => i.Id, i => i.Nome);

        return new PerfisDeConsumoResponse
        {
            ServicoDisponivel = true,
            HistoricoSuficiente = true,
            Silhueta = perfis.Silhueta,
            ComandasAnalisadas = perfis.ComandasAnalisadas,
            SegmentaARecomendacao = perfis.SegmentaARecomendacao,
            Perfis = perfis.Perfis
                .OrderByDescending(p => p.Participacao)
                .Select(p => new PerfilDeConsumoResponse
                {
                    Comandas = p.Comandas,
                    Participacao = p.Participacao,
                    // Item que saiu do cardápio depois de vendido não tem mais nome para mostrar.
                    Itens = p.Itens
                        .Where(i => nomes.ContainsKey(i.ItemId))
                        .Select(i => new ItemDoPerfilResponse { Nome = nomes[i.ItemId], Presenca = i.Presenca, Destaque = i.Destaque })
                        .ToList(),
                })
                .ToList(),
        };
    }
}
