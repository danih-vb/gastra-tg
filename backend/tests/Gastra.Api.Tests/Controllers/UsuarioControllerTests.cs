using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Communication.Responses;
using Gastra.Domain.Enums;
using static Gastra.Api.Tests.Infraestrutura.GastraApiFactory;

namespace Gastra.Api.Tests.Controllers;

public class UsuarioControllerTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>, IAsyncLifetime
{
    private const string Rota = "/api/usuarios";

    private readonly HttpClient _cliente = factory.CreateClient();

    // RF18: gestão de contas roda autenticada como Gerente.
    public async Task InitializeAsync() => Autenticar(_cliente, await factory.TokenGerente(factory.CreateClient()));

    public Task DisposeAsync() => Task.CompletedTask;

    private static string NovoEmail(string prefixo) => $"{prefixo}-{Guid.NewGuid():N}@gastra.test";

    private async Task<UsuarioResponse> Cadastrar(string email, string papel = "Garcom")
    {
        var resposta = await _cliente.PostAsJsonAsync(Rota, new { nome = "Carlos Lima", email, senha = SenhaPadrao, papel }, Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<UsuarioResponse>(Json))!;
    }

    private static async Task<List<string>> LerErros(HttpResponseMessage resposta) =>
        (await resposta.Content.ReadFromJsonAsync<ErroResponse>(Json))!.Erros;

    private async Task<HttpResponseMessage> TentarLogin(string email) =>
        await factory.CreateClient().PostAsJsonAsync("/api/autenticacao/login", new { email, senha = SenhaPadrao }, Json);

    private async Task<HttpStatusCode> StatusComToken(string token)
    {
        var cliente = factory.CreateClient();
        Autenticar(cliente, token);
        // Qualquer rota que só exige login serve: o logoff devolve 204 com token válido.
        return (await cliente.PostAsync("/api/autenticacao/logoff", null)).StatusCode;
    }

    // --- Cadastrar ---

    [Fact]
    public async Task Cadastrar_ComDadosValidos_Retorna201EOUsuarioConsegueEntrar()
    {
        var email = NovoEmail("garcom");

        var resposta = await _cliente.PostAsJsonAsync(Rota, new { nome = "Carlos Lima", email, senha = SenhaPadrao, papel = "Garcom" }, Json);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(resposta.Headers.Location);

        var json = await resposta.Content.ReadAsStringAsync();
        Assert.DoesNotContain("senha", json, StringComparison.OrdinalIgnoreCase);

        var usuario = await resposta.Content.ReadFromJsonAsync<UsuarioResponse>(Json);
        Assert.True(usuario!.Ativo);
        Assert.False(usuario.SegundoFatorConfigurado);

        Assert.Equal(HttpStatusCode.OK, (await TentarLogin(email)).StatusCode);
    }

