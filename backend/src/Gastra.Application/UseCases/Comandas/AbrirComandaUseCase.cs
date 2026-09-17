using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

// UC10 — Abrir comanda (RF01, RF02)
public interface IAbrirComandaUseCase
{
    Task<ComandaResponse> Executar(AbrirComandaRequest request);
}

public class AbrirComandaUseCase(
    IRepositorioComanda repositorio,
    IRepositorioMesa repositorioMesa,
    IUsuarioLogado usuarioLogado,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IAbrirComandaUseCase
{
    public async Task<ComandaResponse> Executar(AbrirComandaRequest request)
    {
        ValidadorComanda.ValidarAbertura(request);

        _ = await repositorioMesa.ObterPorId(request.MesaId)
            ?? throw new NaoEncontradoException(MensagensErro.MesaNaoEncontrada);

        var garcom = await usuarioLogado.Obter();

        // A mesma mesa pode ter mais de uma comanda aberta ao mesmo tempo (REL02 do MER).
        var comanda = new Comanda(request.MesaId, garcom.Id, request.QuantidadePessoas);

        await repositorio.Adicionar(comanda);
        await unitOfWork.Commit();

        // O código de acesso do cliente nunca vai para a auditoria (política de log, 4.3).
        await auditoria.Registrar(EventoAuditoria.ComandaAberta, alvo: (nameof(Comanda), comanda.Id),
            detalhes: new { comanda.MesaId, comanda.QuantidadePessoas });
        await unitOfWork.Commit();

        return MapeadorComanda.Montar(comanda, new Dictionary<int, string>(), garcom.Papel, garcom.Nome);
    }
}
