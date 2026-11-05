using Gastra.Application.UseCases.Promocoes;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;
using Mapster;

namespace Gastra.Application.UseCases.Cardapio;

public interface IObterItemCardapioUseCase
{
    Task<ItemCardapioResponse> Executar(int id);
}

public class ObterItemCardapioUseCase(IRepositorioItemCardapio repositorio, IRepositorioPromocao repositorioPromocao)
    : IObterItemCardapioUseCase
{
    public async Task<ItemCardapioResponse> Executar(int id)
    {
        var item = await repositorio.ObterPorId(id)
                   ?? throw new NaoEncontradoException(MensagensErro.ItemCardapioNaoEncontrado);

        var hoje = HojeNoRestaurante.Data();
        var resposta = item.Adapt<ItemCardapioResponse>();
        resposta.PrecoPromocional = Promocao.PrecoPromocional(item, await repositorioPromocao.ListarVigentes(hoje), hoje);
        return resposta;
    }
}
