using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Communication.Responses;
using Gastra.Domain.Enums;
using static Gastra.Api.Tests.Infraestrutura.GastraApiFactory;

namespace Gastra.Api.Tests.Controllers;

public class CardapioControllerTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>, IAsyncLifetime
{
    private const string Rota = "/api/cardapio";

    private readonly HttpClient _cliente = factory.CreateClient();

    // Os testes de gestão do cardápio rodam autenticados como Gerente (RF19–RF21).
    public async Task InitializeAsync() => Autenticar(_cliente, await factory.TokenGerente(factory.CreateClient()));

    public Task DisposeAsync() => Task.CompletedTask;

    private static object ItemValido(string nome = "Risoto de cogumelos") => new
    {
        nome,
        categoria = "PratoPrincipal",
        preco = 89.90m,
        descricao = "Arroz arbóreo com shimeji",
        flagsDieteticas = new[] { "Vegetariano", "SemGluten" },
    };

    private async Task<ItemCardapioResponse> Cadastrar(string nome)
    {
        var resposta = await _cliente.PostAsJsonAsync(Rota, ItemValido(nome), Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<ItemCardapioResponse>(Json))!;
    }

    private static async Task<List<string>> LerErros(HttpResponseMessage resposta) =>
        (await resposta.Content.ReadFromJsonAsync<ErroResponse>(Json))!.Erros;

    [Fact]
    public async Task Cadastrar_ComDadosValidos_Retorna201ComOItem()
    {
        var resposta = await _cliente.PostAsJsonAsync(Rota, ItemValido(), Json);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(resposta.Headers.Location);

        var item = await resposta.Content.ReadFromJsonAsync<ItemCardapioResponse>(Json);
        Assert.True(item!.Id > 0);
        Assert.True(item.Disponivel);
        Assert.Equal(89.90m, item.Preco);
        Assert.Equal(2, item.FlagsDieteticas.Count);
    }

