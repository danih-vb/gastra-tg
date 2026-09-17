namespace Gastra.Domain.Servicos;

/// <summary>
/// Cálculos feitos pela camada analítica em Python. O domínio só conhece o contrato; a chamada HTTP
/// fica na infraestrutura (decisão D2: o Python calcula, o backend decide e grava).
/// </summary>
public interface IServicoAnalitico
{
    /// <summary>
    /// RF09 — itens que costumam acompanhar os já pedidos, do mais para o menos recomendado.
    /// </summary>
    /// <param name="itensPedidos">Ids do cardápio já lançados na comanda.</param>
    /// <param name="itensPermitidos">Únicos ids que podem voltar como sugestão.</param>
    /// <param name="limite">Quantidade máxima de sugestões.</param>
    /// <exception cref="ServicoAnaliticoIndisponivelException">Serviço fora do ar, lento ou com resposta inválida.</exception>
    Task<IReadOnlyList<int>> SugerirCombinacoes(
        IReadOnlyCollection<int> itensPedidos,
        IReadOnlyCollection<int> itensPermitidos,
        int limite,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// RF06 e RN03 — distribui os garçons do turno entre as praças por programação linear.
    /// </summary>
    /// <exception cref="ServicoAnaliticoIndisponivelException">Serviço fora do ar, lento ou com resposta inválida.</exception>
    Task<IReadOnlyList<DesignacaoSugerida>> SugerirAlocacao(
        IReadOnlyCollection<GarcomParaAlocacao> garcons,
        IReadOnlyCollection<PracaParaAlocacao> pracas,
        double pesoDesequilibrio,
        double pesoEspera,
        CancellationToken cancellationToken = default);
}
