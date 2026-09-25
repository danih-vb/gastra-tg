namespace Gastra.Domain.Enums;

/// <summary>
/// Onde o faturamento por turno de um garçom fica em relação aos colegas do mesmo turno. É o que o Metre vê para
/// entender a sugestão de alocação: a faixa, e nunca o valor, que é indicador de desempenho e só o Gerente consulta.
/// </summary>
public enum FaixaDeFaturamento
{
    /// <summary>Sem turno com comanda fechada na janela: para a RN03, faturamento zero.</summary>
    SemHistorico,
    AbaixoDaEquipe,
    NaMediaDaEquipe,
    AcimaDaEquipe,
}
