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

    // --- RN09: bloqueio por tentativas ---

    private static readonly DateTime Agora = new(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);

    private static void Errar(Usuario usuario, int vezes, DateTime quando)
    {
        for (var i = 0; i < vezes; i++)
            usuario.RegistrarTentativaFalha(quando);
    }

    [Fact]
    public void TentativaFalha_QuatroSeguidas_AindaNaoBloqueia()
    {
        var usuario = CriarUsuario();

        Errar(usuario, Usuario.MaximoTentativasFalhas - 1, Agora);

        Assert.False(usuario.EstaBloqueada(Agora));
        Assert.Equal(4, usuario.TentativasFalhas);
    }

    [Fact]
    public void TentativaFalha_AQuinta_BloqueiaPorQuinzeMinutosEAvisaQueBloqueou()
    {
        var usuario = CriarUsuario();
        Errar(usuario, Usuario.MaximoTentativasFalhas - 1, Agora);

        var bloqueou = usuario.RegistrarTentativaFalha(Agora);

        Assert.True(bloqueou);
        Assert.True(usuario.EstaBloqueada(Agora.AddMinutes(14)));
        Assert.False(usuario.EstaBloqueada(Agora.AddMinutes(15)));
    }

    [Fact]
    public void TentativaFalha_DuranteOBloqueio_NaoEmpurraOFim()
    {
        var usuario = CriarUsuario();
        Errar(usuario, Usuario.MaximoTentativasFalhas, Agora);

        Errar(usuario, 20, Agora.AddMinutes(10));

        // Se cada palpite do atacante estendesse o prazo, o dono da conta nunca mais entraria.
        Assert.Equal(Agora.Add(Usuario.DuracaoBloqueio), usuario.BloqueadaAte);
        Assert.False(usuario.EstaBloqueada(Agora.AddMinutes(15)));
    }

    [Fact]
    public void TentativaFalha_DepoisDoBloqueio_ExigeOutrosCincoErros()
    {
        var usuario = CriarUsuario();
        Errar(usuario, Usuario.MaximoTentativasFalhas, Agora);
        var depois = Agora.AddMinutes(16);

        Errar(usuario, Usuario.MaximoTentativasFalhas - 1, depois);

        Assert.False(usuario.EstaBloqueada(depois));
    }

    [Fact]
    public void AcessoCompleto_ZeraAContagemELiberaAConta()
    {
        var usuario = CriarUsuario();
        Errar(usuario, Usuario.MaximoTentativasFalhas - 1, Agora);

        usuario.RegistrarAcessoCompleto();
        Errar(usuario, Usuario.MaximoTentativasFalhas - 1, Agora);

        Assert.False(usuario.EstaBloqueada(Agora));
    }

    [Fact]
    public void RedefinirSenha_DesfazOBloqueio()
    {
        var usuario = CriarUsuario();
        Errar(usuario, Usuario.MaximoTentativasFalhas, Agora);

        usuario.RedefinirSenha("hash-novo");

        Assert.False(usuario.EstaBloqueada(Agora));
        Assert.Equal(0, usuario.TentativasFalhas);
    }
}
