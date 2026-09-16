using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Salao;

// UC24 — Editar praça (RF23)
public interface IEditarPracaUseCase
{
    Task Executar(int id, PracaRequest request);
}

public class EditarPracaUseCase(IRepositorioPraca repositorio, IUnitOfWork unitOfWork) : IEditarPracaUseCase
{
    public async Task Executar(int id, PracaRequest request)
    {
        ValidadorSalao.ValidarPraca(request);

        var praca = await repositorio.ObterPorId(id)
                    ?? throw new NaoEncontradoException(MensagensErro.PracaNaoEncontrada);

        var dono = await repositorio.ObterPorCodigo(request.Codigo);
        if (dono is not null && dono.Id != praca.Id)
            throw new RegraDeNegocioException(MensagensErro.CodigoPracaJaCadastrado);

        praca.Atualizar(request.Codigo.Trim(), request.QuantidadeGarcons);
        await unitOfWork.Commit();
    }
}
