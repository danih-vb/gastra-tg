using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;

namespace Gastra.Domain.Tests.Entidades;

public class UsuarioTests
{
    private static Usuario CriarUsuario(PapelUsuario papel = PapelUsuario.Garcom) =>
        new("Ana Souza", "  Ana.Souza@Gastra.Local ", "hash", papel);

    [Fact]
    public void Criar_NormalizaEmailEComecaAtivo()
    {
        var usuario = CriarUsuario();

        Assert.Equal("ana.souza@gastra.local", usuario.Email);
        Assert.True(usuario.Ativo);
    }

    [Fact]
    public void AtualizarDados_TrocandoPapel_EncerraAsSessoes()
    {
        var usuario = CriarUsuario(PapelUsuario.Garcom);
        var chaveAnterior = usuario.ChaveSessao;

        usuario.AtualizarDados("Ana Souza", "ana.souza@gastra.local", PapelUsuario.Metre);

        Assert.Equal(PapelUsuario.Metre, usuario.Papel);
        Assert.NotEqual(chaveAnterior, usuario.ChaveSessao);
    }

    [Fact]
    public void AtualizarDados_MantendoPapel_NaoEncerraAsSessoes()
    {
        var usuario = CriarUsuario(PapelUsuario.Garcom);
        var chaveAnterior = usuario.ChaveSessao;

        usuario.AtualizarDados("Ana Paula Souza", "ANA.PAULA@gastra.local", PapelUsuario.Garcom);

        Assert.Equal("Ana Paula Souza", usuario.Nome);
        Assert.Equal("ana.paula@gastra.local", usuario.Email);
        Assert.Equal(chaveAnterior, usuario.ChaveSessao);
    }

    [Fact]
    public void Inativar_EncerraAsSessoes_EReativarDevolveOAcesso()
    {
        var usuario = CriarUsuario();
        var chaveAnterior = usuario.ChaveSessao;

        usuario.Inativar();
        Assert.False(usuario.Ativo);
        Assert.NotEqual(chaveAnterior, usuario.ChaveSessao);

        usuario.Reativar();
        Assert.True(usuario.Ativo);
    }
}
