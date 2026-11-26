using Gastra.Domain.Entidades;

namespace Gastra.Domain.Tests.Entidades;

public class MencaoHonrosaTests
{
    private static MencaoHonrosa Criar(string titulo = "Melhor ideia", string? descricao = null, string icone = "estrela",
        string cor = "dourado") =>
        new(7, titulo, descricao, icone, cor, new DateOnly(2026, 11, 26), concedidaPorId: 1);

    [Fact]
    public void GuardaOMesPeloPrimeiroDia_ETiraEspacosDoTexto()
    {
        var mencao = Criar(titulo: "  Melhor ideia  ", descricao: "   ");

        Assert.Equal(new DateOnly(2026, 11, 1), mencao.Mes);
        Assert.Equal("Melhor ideia", mencao.Titulo);
        Assert.Null(mencao.Descricao);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TituloVazio_LancaExcecao(string titulo)
    {
        Assert.Throws<ArgumentException>(() => Criar(titulo: titulo));
    }

    [Fact]
    public void TextoLongoDemais_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => Criar(titulo: new string('a', 61)));
        Assert.Throws<ArgumentException>(() => Criar(descricao: new string('a', 201)));
    }

    [Fact]
    public void IconeOuCorForaDaLista_LancaExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Criar(icone: "foguete"));
        Assert.Throws<ArgumentOutOfRangeException>(() => Criar(cor: "rosa"));
    }
}
