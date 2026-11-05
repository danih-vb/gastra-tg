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

// UC14 — Fechar comanda (RF04, RN02)
public interface IFecharComandaUseCase
{
    Task<ComandaResponse> Executar(int comandaId);
}

public class FecharComandaUseCase(
    IRepositorioComanda repositorio,
    IRepositorioItemCardapio repositorioCardapio,
    IRepositorioUsuario repositorioUsuario,
    IUsuarioLogado usuarioLogado,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IFecharComandaUseCase
{
    public async Task<ComandaResponse> Executar(int comandaId)
    {
        var comanda = await BuscadorDeComanda.Aberta(repositorio, comandaId);

        if (comanda.PossuiPendencias())
            throw new RegraDeNegocioException(MensagensErro.ComandaComItensPendentes);

        // Fechar também apaga a observação livre das restrições (LGPD).
        comanda.Fechar();

        await auditoria.Registrar(EventoAuditoria.ComandaFechada, alvo: (nameof(Comanda), comanda.Id),
            detalhes: new { Total = comanda.CalcularTotal() });
        await unitOfWork.Commit();

        var nomes = await LeitorDeNomesDoCardapio.Obter(repositorioCardapio, comanda);
        var garcom = await LeitorDeNomesDeGarcons.Obter(repositorioUsuario, comanda);
        return MapeadorComanda.Montar(comanda, nomes, usuarioLogado.ObterPapel(), garcom);
    }
}
