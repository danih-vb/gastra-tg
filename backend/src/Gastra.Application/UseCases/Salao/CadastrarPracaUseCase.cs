using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Salao;

// UC24 — Cadastrar praça (RF23)
public interface ICadastrarPracaUseCase
{
    Task<PracaResponse> Executar(PracaRequest request);
}

public class CadastrarPracaUseCase(IRepositorioPraca repositorio, IUnitOfWork unitOfWork) : ICadastrarPracaUseCase
{
    public async Task<PracaResponse> Executar(PracaRequest request)
    {
        ValidadorSalao.ValidarPraca(request);

        if (await repositorio.ObterPorCodigo(request.Codigo) is not null)
            throw new RegraDeNegocioException(MensagensErro.CodigoPracaJaCadastrado);

        var praca = new Praca(request.Codigo.Trim(), request.QuantidadeGarcons);

        await repositorio.Adicionar(praca);
        await unitOfWork.Commit();

        return new PracaResponse
        {
            Id = praca.Id,
            Codigo = praca.Codigo,
            QuantidadeGarcons = praca.QuantidadeGarcons,
        };
    }
}
