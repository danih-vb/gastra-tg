using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Cardapio;

// RF05 — definir ou tirar a foto do item mostrada no cardápio digital (#141).
public interface IAlterarImagemItemUseCase
{
    Task Executar(int id, ImagemItemRequest request);
}

public class AlterarImagemItemUseCase(
    IRepositorioItemCardapio repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork)
    : IAlterarImagemItemUseCase
{
    public async Task Executar(int id, ImagemItemRequest request)
    {
        ValidadorItemCardapio.ValidarImagem(request);

        var item = await repositorio.ObterPorId(id)
                   ?? throw new NaoEncontradoException(MensagensErro.ItemCardapioNaoEncontrado);

        item.DefinirImagem(request.Imagem);

        // O endereço da foto não é dado pessoal, mas registrar se o item ficou com ou sem foto ajuda a explicar
        // uma mudança no cardápio digital.
        await auditoria.Registrar(EventoAuditoria.ImagemItemAlterada, alvo: (nameof(ItemDoCardapio), item.Id),
            detalhes: new { ComImagem = item.Imagem is not null });
        await unitOfWork.Commit();
    }
}
