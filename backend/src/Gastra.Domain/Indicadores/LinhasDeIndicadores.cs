namespace Gastra.Domain.Indicadores;

// Linhas lidas das views analíticas para os relatórios de BI (RF10). Os valores são calculados pelo banco a
// cada consulta e nunca gravados.

/// <summary>Um garçom no período. Soma de minutos, e não média, para permitir a média geral ponderada.</summary>
public record IndicadorGarcom(int GarcomId, decimal Faturamento, int Comandas, int Turnos, int MesasAtendidas, int SomaMinutosAtendimento);

/// <summary>Uma praça no período.</summary>
public record IndicadorPraca(int PracaId, decimal Faturamento, int Comandas, int Turnos);

/// <summary>Um item do cardápio no período.</summary>
public record IndicadorItemCardapio(int ItemCardapioId, string Categoria, decimal Quantidade, decimal Faturamento);

/// <summary>Faturamento de uma praça numa fatia do tempo: hora do dia (0–23) ou dia da semana (1 = domingo).</summary>
public record IndicadorPracaNoTempo(int PracaId, int Fatia, decimal Faturamento, int Comandas);

/// <summary>
/// Uma faixa da distribuição de notas das avaliações (RF25). Agregado: nenhuma linha identifica quem
/// avaliou nem o garçom atendido.
/// </summary>
public record LinhaAvaliacao(int Nota, int Quantidade);

/// <summary>
/// Quantas avaliações as comandas de um garçom receberam no período, e a soma das notas. Só alimenta o índice de
/// desempenho (RF11), que aplica o mínimo de avaliações da RN08 antes de usar ou mostrar qualquer nota.
/// </summary>
public record AvaliacoesDoGarcom(int GarcomId, int Quantidade, int SomaDasNotas);
