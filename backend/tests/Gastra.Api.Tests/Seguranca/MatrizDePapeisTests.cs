using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Communication.Responses;
using Gastra.Domain.Enums;

namespace Gastra.Api.Tests.Seguranca;

/// <summary>
/// A matriz papel × endpoint conferida em execução (#160): cada endpoint é chamado de verdade sem token e com cada um
/// dos quatro papéis. Sem token, 401; papel fora da lista, 403; papel da lista, qualquer coisa menos 401 e 403 (o
/// recurso de exemplo não existe, então o normal é 404 ou 400). Os papéis esperados vêm dos atributos, os mesmos que
/// o <see cref="PoliticaDeAcessoTests"/> confere.
/// </summary>
public class MatrizDePapeisTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>
{
    /// <summary>Sair do sistema derruba a sessão do próprio token: tem teste próprio em AutenticacaoControllerTests.</summary>
    private static readonly string[] ForaDaMatriz = ["POST /api/autenticacao/logoff"];

    /// <summary>Públicos que respondem 401 a um token de segundo fator inválido: é a regra deles, não o login do JWT.</summary>
    private static readonly string[] PublicosQueValidamOProprioToken =
    [
        "POST /api/autenticacao/segundo-fator/configurar",
        "POST /api/autenticacao/segundo-fator/confirmar",
    ];

    [Fact]
    public async Task CadaEndpoint_RespondeAoPapelComoOsAtributosDizem()
    {
        var cliente = factory.CreateClient();
        var tokens = new Dictionary<string, string>
        {
            ["Gerente"] = await factory.TokenGerente(cliente),
            ["Coordenador"] = await TokenComSegundoFator(cliente, PapelUsuario.Coordenador),
            ["Metre"] = await TokenSemSegundoFator(cliente, PapelUsuario.Metre),
            ["Garcom"] = await factory.TokenGarcom(cliente),
        };

        var falhas = new List<string>();
        foreach (var rota in MapaDeEndpoints.Todos.Where(r => !ForaDaMatriz.Contains(r.ToString())))
        {
            var semToken = await Chamar(cliente, rota, token: null);
            if (rota.Publico)
            {
                if (semToken is HttpStatusCode.Forbidden
                    || (semToken is HttpStatusCode.Unauthorized && !PublicosQueValidamOProprioToken.Contains(rota.ToString())))
                    falhas.Add($"{rota} sem token: {(int)semToken}, mas é público");
                continue;
            }

            if (semToken != HttpStatusCode.Unauthorized)
                falhas.Add($"{rota} sem token: {(int)semToken}, esperado 401");

            foreach (var (papel, token) in tokens)
            {
                var status = await Chamar(cliente, rota, token);
                var pode = rota.Papeis is null || rota.Papeis.Contains(papel);
                if (pode && status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    falhas.Add($"{rota} como {papel}: {(int)status}, mas o papel pode");
                else if (!pode && status != HttpStatusCode.Forbidden)
                    falhas.Add($"{rota} como {papel}: {(int)status}, esperado 403");
            }
        }

        Assert.True(falhas.Count == 0, string.Join(Environment.NewLine, falhas));
    }

    private static async Task<HttpStatusCode> Chamar(HttpClient cliente, RotaDaApi rota, string? token)
    {
        using var pedido = new HttpRequestMessage(new HttpMethod(rota.Metodo), rota.RotaDeExemplo);
        if (token is not null)
            pedido.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        // Envio de arquivo só casa com a rota se vier como formulário; senão o ASP.NET responde 415 antes do login.
        if (rota.RecebeArquivo)
            pedido.Content = new MultipartFormDataContent();
        else if (rota.Metodo is "POST" or "PUT" or "PATCH")
            pedido.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        using var resposta = await cliente.SendAsync(pedido);
        return resposta.StatusCode;
    }

    private async Task<string> TokenSemSegundoFator(HttpClient cliente, PapelUsuario papel)
    {
        var email = $"{papel}-{Guid.NewGuid():N}@gastra.test".ToLowerInvariant();
        await factory.CriarUsuario(email, papel);
        return await factory.Login(cliente, email);
    }

    /// <summary>Gerente e Coordenador: senha, configuração do autenticador e o código, como o celular faria.</summary>
    private async Task<string> TokenComSegundoFator(HttpClient cliente, PapelUsuario papel)
    {
        var email = $"{papel}-{Guid.NewGuid():N}@gastra.test".ToLowerInvariant();
        await factory.CriarUsuario(email, papel);
        var json = GastraApiFactory.Json;

        var login = await (await cliente.PostAsJsonAsync("/api/autenticacao/login",
            new { email, senha = GastraApiFactory.SenhaPadrao }, json)).Content.ReadFromJsonAsync<LoginResponse>(json);
        var configuracao = await (await cliente.PostAsJsonAsync("/api/autenticacao/segundo-fator/configurar",
                new { tokenSegundoFator = login!.TokenSegundoFator }, json))
            .Content.ReadFromJsonAsync<ConfiguracaoSegundoFatorResponse>(json);
        var confirmacao = await cliente.PostAsJsonAsync("/api/autenticacao/segundo-fator/confirmar",
            new { tokenSegundoFator = login.TokenSegundoFator, codigo = GastraApiFactory.CodigoTotp(configuracao!.ChaveManual) },
            json);
        confirmacao.EnsureSuccessStatusCode();
        return (await confirmacao.Content.ReadFromJsonAsync<LoginResponse>(json))!.TokenAcesso!;
    }
}
