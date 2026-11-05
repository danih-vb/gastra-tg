using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Cardapio;

// UC06 — Atualizar preço de item (RF20)
public interface IAtualizarPrecoItemUseCase
{
    Task Executar(int id, AtualizarPrecoRequest request);
}

public class AtualizarPrecoItemUseCase(
    IRepositorioItemCardapio repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork)
    : IAtualizarPrecoItemUseCase
{
    public async Task Executar(int id, AtualizarPrecoRequest request)
    {
        ValidadorItemCardapio.ValidarPreco(request);

        var item = await repositorio.ObterPorId(id)
                   ?? throw new NaoEncontradoException(MensagensErro.ItemCardapioNaoEncontrado);

        var precoAnterior = item.Preco;
        item.AtualizarPreco(request.Preco);

        await auditoria.Registrar(EventoAuditoria.PrecoAlterado, alvo: (nameof(ItemDoCardapio), item.Id),
            detalhes: new { PrecoAnterior = precoAnterior, PrecoNovo = item.Preco });
        await unitOfWork.Commit();
    }
}
