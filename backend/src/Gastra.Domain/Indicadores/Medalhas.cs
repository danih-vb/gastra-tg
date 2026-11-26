namespace Gastra.Domain.Indicadores;

/// <summary>O que um garçom fez no mês, lido das views e das avaliações: a base das medalhas.</summary>
public record DesempenhoDoMes(
    int? Posicao, int Mesas, int Turnos, int Avaliacoes, int SomaDasNotas, int UnidadesDeMeta, bool EquipeBateuMeta);

/// <summary>Uma medalha do mês: conquistada ou não, e quanto falta (progresso de 0 até o alvo).</summary>
public record Medalha(string Codigo, bool Conquistada, decimal Progresso, decimal Alvo);

/// <summary>
/// Medalhas do garçom, calculadas a cada consulta a partir do que o sistema já registra: nada é gravado sobre o
/// funcionário além do que o índice de desempenho já usa. Premiam a evolução de cada um e a equipe, e não só o
/// primeiro lugar, para não contrariar a RN03, que existe para reduzir a disputa pelas melhores praças.
/// </summary>
public static class Medalhas
{
    public const int PosicaoParaTop = 3;
    public const int MesasParaMesaCheia = 100;
    public const int TurnosParaMaratonista = 20;
    public const int AvaliacoesParaBemAvaliado = IndiceDeDesempenho.MinimoDeAvaliacoes;
    public const decimal NotaParaBemAvaliado = 4.5m;
    public const int UnidadesParaVendedorDeDestaque = 20;

    public static IReadOnlyList<Medalha> Calcular(DesempenhoDoMes mes)
    {
        var media = mes.Avaliacoes > 0 ? (decimal)mes.SomaDasNotas / mes.Avaliacoes : 0;

        return
        [
            new("top-3", mes.Posicao is >= 1 and <= PosicaoParaTop, mes.Posicao ?? 0, PosicaoParaTop),
            new("mesa-cheia", mes.Mesas >= MesasParaMesaCheia, Math.Min(mes.Mesas, MesasParaMesaCheia), MesasParaMesaCheia),
            new("maratonista", mes.Turnos >= TurnosParaMaratonista, Math.Min(mes.Turnos, TurnosParaMaratonista), TurnosParaMaratonista),
            new("bem-avaliado", mes.Avaliacoes >= AvaliacoesParaBemAvaliado && media >= NotaParaBemAvaliado,
                Math.Round(media, 2), NotaParaBemAvaliado),
            new("vendedor-de-destaque", mes.UnidadesDeMeta >= UnidadesParaVendedorDeDestaque,
                Math.Min(mes.UnidadesDeMeta, UnidadesParaVendedorDeDestaque), UnidadesParaVendedorDeDestaque),
            new("meta-batida", mes.EquipeBateuMeta, mes.EquipeBateuMeta ? 1 : 0, 1),
        ];
    }
}
