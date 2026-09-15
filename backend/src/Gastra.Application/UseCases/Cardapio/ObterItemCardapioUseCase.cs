using Gastra.Communication.Responses;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;
using Mapster;

namespace Gastra.Application.UseCases.Cardapio;

public interface IObterItemCardapioUseCase
{
    Task<ItemCardapioResponse> Executar(int id);
}

public class ObterItemCardapioUseCase(IRepositorioItemCardapio repositorio) : IObterItemCardapioUseCase
{
    public async Task<ItemCardapioResponse> Executar(int id)
    {
        var item = await repositorio.ObterPorId(id)
                   ?? throw new NaoEncontradoException(MensagensErro.ItemCardapioNaoEncontrado);

        return item.Adapt<ItemCardapioResponse>();
    }
}
