using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Salao;

// UC24 — Cadastrar mesa numa praça (RF23, REL01)
public interface ICadastrarMesaUseCase
{
    Task<MesaResponse> Executar(CadastrarMesaRequest request);
}

public class CadastrarMesaUseCase(
    IRepositorioMesa repositorio,
    IRepositorioPraca repositorioPraca,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : ICadastrarMesaUseCase
{
    public async Task<MesaResponse> Executar(CadastrarMesaRequest request)
    {
        ValidadorSalao.ValidarMesa(request.Numero, request.Capacidade);

        var praca = await repositorioPraca.ObterPorId(request.PracaId)
                    ?? throw new NaoEncontradoException(MensagensErro.PracaNaoEncontrada);

        if (await repositorio.ObterPorNumero(request.Numero) is not null)
            throw new RegraDeNegocioException(MensagensErro.NumeroMesaJaCadastrado);

        var mesa = new Mesa(request.Numero.Trim(), request.Capacidade, praca.Id);

        await repositorio.Adicionar(mesa);
        await unitOfWork.Commit();

        await auditoria.Registrar(EventoAuditoria.MesaCadastrada, alvo: (nameof(Mesa), mesa.Id),
            detalhes: new { mesa.Numero, mesa.Capacidade, mesa.PracaId });
        await unitOfWork.Commit();

        return new MesaResponse
        {
            Id = mesa.Id,
            Numero = mesa.Numero,
            Capacidade = mesa.Capacidade,
            PracaId = praca.Id,
            PracaCodigo = praca.Codigo,
        };
    }
}