    [Fact]
    public async Task Cadastrar_ComDadosInvalidos_Retorna400ComTodosOsErros()
    {
        var invalido = new { nome = "", email = "nao-e-email", senha = "123", papel = "Garcom" };

        var resposta = await _cliente.PostAsJsonAsync(Rota, invalido, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(
            ["O nome é obrigatório.", "O e-mail informado é inválido.", "A senha deve ter pelo menos 8 caracteres."],
            await LerErros(resposta));
    }

    [Fact]
    public async Task Cadastrar_ComSenhaAcimaDoLimiteDoBCrypt_Retorna400()
    {
        var resposta = await _cliente.PostAsJsonAsync(Rota,
            new { nome = "Carlos Lima", email = NovoEmail("longa"), senha = new string('a', 73), papel = "Garcom" }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(["A senha é longa demais."], await LerErros(resposta));
    }

    [Fact]
    public async Task Cadastrar_ComEmailJaUsadoEmOutraCaixa_Retorna422()
    {
        var email = NovoEmail("repetido");
        await Cadastrar(email);

        var resposta = await _cliente.PostAsJsonAsync(Rota,
            new { nome = "Outra Pessoa", email = email.ToUpperInvariant(), senha = SenhaPadrao, papel = "Metre" }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal(["Já existe uma conta com este e-mail."], await LerErros(resposta));
    }

    // --- Permissões ---

    [Fact]
    public async Task GestaoDeContas_SemLogin_Retorna401()
    {
        var resposta = await factory.CreateClient().GetAsync(Rota);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task GestaoDeContas_ComoGarcom_Retorna403()
    {
        var email = NovoEmail("garcom");
        await factory.CriarUsuario(email, PapelUsuario.Garcom);
        var cliente = factory.CreateClient();
        Autenticar(cliente, await factory.Login(cliente, email));

        var resposta = await cliente.GetAsync(Rota);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    // --- Listar e obter ---

    [Fact]
    public async Task Listar_TrazContasAtivasEInativas()
    {
        var ativo = await Cadastrar(NovoEmail("ativo"));
        var inativo = await Cadastrar(NovoEmail("inativo"));
        await _cliente.PatchAsJsonAsync($"{Rota}/{inativo.Id}/situacao", new { ativo = false }, Json);

        var usuarios = await _cliente.GetFromJsonAsync<List<UsuarioResponse>>(Rota, Json);

        Assert.Contains(usuarios!, u => u.Id == ativo.Id && u.Ativo);
        Assert.Contains(usuarios!, u => u.Id == inativo.Id && !u.Ativo);
    }

    [Fact]
    public async Task Obter_Inexistente_Retorna404()
    {
        var resposta = await _cliente.GetAsync($"{Rota}/999999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(["Usuário não encontrado."], await LerErros(resposta));
    }

    // --- Editar ---

    [Fact]
    public async Task Editar_TrocandoOPapel_DerrubaOTokenAntigoENovoLoginVemComONovoPapel()
    {
        var email = NovoEmail("promovido");
        var usuario = await Cadastrar(email);
        var tokenAntigo = await factory.Login(factory.CreateClient(), email);

        var resposta = await _cliente.PutAsJsonAsync($"{Rota}/{usuario.Id}", new { nome = "Carlos Lima", email, papel = "Metre" }, Json);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, await StatusComToken(tokenAntigo));

        var novoLogin = await (await TentarLogin(email)).Content.ReadFromJsonAsync<LoginResponse>(Json);
        Assert.Equal(Gastra.Communication.Enums.PapelUsuario.Metre, novoLogin!.Papel);
    }

    [Fact]
    public async Task Editar_ComEmailDeOutraConta_Retorna422()
    {
        var emailOcupado = NovoEmail("ocupado");
        await Cadastrar(emailOcupado);
        var usuario = await Cadastrar(NovoEmail("editado"));

        var resposta = await _cliente.PutAsJsonAsync($"{Rota}/{usuario.Id}",
            new { nome = "Carlos Lima", email = emailOcupado, papel = "Garcom" }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Editar_OProprioPapel_Retorna422()
    {
        var gerente = await _cliente.GetFromJsonAsync<UsuarioResponse>($"{Rota}/{factory.IdGerente}", Json);

        var resposta = await _cliente.PutAsJsonAsync($"{Rota}/{gerente!.Id}",
            new { nome = gerente.Nome, email = gerente.Email, papel = "Garcom" }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal(["Você não pode alterar o próprio papel."], await LerErros(resposta));
    }

    // --- Inativar e reativar ---

    [Fact]
    public async Task Inativar_BloqueiaTokenELogin_EReativarDevolveOAcesso()
    {
        var email = NovoEmail("desligado");
        var usuario = await Cadastrar(email);
        var token = await factory.Login(factory.CreateClient(), email);

        var inativacao = await _cliente.PatchAsJsonAsync($"{Rota}/{usuario.Id}/situacao", new { ativo = false }, Json);

        Assert.Equal(HttpStatusCode.NoContent, inativacao.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, await StatusComToken(token));
        Assert.Equal(HttpStatusCode.Unauthorized, (await TentarLogin(email)).StatusCode);

        await _cliente.PatchAsJsonAsync($"{Rota}/{usuario.Id}/situacao", new { ativo = true }, Json);

        Assert.Equal(HttpStatusCode.OK, (await TentarLogin(email)).StatusCode);
    }

    [Fact]
    public async Task Inativar_APropriaConta_Retorna422()
    {
        var resposta = await _cliente.PatchAsJsonAsync($"{Rota}/{factory.IdGerente}/situacao", new { ativo = false }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal(["Você não pode inativar a própria conta."], await LerErros(resposta));
    }
}
