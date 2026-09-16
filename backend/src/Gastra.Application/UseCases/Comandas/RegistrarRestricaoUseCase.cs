using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;
using Mapster;

namespace Gastra.Application.UseCases.Comandas;

// UC13 — Registrar restrição alimentar (RF14)
public interface IRegistrarRestricaoUseCase
{
    Task<RestricaoAlimentarResponse> Executar(int comandaId, RestricaoAlimentarRequest request);
}

public class RegistrarRestricaoUseCase(IRepositorioComanda repositorio, IUnitOfWork unitOfWork)
    : IRegistrarRestricaoUseCase
{
    public async Task<RestricaoAlimentarResponse> Executar(int comandaId, RestricaoAlimentarRequest request)
    {
        ValidadorComanda.ValidarRestricao(request);

        var comanda = await BuscadorDeComanda.Aberta(repositorio, comandaId);

        var restricao = comanda.RegistrarRestricao(
            request.Categoria.Adapt<CategoriaRestricao>(),
            request.ObservacaoLivre);

        await unitOfWork.Commit();

        return new RestricaoAlimentarResponse
        {
            Id = restricao.Id,
            Categoria = request.Categoria,
            ObservacaoLivre = restricao.ObservacaoLivre,
        };
    }
}
