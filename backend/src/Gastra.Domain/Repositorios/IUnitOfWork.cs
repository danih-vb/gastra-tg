namespace Gastra.Domain.Repositorios;

public interface IUnitOfWork
{
    /// <summary>Grava no banco, de uma vez, todas as alterações feitas pelos repositórios.</summary>
    Task Commit();
}
