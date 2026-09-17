using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
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

public class CadastrarItemCardapioUseCase(
    IRepositorioItemCardapio repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork)
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

        // Segundo Commit: o id do item só existe depois de gravado.
        await auditoria.Registrar(EventoAuditoria.ItemCardapioCadastrado, alvo: (nameof(ItemDoCardapio), item.Id),
            detalhes: new { item.Preco });
        await unitOfWork.Commit();

        return item.Adapt<ItemCardapioResponse>();
    }
}
