using Gastra.Application.UseCases.Promocoes;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Mapster;

namespace Gastra.Application.UseCases.Cardapio;

// UC19 (disponíveis) e gestão do cardápio (todos)
public interface IListarItensCardapioUseCase
{
    Task<List<ItemCardapioResponse>> Executar(bool somenteDisponiveis);
}

public class ListarItensCardapioUseCase(IRepositorioItemCardapio repositorio, IRepositorioPromocao repositorioPromocao)
    : IListarItensCardapioUseCase
{
    public async Task<List<ItemCardapioResponse>> Executar(bool somenteDisponiveis)
    {
        var itens = somenteDisponiveis
            ? await repositorio.ListarDisponiveis()
            : await repositorio.ListarTodos();

        // RF22: o cardápio (inclusive o digital, do cliente) mostra o preço das promoções que valem hoje.
        var hoje = HojeNoRestaurante.Data();
        var vigentes = await repositorioPromocao.ListarVigentes(hoje);

        return itens.Select(item =>
        {
            var resposta = item.Adapt<ItemCardapioResponse>();
            resposta.PrecoPromocional = Promocao.PrecoPromocional(item, vigentes, hoje);
            return resposta;
        }).ToList();
    }
}
