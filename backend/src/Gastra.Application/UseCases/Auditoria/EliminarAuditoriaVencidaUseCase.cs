using Gastra.Application.Auditoria;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;

namespace Gastra.Application.UseCases.Auditoria;

// Política de log, seção 7, e LGPD art. 16: auditoria é eliminada ao fim do prazo de retenção
public interface IEliminarAuditoriaVencidaUseCase
{
    /// <returns>Quantos registros foram eliminados.</returns>
    Task<int> Executar(DateTime agoraUtc);
}

public class EliminarAuditoriaVencidaUseCase(
    IRepositorioAuditoria repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IEliminarAuditoriaVencidaUseCase
{
    public async Task<int> Executar(DateTime agoraUtc)
    {
        var limite = RegistroAuditoria.LimiteDeRetencao(agoraUtc);
        var quantidade = await repositorio.EliminarAnterioresA(limite);

        // A própria eliminação fica registrada: prova que o prazo foi cumprido, sem guardar o que foi apagado.
        if (quantidade > 0)
        {
            await auditoria.Registrar(EventoAuditoria.AuditoriaEliminadaPorPrazo,
                detalhes: new { Quantidade = quantidade, LimiteUtc = limite });
            await unitOfWork.Commit();
        }

        return quantidade;
    }
}
