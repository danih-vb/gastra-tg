using Gastra.Communication.Responses;
using Gastra.Domain.Repositorios;
using Mapster;

namespace Gastra.Application.UseCases.Cardapio;

// UC19 (disponíveis) e gestão do cardápio (todos)
public interface IListarItensCardapioUseCase
{
    Task<List<ItemCardapioResponse>> Executar(bool somenteDisponiveis);
}

public class ListarItensCardapioUseCase(IRepositorioItemCardapio repositorio) : IListarItensCardapioUseCase
{
    public async Task<List<ItemCardapioResponse>> Executar(bool somenteDisponiveis)
    {
        var itens = somenteDisponiveis
            ? await repositorio.ListarDisponiveis()
            : await repositorio.ListarTodos();

        return itens.Adapt<List<ItemCardapioResponse>>();
    }
}
