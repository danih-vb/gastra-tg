using Gastra.Domain.Enums;

namespace Gastra.Domain.Servicos;

/// <summary>
/// Engenharia de cardápio (KASAVANA; SMITH, 1982), adaptada: o método original compara a margem de cada prato, e o
/// sistema não guarda custo. Aqui o eixo do valor é o quanto cada unidade rende, ou seja, o preço médio de venda.
/// A comparação é dentro da categoria: uma bebida nunca teria valor alto contra um prato principal.
/// </summary>
public static class EngenhariaDeCardapio
{
    /// <summary>Popular é vender ao menos 70% da fatia que caberia a cada item se todos vendessem igual.</summary>
    public const decimal FatorDePopularidade = 0.7m;

    public static ClassificacaoDoItem Classificar(
        decimal quantidade,
        decimal faturamento,
        int itensNaCategoria,
        decimal quantidadeDaCategoria,
        decimal faturamentoDaCategoria)
    {
        if (quantidade <= 0 || itensNaCategoria <= 0 || quantidadeDaCategoria <= 0)
            return ClassificacaoDoItem.Cao;

        var popular = quantidade / quantidadeDaCategoria >= FatorDePopularidade / itensNaCategoria;
        var valorAlto = faturamento / quantidade >= faturamentoDaCategoria / quantidadeDaCategoria;

        return (popular, valorAlto) switch
        {
            (true, true) => ClassificacaoDoItem.Estrela,
            (true, false) => ClassificacaoDoItem.BurroDeCarga,
            (false, true) => ClassificacaoDoItem.QuebraCabeca,
            (false, false) => ClassificacaoDoItem.Cao,
        };
    }
}
