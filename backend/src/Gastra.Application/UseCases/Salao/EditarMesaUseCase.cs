using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Salao;

// UC24 — Editar mesa (RF23). A praça não muda: o vínculo é fixo (REL01).
public interface IEditarMesaUseCase
{
    Task Executar(int id, EditarMesaRequest request);
}

public class EditarMesaUseCase(IRepositorioMesa repositorio, IUnitOfWork unitOfWork) : IEditarMesaUseCase
{
    public async Task Executar(int id, EditarMesaRequest request)
    {
        ValidadorSalao.ValidarMesa(request.Numero, request.Capacidade);

        var mesa = await repositorio.ObterPorId(id)
                   ?? throw new NaoEncontradoException(MensagensErro.MesaNaoEncontrada);

        var dona = await repositorio.ObterPorNumero(request.Numero);
        if (dona is not null && dona.Id != mesa.Id)
            throw new RegraDeNegocioException(MensagensErro.NumeroMesaJaCadastrado);

        mesa.Atualizar(request.Numero.Trim(), request.Capacidade);
        await unitOfWork.Commit();
    }
}
