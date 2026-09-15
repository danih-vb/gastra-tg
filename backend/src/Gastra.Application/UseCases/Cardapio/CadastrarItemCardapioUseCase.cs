using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Mapster;
using DominioEnums = Gastra.Domain.Enums;

namespace Gastra.Application.UseCases.Cardapio;

// UC05 — Cadastrar item do cardápio (RF19)
public interface ICadastrarItemCardapioUseCase
{
    Task<ItemCardapioResponse> Executar(ItemCardapioRequest request);
}

public class CadastrarItemCardapioUseCase(IRepositorioItemCardapio repositorio, IUnitOfWork unitOfWork)
    : ICadastrarItemCardapioUseCase
{
    public async Task<ItemCardapioResponse> Executar(ItemCardapioRequest request)
    {
        ValidadorItemCardapio.Validar(request);

        var item = new ItemDoCardapio(
            request.Nome.Trim(),
            request.Categoria.Adapt<DominioEnums.CategoriaItemCardapio>(),
            request.Preco,
            request.Descricao?.Trim() ?? string.Empty,
            request.FlagsDieteticas.Adapt<List<DominioEnums.FlagDietetica>>());

        await repositorio.Adicionar(item);
        await unitOfWork.Commit();

        return item.Adapt<ItemCardapioResponse>();
    }
}
