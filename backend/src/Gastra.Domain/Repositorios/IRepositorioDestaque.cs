using Gastra.Domain.Entidades;

namespace Gastra.Domain.Repositorios;

public interface IRepositorioDestaque
{
    Task Adicionar(DestaqueDoCardapio destaque);
    Task<DestaqueDoCardapio?> ObterPorId(int id);
    void Remover(DestaqueDoCardapio destaque);

    /// <summary>Destaques que ainda valem ou vão valer: os de hoje em diante.</summary>
    Task<List<DestaqueDoCardapio>> ListarAPartirDe(DateOnly data);

    /// <summary>Metas do mês cujo período cruza [inicio, fim).</summary>
    Task<List<DestaqueDoCardapio>> ListarMetasDoPeriodo(DateOnly inicio, DateOnly fim);

    /// <summary>
    /// Unidades do item vendidas pela equipe nas comandas abertas no período (horário de Brasília): sem cancelados, sem
    /// itens à espera do garçom e sem cortesias, que não são venda.
    /// </summary>
    Task<int> QuantidadeVendida(int itemDoCardapioId, DateOnly inicio, DateOnly fim);
}
