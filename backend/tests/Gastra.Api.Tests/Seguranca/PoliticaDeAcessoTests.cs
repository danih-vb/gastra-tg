using Gastra.Api.Tests.Infraestrutura;

namespace Gastra.Api.Tests.Seguranca;

/// <summary>
/// Nenhum endpoint fica aberto por esquecimento (#159). Sem atributo, o ASP.NET Core deixa a rota pública; por isso
/// todo endpoint precisa dizer quem pode chamá-lo: <c>[Authorize(Roles = ...)]</c>, ou <c>[AllowAnonymous]</c> para os
/// públicos de propósito.
/// </summary>
public class PoliticaDeAcessoTests
{
    /// <summary>O que o cliente usa sem login, e o próprio login, que ainda não tem token.</summary>
    private static readonly string[] PublicosEsperados =
    [
        "POST /api/autenticacao/login",
        "POST /api/autenticacao/segundo-fator/configurar",
        "POST /api/autenticacao/segundo-fator/confirmar",
        "GET /api/cardapio/digital",
        "GET /api/cardapio/{id:int}",
        "GET /api/cardapio/fotos/{nome}",
        "GET /api/comandas/consulta/{codigoAcesso}",
        "POST /api/comandas/consulta/{codigoAcesso}/chamado",
        "POST /api/comandas/consulta/{codigoAcesso}/avaliacao",
    ];

    /// <summary>Exigem login, mas servem a qualquer papel: sair do sistema e ver a foto do crachá.</summary>
    private static readonly string[] ParaQualquerUsuarioLogado =
    [
        "POST /api/autenticacao/logoff",
        "GET /api/usuarios/fotos/{nome}",
    ];

    [Fact]
    public void TodoEndpoint_DeclaraQuemPodeChamar()
    {
        Assert.NotEmpty(MapaDeEndpoints.Todos);
        Assert.Empty(MapaDeEndpoints.SemPolitica.Select(e => e.ToString()));
    }

    [Fact]
    public void Publicos_SaoExatamenteOsEsperados()
    {
        var publicos = MapaDeEndpoints.Todos.Where(e => e.Publico).Select(e => e.ToString()).Order();

        Assert.Equal(PublicosEsperados.Order(), publicos);
    }

    [Fact]
    public void SemPapelDefinido_SoOsQueServemAQualquerUsuarioLogado()
    {
        var semPapel = MapaDeEndpoints.Todos.Where(e => e.QualquerUsuarioLogado).Select(e => e.ToString()).Order();

        Assert.Equal(ParaQualquerUsuarioLogado.Order(), semPapel);
    }

    [Fact]
    public void Papeis_SaoOsQueExistem()
    {
        var desconhecidos = MapaDeEndpoints.Todos
            .Where(e => e.Papeis is not null)
            .SelectMany(e => e.Papeis!.Except(MapaDeEndpoints.TodosOsPapeis).Select(p => $"{e}: {p}"));

        Assert.Empty(desconhecidos);
    }
}
