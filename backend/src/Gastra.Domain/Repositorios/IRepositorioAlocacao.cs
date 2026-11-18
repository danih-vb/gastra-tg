using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;

namespace Gastra.Domain.Repositorios;

public interface IRepositorioAlocacao
{
    Task Adicionar(Alocacao alocacao);

    /// <summary>Alocações de um turno, para editar (com rastreamento).</summary>
    Task<List<Alocacao>> ListarDoTurno(DateOnly data, PeriodoAlocacao periodo);

    void Remover(IEnumerable<Alocacao> alocacoes);

    /// <summary>
    /// Praças dos turnos confirmados de cada garçom anteriores à data, do mais recente para o mais antigo (RN03).
    /// </summary>
    Task<Dictionary<int, List<int>>> ListarPracasConfirmadasAntesDe(IEnumerable<int> garcomIds, DateOnly data);
}
