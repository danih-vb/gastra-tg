using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;
using Mapster;

namespace Gastra.Application.UseCases.Comandas;

// UC11 — Confirmar ou ajustar a composição da mesa (RF02, RN01)
public interface IConfirmarComposicaoUseCase
{
    Task Executar(int comandaId, ComposicaoRequest request);
}

public class ConfirmarComposicaoUseCase(
    IRepositorioComanda repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork)
    : IConfirmarComposicaoUseCase
{
    public async Task Executar(int comandaId, ComposicaoRequest request)
    {
        ValidadorComanda.ValidarComposicao(request);

        var comanda = await BuscadorDeComanda.Aberta(repositorio, comandaId);

        var sugerida = comanda.SugerirComposicao(request.QuantidadePessoas);
        comanda.ConfirmarComposicao(request.QuantidadePessoas, request.Composicao.Adapt<ComposicaoMesa>());

        await auditoria.Registrar(EventoAuditoria.ComposicaoAjustada, alvo: (nameof(Comanda), comanda.Id),
            detalhes: new { comanda.QuantidadePessoas, ComposicaoSugerida = sugerida, ComposicaoConfirmada = comanda.Composicao });
        await unitOfWork.Commit();
    }
}
