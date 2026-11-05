using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;

namespace Gastra.Domain.Tests.Entidades;

public class AlocacaoTests
{
    private static Alocacao CriarAlocacao() =>
        new(new DateOnly(2026, 9, 16), PeriodoAlocacao.Jantar, garcomId: 3, pracaId: 1);

    [Fact]
    public void Criar_ComecaNaoConfirmada()
    {
        Assert.False(CriarAlocacao().Confirmada);
    }

    [Fact]
    public void Ajustar_AntesDeConfirmar_TrocaAPraca()
    {
        var alocacao = CriarAlocacao();

        alocacao.Ajustar(novaPracaId: 4);

        Assert.Equal(4, alocacao.PracaId);
    }

    [Fact]
    public void Ajustar_DepoisDeConfirmar_LancaExcecao()
    {
        var alocacao = CriarAlocacao();
        alocacao.Confirmar();

        Assert.Throws<InvalidOperationException>(() => alocacao.Ajustar(4));
    }

    [Fact]
    public void Confirmar_DuasVezes_LancaExcecao()
    {
        var alocacao = CriarAlocacao();
        alocacao.Confirmar();

        Assert.Throws<InvalidOperationException>(alocacao.Confirmar);
    }
}
