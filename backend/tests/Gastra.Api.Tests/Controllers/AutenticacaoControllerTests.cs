using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Communication.Responses;
using Gastra.Domain.Enums;
using static Gastra.Api.Tests.Infraestrutura.GastraApiFactory;

namespace Gastra.Api.Tests.Controllers;

public class AutenticacaoControllerTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>
{
    private const string Login = "/api/autenticacao/login";
    private const string Configurar = "/api/autenticacao/segundo-fator/configurar";
    private const string Confirmar = "/api/autenticacao/segundo-fator/confirmar";
    private const string Logoff = "/api/autenticacao/logoff";
    private const string RotaProtegida = "/api/cardapio";

    private const string CredenciaisInvalidas =
        "E-mail ou senha inválidos. Depois de 5 tentativas erradas seguidas, o acesso fica bloqueado por 15 minutos.";
    private const string AcessoBloqueado = "Muitas tentativas erradas seguidas. O acesso está bloqueado por 15 minutos.";

    private readonly HttpClient _cliente = factory.CreateClient();

    private static string NovoEmail(string prefixo) => $"{prefixo}-{Guid.NewGuid():N}@gastra.test";

    private async Task<LoginResponse> Logar(string email, string senha = SenhaPadrao)
    {
        var resposta = await _cliente.PostAsJsonAsync(Login, new { email, senha }, Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<LoginResponse>(Json))!;
    }

    private static async Task<List<string>> LerErros(HttpResponseMessage resposta) =>
        (await resposta.Content.ReadFromJsonAsync<ErroResponse>(Json))!.Erros;

    // --- UC01: login ---

    [Fact]
    public async Task Login_GarcomComCredenciaisValidas_RecebeTokenDeAcessoDireto()
    {
        var email = NovoEmail("garcom");
        await factory.CriarUsuario(email, PapelUsuario.Garcom);

        var login = await Logar(email.ToUpperInvariant());

        Assert.False(login.RequerSegundoFator);
        Assert.False(string.IsNullOrWhiteSpace(login.TokenAcesso));
        Assert.Equal(Gastra.Communication.Enums.PapelUsuario.Garcom, login.Papel);
    }

    [Theory]
    [InlineData("senha-errada", true)]
    [InlineData(SenhaPadrao, false)]
    public async Task Login_ComSenhaErradaOuEmailInexistente_Retorna401ComAMesmaMensagem(string senha, bool usuarioExiste)
    {
        var email = NovoEmail("metre");
        if (usuarioExiste)
            await factory.CriarUsuario(email, PapelUsuario.Metre);

        var resposta = await _cliente.PostAsJsonAsync(Login, new { email, senha }, Json);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Equal([CredenciaisInvalidas], await LerErros(resposta));
    }