    [Fact]
    public async Task Cadastrar_ComNomeVazioEPrecoZero_Retorna400ComOsDoisErrosEmPortugues()
    {
        var invalido = new { nome = "", categoria = "Bebida", preco = 0, descricao = "", flagsDieteticas = Array.Empty<string>() };

        var resposta = await _cliente.PostAsJsonAsync(Rota, invalido, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var erros = await LerErros(resposta);
        Assert.Contains("O nome é obrigatório.", erros);
        Assert.Contains("O preço deve ser maior que zero.", erros);
    }

    [Fact]
    public async Task Cadastrar_ComDadosInvalidosEmIngles_RetornaMensagensEmIngles()
    {
        var invalido = new { nome = "", categoria = "Bebida", preco = 10, descricao = "", flagsDieteticas = Array.Empty<string>() };
        using var requisicao = new HttpRequestMessage(HttpMethod.Post, Rota) { Content = JsonContent.Create(invalido, options: Json) };
        requisicao.Headers.AcceptLanguage.ParseAdd("en-US");

        var resposta = await _cliente.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(["Name is required."], await LerErros(resposta));
    }

    [Fact]
    public async Task Cadastrar_ComCategoriaInexistente_Retorna400NoFormatoPadrao()
    {
        var invalido = new { nome = "Pizza", categoria = "Pizza", preco = 50, descricao = "", flagsDieteticas = Array.Empty<string>() };

        var resposta = await _cliente.PostAsJsonAsync(Rota, invalido, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(["Requisição inválida."], await LerErros(resposta));
    }

    [Fact]
    public async Task AtualizarPreco_DeItemExistente_Retorna204EOPrecoMuda()
    {
        var item = await Cadastrar("Pudim");

        var resposta = await _cliente.PatchAsJsonAsync($"{Rota}/{item.Id}/preco", new { preco = 15.50m }, Json);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        var atualizado = await _cliente.GetFromJsonAsync<ItemCardapioResponse>($"{Rota}/{item.Id}", Json);
        Assert.Equal(15.50m, atualizado!.Preco);
    }

    [Fact]
    public async Task AtualizarPreco_DeItemInexistente_Retorna404ComMensagem()
    {
        var resposta = await _cliente.PatchAsJsonAsync($"{Rota}/999999/preco", new { preco = 10m }, Json);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(["Item do cardápio não encontrado."], await LerErros(resposta));
    }

    [Fact]
    public async Task MarcarIndisponivel_ItemSaiDoCardapioDigitalMasContinuaNaGestao()
    {
        var item = await Cadastrar("Suco de caju");

        var resposta = await _cliente.PatchAsJsonAsync($"{Rota}/{item.Id}/disponibilidade", new { disponivel = false }, Json);
        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);

        var digital = await factory.CreateClient().GetFromJsonAsync<List<ItemCardapioResponse>>($"{Rota}/digital", Json);
        var gestao = await _cliente.GetFromJsonAsync<List<ItemCardapioResponse>>(Rota, Json);
        Assert.DoesNotContain(digital!, i => i.Id == item.Id);
        Assert.Contains(gestao!, i => i.Id == item.Id && !i.Disponivel);
    }

    // --- Autorização (RF19–RF21, RNF04) ---

    [Fact]
    public async Task Cadastrar_SemLogin_Retorna401()
    {
        var anonimo = factory.CreateClient();

        var resposta = await anonimo.PostAsJsonAsync(Rota, ItemValido(), Json);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Equal(["É necessário fazer login."], await LerErros(resposta));
    }

    [Fact]
    public async Task Cadastrar_ComoGarcom_Retorna403()
    {
        var email = $"garcom-{Guid.NewGuid():N}@gastra.test";
        await factory.CriarUsuario(email, PapelUsuario.Garcom);
        var garcom = factory.CreateClient();
        Autenticar(garcom, await factory.Login(garcom, email));

        var resposta = await garcom.PostAsJsonAsync(Rota, ItemValido(), Json);

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        Assert.Equal(["Seu perfil não tem permissão para esta ação."], await LerErros(resposta));
    }

    private async Task<HttpClient> ClienteGarcom()
    {
        var email = $"garcom-{Guid.NewGuid():N}@gastra.test";
        await factory.CriarUsuario(email, PapelUsuario.Garcom);
        var garcom = factory.CreateClient();
        Autenticar(garcom, await factory.Login(garcom, email));
        return garcom;
    }

    // A tela da comanda carrega esta lista junto com a comanda: sem a leitura, o garçom não abre comanda nenhuma.
    [Fact]
    public async Task Listar_ComoGarcom_Retorna200InclusiveComItemIndisponivel()
    {
        var item = await Cadastrar("Item esgotado para o garçom");
        await _cliente.PatchAsJsonAsync($"{Rota}/{item.Id}/disponibilidade", new { disponivel = false }, Json);
        var garcom = await ClienteGarcom();

        var resposta = await garcom.GetAsync(Rota);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var itens = await resposta.Content.ReadFromJsonAsync<List<ItemCardapioResponse>>(Json);
        Assert.Contains(itens!, i => i.Id == item.Id && !i.Disponivel);
    }

    [Fact]
    public async Task Listar_SemLogin_Retorna401()
    {
        var resposta = await factory.CreateClient().GetAsync(Rota);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Alterar_ComoGarcom_Retorna403EmTodaAcaoDeGestao()
    {
        var item = await Cadastrar("Item que o garçom não altera");
        var garcom = await ClienteGarcom();

        var preco = await garcom.PatchAsJsonAsync($"{Rota}/{item.Id}/preco", new { preco = 1m }, Json);
        var imagem = await garcom.PatchAsJsonAsync($"{Rota}/{item.Id}/imagem", new { imagem = (string?)null }, Json);
        var disponibilidade = await garcom.PatchAsJsonAsync($"{Rota}/{item.Id}/disponibilidade", new { disponivel = false }, Json);

        Assert.Equal(HttpStatusCode.Forbidden, preco.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, imagem.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, disponibilidade.StatusCode);
    }

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];

    private static MultipartFormDataContent Arquivo(byte[] conteudo, string nome = "foto.png")
    {
        var corpo = new ByteArrayContent(conteudo);
        corpo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        return new MultipartFormDataContent { { corpo, "arquivo", nome } };
    }

    [Fact]
    public async Task EnviarFoto_GuardaEOClienteVeSemLogin()
    {
        var envio = await _cliente.PostAsync($"{Rota}/fotos", Arquivo(Png));

        Assert.Equal(HttpStatusCode.OK, envio.StatusCode);
        var foto = await envio.Content.ReadFromJsonAsync<FotoEnviadaResponse>(Json);
        var leitura = await factory.CreateClient().GetAsync(foto!.Imagem);
        Assert.Equal(HttpStatusCode.OK, leitura.StatusCode);
        Assert.Equal("image/png", leitura.Content.Headers.ContentType!.MediaType);
        Assert.Equal(Png, await leitura.Content.ReadAsByteArrayAsync());

        var item = await Cadastrar("Prato com foto importada");
        var gravacao = await _cliente.PatchAsJsonAsync($"{Rota}/{item.Id}/imagem", new { imagem = foto.Imagem }, Json);
        Assert.Equal(HttpStatusCode.NoContent, gravacao.StatusCode);
    }

    [Fact]
    public async Task EnviarFoto_ComTextoDisfarcadoDePng_Retorna400()
    {
        var resposta = await _cliente.PostAsync($"{Rota}/fotos", Arquivo("<svg onload=alert(1)>"u8.ToArray(), "foto.png"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(["A foto deve ser JPG, PNG ou WEBP, com até 5 MB."], await LerErros(resposta));
    }

    [Fact]
    public async Task EnviarFoto_AcimaDoTamanho_Retorna400()
    {
        var grande = new byte[5 * 1024 * 1024 + 1];
        Png.CopyTo(grande, 0);

        var resposta = await _cliente.PostAsync($"{Rota}/fotos", Arquivo(grande));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task EnviarFoto_ComoGarcomOuSemLogin_EBarrado()
    {
        var garcom = await ClienteGarcom();

        Assert.Equal(HttpStatusCode.Forbidden, (await garcom.PostAsync($"{Rota}/fotos", Arquivo(Png))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().PostAsync($"{Rota}/fotos", Arquivo(Png))).StatusCode);
    }

    [Theory]
    [InlineData("..%2Fappsettings.json")]
    [InlineData("nao-existe.png")]
    [InlineData("00000000000000000000000000000000.png")]
    public async Task ObterFoto_ComNomeQueNaoEDeFotoGuardada_Retorna404(string nome)
    {
        var resposta = await factory.CreateClient().GetAsync($"{Rota}/fotos/{nome}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task CardapioDigital_SemLogin_Retorna200()
    {
        await Cadastrar("Água com gás");

        var resposta = await factory.CreateClient().GetAsync($"{Rota}/digital");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    // --- RF05: foto do item no cardápio digital (#141) ---

    [Fact]
    public async Task Cadastrar_ComEnderecoDeFoto_GuardaAImagem()
    {
        var resposta = await _cliente.PostAsJsonAsync(Rota,
            new { nome = "Pudim com foto", categoria = "Sobremesa", preco = 19m, descricao = "", flagsDieteticas = Array.Empty<string>(), imagem = "https://cdn.exemplo.com/pudim.jpg" }, Json);

        resposta.EnsureSuccessStatusCode();
        var item = await resposta.Content.ReadFromJsonAsync<ItemCardapioResponse>(Json);
        Assert.Equal("https://cdn.exemplo.com/pudim.jpg", item!.Imagem);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("//cdn.exemplo.com/pudim.jpg")]
    [InlineData("pudim.jpg")]
    public async Task Cadastrar_ComEnderecoDeFotoInvalido_Retorna400(string imagem)
    {
        var resposta = await _cliente.PostAsJsonAsync(Rota,
            new { nome = "Pudim", categoria = "Sobremesa", preco = 19m, descricao = "", flagsDieteticas = Array.Empty<string>(), imagem }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains(await LerErros(resposta), e => e.Contains("imagem", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AlterarImagem_TrocaEDepoisRemoveAFoto()
    {
        var item = await Cadastrar("Petit gâteau");

        var troca = await _cliente.PatchAsJsonAsync($"{Rota}/{item.Id}/imagem", new { imagem = "/fotos/petit-gateau.jpg" }, Json);

        Assert.Equal(HttpStatusCode.NoContent, troca.StatusCode);
        var comFoto = await _cliente.GetFromJsonAsync<ItemCardapioResponse>($"{Rota}/{item.Id}", Json);
        Assert.Equal("/fotos/petit-gateau.jpg", comFoto!.Imagem);

        await _cliente.PatchAsJsonAsync($"{Rota}/{item.Id}/imagem", new { imagem = (string?)null }, Json);

        var semFoto = await _cliente.GetFromJsonAsync<ItemCardapioResponse>($"{Rota}/{item.Id}", Json);
        Assert.Null(semFoto!.Imagem);
    }

    [Fact]
    public async Task AlterarImagem_DeItemInexistente_Retorna404()
    {
        var resposta = await _cliente.PatchAsJsonAsync($"{Rota}/999999/imagem", new { imagem = "https://cdn.exemplo.com/x.jpg" }, Json);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }
}
