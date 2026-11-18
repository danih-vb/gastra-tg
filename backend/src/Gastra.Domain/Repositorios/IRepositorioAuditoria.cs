using Gastra.Domain.Entidades;

namespace Gastra.Domain.Repositorios;

public interface IRepositorioAuditoria
{
    /// <summary>Entra no mesmo Commit da operação auditada.</summary>
    Task Adicionar(RegistroAuditoria registro);

    /// <summary>Apaga, direto no banco, os registros anteriores ao limite. Devolve quantos apagou.</summary>
    Task<int> EliminarAnterioresA(DateTime limiteUtc);
}