    [Fact]
    public async Task Login_UsuarioInativo_Retorna401()
    {
        var email = NovoEmail("inativo");
        await factory.CriarUsuario(email, PapelUsuario.Garcom, ativo: false);

        var resposta = await _cliente.PostAsJsonAsync(Login, new { email, senha = SenhaPadrao }, Json);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_SemEmailESenha_Retorna400()
    {
        var resposta = await _cliente.PostAsJsonAsync(Login, new { email = "", senha = "" }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(["O e-mail é obrigatório.", "A senha é obrigatória."], await LerErros(resposta));
    }

    // --- UC02 e RN07: segundo fator ---

    [Fact]
    public async Task Login_Gerente_NaoRecebeTokenDeAcessoEPrecisaConfigurarOAutenticador()
    {
        var email = NovoEmail("gerente");
        await factory.CriarUsuario(email, PapelUsuario.Gerente);

        var login = await Logar(email);

        Assert.True(login.RequerSegundoFator);
        Assert.True(login.RequerConfiguracaoSegundoFator);
        Assert.Null(login.TokenAcesso);
        Assert.False(string.IsNullOrWhiteSpace(login.TokenSegundoFator));
    }

    [Fact]
    public async Task SegundoFator_FluxoCompleto_LiberaAcessoEOSegredoNaoPodeSerGeradoDeNovo()
    {
        var email = NovoEmail("coordenador");
        await factory.CriarUsuario(email, PapelUsuario.Coordenador);
        var login = await Logar(email);

        var configuracao = await (await _cliente.PostAsJsonAsync(Configurar, new { tokenSegundoFator = login.TokenSegundoFator }, Json))
            .Content.ReadFromJsonAsync<ConfiguracaoSegundoFatorResponse>(Json);
        Assert.StartsWith("otpauth://totp/GASTRA:", configuracao!.UriConfiguracao);

        var confirmacao = await _cliente.PostAsJsonAsync(Confirmar,
            new { tokenSegundoFator = login.TokenSegundoFator, codigo = CodigoTotp(configuracao.ChaveManual) }, Json);
        Assert.Equal(HttpStatusCode.OK, confirmacao.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace((await confirmacao.Content.ReadFromJsonAsync<LoginResponse>(Json))!.TokenAcesso));

        // RN07: o segredo é gerado uma única vez.
        var segundaConfiguracao = await _cliente.PostAsJsonAsync(Configurar, new { tokenSegundoFator = login.TokenSegundoFator }, Json);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, segundaConfiguracao.StatusCode);

        // No próximo login, só pede o código: não pede configuração de novo.
        var novoLogin = await Logar(email);
        Assert.True(novoLogin.RequerSegundoFator);
        Assert.False(novoLogin.RequerConfiguracaoSegundoFator);
    }

    [Fact]
    public async Task SegundoFator_ComCodigoErrado_Retorna401()
    {
        var email = NovoEmail("gerente");
        await factory.CriarUsuario(email, PapelUsuario.Gerente);
        var login = await Logar(email);
        await _cliente.PostAsJsonAsync(Configurar, new { tokenSegundoFator = login.TokenSegundoFator }, Json);

        var resposta = await _cliente.PostAsJsonAsync(Confirmar, new { tokenSegundoFator = login.TokenSegundoFator, codigo = "000000" }, Json);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Equal(["Código de verificação inválido."], await LerErros(resposta));
    }

    // --- RN09: bloqueio por tentativas ---

    private Task<HttpResponseMessage> TentarLogin(string email, string senha) =>
        _cliente.PostAsJsonAsync(Login, new { email, senha }, Json);

    private Task<HttpResponseMessage> TentarCodigo(string tokenSegundoFator, string codigo) =>
        _cliente.PostAsJsonAsync(Confirmar, new { tokenSegundoFator, codigo }, Json);

    /// <summary>Um código de 6 dígitos que com certeza não é o atual.</summary>
    private static string CodigoErrado(string chaveManual) =>
        ((int.Parse(CodigoTotp(chaveManual)) + 500_000) % 1_000_000).ToString("D6");

    /// <summary>Gerente com o autenticador já vinculado; devolve o e-mail e a chave manual.</summary>
    private async Task<(string Email, string Chave)> GerenteComAutenticador()
    {
        var email = NovoEmail("gerente");
        await factory.CriarUsuario(email, PapelUsuario.Gerente);
        var login = await Logar(email);
        var configuracao = await (await _cliente.PostAsJsonAsync(Configurar, new { tokenSegundoFator = login.TokenSegundoFator }, Json))
            .Content.ReadFromJsonAsync<ConfiguracaoSegundoFatorResponse>(Json);
        return (email, configuracao!.ChaveManual);
    }

    [Fact]
    public async Task Login_CincoSenhasErradas_BloqueiaAtéASenhaCertaComAMesmaMensagem()
    {
        var email = NovoEmail("garcom");
        await factory.CriarUsuario(email, PapelUsuario.Garcom);
        for (var i = 0; i < 5; i++)
            await TentarLogin(email, "senha-errada");

        var comSenhaCerta = await TentarLogin(email, SenhaPadrao);

        Assert.Equal(HttpStatusCode.Unauthorized, comSenhaCerta.StatusCode);
        // Mesma mensagem da senha errada: senão cinco erros revelariam que o e-mail existe.
        Assert.Equal([CredenciaisInvalidas], await LerErros(comSenhaCerta));
    }

    [Fact]
    public async Task Login_ComSucesso_ZeraAContagem()
    {
        var email = NovoEmail("metre");
        await factory.CriarUsuario(email, PapelUsuario.Metre);

        for (var rodada = 0; rodada < 2; rodada++)
        {
            for (var i = 0; i < 4; i++)
                await TentarLogin(email, "senha-errada");

            Assert.Equal(HttpStatusCode.OK, (await TentarLogin(email, SenhaPadrao)).StatusCode);
        }
    }

    [Fact]
    public async Task SegundoFator_QuintoCodigoErrado_BloqueiaComMensagemPropria()
    {
        var (email, chave) = await GerenteComAutenticador();
        var token = (await Logar(email)).TokenSegundoFator!;
        for (var i = 0; i < 4; i++)
            Assert.Equal(["Código de verificação inválido."], await LerErros(await TentarCodigo(token, CodigoErrado(chave))));

        var quinto = await TentarCodigo(token, CodigoErrado(chave));
        var comCodigoCerto = await TentarCodigo(token, CodigoTotp(chave));

        Assert.Equal([AcessoBloqueado], await LerErros(quinto));
        Assert.Equal(HttpStatusCode.Unauthorized, comCodigoCerto.StatusCode);
        Assert.Equal([AcessoBloqueado], await LerErros(comCodigoCerto));
    }

    [Fact]
    public async Task SegundoFator_LogarDeNovo_NaoDaMaisPalpitesDeCodigo()
    {
        var (email, chave) = await GerenteComAutenticador();

        // Acertar a senha entre um palpite e outro não zera a contagem: são 5 erros no total, não 5 por login.
        for (var i = 0; i < 5; i++)
        {
            var token = (await Logar(email)).TokenSegundoFator!;
            await TentarCodigo(token, CodigoErrado(chave));
        }

        // Bloqueada, a conta recusa até a senha certa: não sai nem um token de segundo fator para tentar mais.
        var comSenhaCerta = await TentarLogin(email, SenhaPadrao);
        Assert.Equal(HttpStatusCode.Unauthorized, comSenhaCerta.StatusCode);
        Assert.Equal([CredenciaisInvalidas], await LerErros(comSenhaCerta));
    }

    [Fact]
    public async Task SegundoFator_CodigoCerto_ZeraAContagem()
    {
        var (email, chave) = await GerenteComAutenticador();

        for (var rodada = 0; rodada < 2; rodada++)
        {
            var token = (await Logar(email)).TokenSegundoFator!;
            for (var i = 0; i < 4; i++)
                await TentarCodigo(token, CodigoErrado(chave));

            Assert.Equal(HttpStatusCode.OK, (await TentarCodigo(token, CodigoTotp(chave))).StatusCode);
        }
    }

    [Fact]
    public async Task TokenDoSegundoFator_NaoServeComoTokenDeAcesso()
    {
        var email = NovoEmail("gerente");
        await factory.CriarUsuario(email, PapelUsuario.Gerente);
        var login = await Logar(email);

        var cliente = factory.CreateClient();
        Autenticar(cliente, login.TokenSegundoFator!);
        var resposta = await cliente.GetAsync(RotaProtegida);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    // --- UC03: logoff ---

    [Fact]
    public async Task Logoff_InvalidaOTokenNoServidor()
    {
        var token = await factory.TokenGerente(factory.CreateClient());
        var cliente = factory.CreateClient();
        Autenticar(cliente, token);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync(RotaProtegida)).StatusCode);

        var logoff = await cliente.PostAsync(Logoff, null);
        Assert.Equal(HttpStatusCode.NoContent, logoff.StatusCode);

        // O mesmo token, que ainda não expirou, deixa de funcionar.
        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync(RotaProtegida)).StatusCode);
    }
}
