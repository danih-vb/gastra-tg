using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Cardapio;

// UC07 — Marcar item disponível/indisponível (RF21)
public interface IAlterarDisponibilidadeItemUseCase
{
    Task Executar(int id, DisponibilidadeRequest request);
}

public class AlterarDisponibilidadeItemUseCase(
    IRepositorioItemCardapio repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork)
    : IAlterarDisponibilidadeItemUseCase
{
    public async Task Executar(int id, DisponibilidadeRequest request)
    {
        var item = await repositorio.ObterPorId(id)
                   ?? throw new NaoEncontradoException(MensagensErro.ItemCardapioNaoEncontrado);

        item.MarcarDisponibilidade(request.Disponivel);

        await auditoria.Registrar(EventoAuditoria.DisponibilidadeAlterada, alvo: (nameof(ItemDoCardapio), item.Id),
            detalhes: new { item.Disponivel });
        await unitOfWork.Commit();
    }
}
