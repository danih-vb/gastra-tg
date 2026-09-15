using Gastra.Communication.Requests;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Cardapio;

// UC06 — Atualizar preço de item (RF20)
public interface IAtualizarPrecoItemUseCase
{
    Task Executar(int id, AtualizarPrecoRequest request);
}

public class AtualizarPrecoItemUseCase(IRepositorioItemCardapio repositorio, IUnitOfWork unitOfWork)
    : IAtualizarPrecoItemUseCase
{
    public async Task Executar(int id, AtualizarPrecoRequest request)
    {
        ValidadorItemCardapio.ValidarPreco(request);

        var item = await repositorio.ObterPorId(id)
                   ?? throw new NaoEncontradoException(MensagensErro.ItemCardapioNaoEncontrado);

        item.AtualizarPreco(request.Preco);
        await unitOfWork.Commit();
    }
}
