using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Application.UseCases.Usuarios;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Microsoft.Extensions.DependencyInjection;
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

    // --- Foto do funcionário ---

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];

    private static MultipartFormDataContent Arquivo(byte[] conteudo)
    {
        var corpo = new ByteArrayContent(conteudo);
        corpo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        return new MultipartFormDataContent { { corpo, "arquivo", "foto.png" } };
    }

    private async Task<UsuarioResponse> PorFoto(int id)
    {
        var resposta = await _cliente.PutAsync($"{Rota}/{id}/foto", Arquivo(Png));
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<UsuarioResponse>(Json))!;
    }

    private static bool NoDisco(string endereco) =>
        File.Exists(Path.Combine(DiretorioDasFotosDeFuncionarios, endereco[FotoDoFuncionario.Prefixo.Length..]));

    [Fact]
    public async Task Foto_PeloGerente_ApareceNaContaENoLogin_ESoSaiComLogin()
    {
        var email = NovoEmail("foto");
        var usuario = await Cadastrar(email);

        var conta = await PorFoto(usuario.Id);

        Assert.StartsWith(FotoDoFuncionario.Prefixo, conta.Foto);
        var login = await (await TentarLogin(email)).Content.ReadFromJsonAsync<LoginResponse>(Json);
        Assert.Equal(conta.Foto, login!.Foto);

        // Sem login, não sai; nem pela rota pública do cardápio, que lê outra pasta.
        var anonimo = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonimo.GetAsync(conta.Foto)).StatusCode);
        var nome = conta.Foto![FotoDoFuncionario.Prefixo.Length..];
        Assert.Equal(HttpStatusCode.NotFound, (await anonimo.GetAsync($"/api/cardapio/fotos/{nome}")).StatusCode);

        var peloGerente = await _cliente.GetAsync(conta.Foto);
        Assert.Equal(HttpStatusCode.OK, peloGerente.StatusCode);
        Assert.Equal("image/png", peloGerente.Content.Headers.ContentType!.MediaType);
        Assert.True(peloGerente.Headers.CacheControl!.NoStore);
        Assert.Equal(Png, await peloGerente.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Foto_ODonoVeAPropria_EOutroFuncionarioNao()
    {
        var email = NovoEmail("foto-dono");
        var conta = await PorFoto((await Cadastrar(email)).Id);

        var dono = factory.CreateClient();
        Autenticar(dono, await factory.Login(dono, email));
        var colega = factory.CreateClient();
        Autenticar(colega, await factory.TokenGarcom(colega));

        Assert.Equal(HttpStatusCode.OK, (await dono.GetAsync(conta.Foto)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await colega.GetAsync(conta.Foto)).StatusCode);
    }

    [Fact]
    public async Task Foto_Trocada_ApagaAAnteriorDoDisco()
    {
        var usuario = await Cadastrar(NovoEmail("foto-troca"));
        var primeira = (await PorFoto(usuario.Id)).Foto!;

        var segunda = (await PorFoto(usuario.Id)).Foto!;

        Assert.NotEqual(primeira, segunda);
        Assert.False(NoDisco(primeira));
        Assert.True(NoDisco(segunda));
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.GetAsync(primeira)).StatusCode);
    }

    [Fact]
    public async Task Foto_Tirada_ApagaDoDisco()
    {
        var usuario = await Cadastrar(NovoEmail("foto-tira"));
        var foto = (await PorFoto(usuario.Id)).Foto!;

        var resposta = await _cliente.DeleteAsync($"{Rota}/{usuario.Id}/foto");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Null((await resposta.Content.ReadFromJsonAsync<UsuarioResponse>(Json))!.Foto);
        Assert.False(NoDisco(foto));
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.GetAsync(foto)).StatusCode);
    }

    [Fact]
    public async Task Inativar_TiraAFotoDaConta_EApagaDoDisco()
    {
        var usuario = await Cadastrar(NovoEmail("foto-inativa"));
        var foto = (await PorFoto(usuario.Id)).Foto!;
        Assert.True(NoDisco(foto));

        await _cliente.PatchAsJsonAsync($"{Rota}/{usuario.Id}/situacao", new { ativo = false }, Json);

        var depois = await _cliente.GetFromJsonAsync<UsuarioResponse>($"{Rota}/{usuario.Id}", Json);
        Assert.Null(depois!.Foto);
        Assert.False(NoDisco(foto));
        Assert.Equal(HttpStatusCode.NotFound, (await _cliente.GetAsync(foto)).StatusCode);
    }

    [Fact]
    public async Task Foto_EmContaInativa_Retorna422()
    {
        var usuario = await Cadastrar(NovoEmail("foto-inativa-422"));
        await _cliente.PatchAsJsonAsync($"{Rota}/{usuario.Id}/situacao", new { ativo = false }, Json);

        var resposta = await _cliente.PutAsync($"{Rota}/{usuario.Id}/foto", Arquivo(Png));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal(["Reative a conta antes de pôr uma foto."], await LerErros(resposta));
    }

    [Fact]
    public async Task Foto_ComTextoDisfarcadoDePng_Retorna400()
    {
        var usuario = await Cadastrar(NovoEmail("foto-svg"));

        var resposta = await _cliente.PutAsync($"{Rota}/{usuario.Id}/foto", Arquivo("<svg onload=alert(1)>"u8.ToArray()));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(["A foto deve ser JPG, PNG ou WEBP, com até 5 MB."], await LerErros(resposta));
    }

    [Fact]
    public async Task Foto_PorGarcomOuSemLogin_EBarrada()
    {
        var usuario = await Cadastrar(NovoEmail("foto-barrada"));
        var garcom = factory.CreateClient();
        Autenticar(garcom, await factory.TokenGarcom(garcom));

        Assert.Equal(HttpStatusCode.Forbidden, (await garcom.PutAsync($"{Rota}/{usuario.Id}/foto", Arquivo(Png))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await factory.CreateClient().PutAsync($"{Rota}/{usuario.Id}/foto", Arquivo(Png))).StatusCode);
    }

    [Fact]
    public async Task FotoAntiga_NaPastaDoCardapio_VaiParaAPastaPrivada()
    {
        // Como ficava até a versão 1.0: a foto do funcionário era uma foto pública do cardápio.
        Directory.CreateDirectory(DiretorioDasFotos);
        var sozinha = $"{Guid.NewGuid():N}.png";
        var compartilhada = $"{Guid.NewGuid():N}.png";
        await File.WriteAllBytesAsync(Path.Combine(DiretorioDasFotos, sozinha), Png);
        await File.WriteAllBytesAsync(Path.Combine(DiretorioDasFotos, compartilhada), Png);

        var comFotoSo = await factory.CriarUsuario(NovoEmail("legado"), PapelUsuario.Garcom);
        var comFotoDoPrato = await factory.CriarUsuario(NovoEmail("legado"), PapelUsuario.Garcom);
        var foraDaPasta = await factory.CriarUsuario(NovoEmail("legado"), PapelUsuario.Garcom);
        var prato = await factory.CriarItemCardapio();
        var fotoForaDaPasta = $"/api/cardapio/fotos/{Guid.NewGuid():N}.png";

        using (var escopo = factory.Services.CreateScope())
        {
            var usuarios = escopo.ServiceProvider.GetRequiredService<IRepositorioUsuario>();
            (await usuarios.ObterPorId(comFotoSo.Id))!.DefinirFoto($"/api/cardapio/fotos/{sozinha}");
            (await usuarios.ObterPorId(comFotoDoPrato.Id))!.DefinirFoto($"/api/cardapio/fotos/{compartilhada}");
            (await usuarios.ObterPorId(foraDaPasta.Id))!.DefinirFoto(fotoForaDaPasta);
            (await escopo.ServiceProvider.GetRequiredService<IRepositorioItemCardapio>().ObterPorId(prato))!
                .DefinirImagem($"/api/cardapio/fotos/{compartilhada}");
            await escopo.ServiceProvider.GetRequiredService<IUnitOfWork>().Commit();
        }

        using (var escopo = factory.Services.CreateScope())
        {
            var recolher = escopo.ServiceProvider.GetRequiredService<IRecolherFotosAntigasDeFuncionariosUseCase>();
            Assert.Equal(2, await recolher.Executar());
        }

        var primeira = await _cliente.GetFromJsonAsync<UsuarioResponse>($"{Rota}/{comFotoSo.Id}", Json);
        var segunda = await _cliente.GetFromJsonAsync<UsuarioResponse>($"{Rota}/{comFotoDoPrato.Id}", Json);
        var terceira = await _cliente.GetFromJsonAsync<UsuarioResponse>($"{Rota}/{foraDaPasta.Id}", Json);

        Assert.StartsWith(FotoDoFuncionario.Prefixo, primeira!.Foto);
        Assert.Equal(Png, await _cliente.GetByteArrayAsync(primeira.Foto));
        Assert.False(File.Exists(Path.Combine(DiretorioDasFotos, sozinha)));

        // O prato continua com a foto dele; o funcionário ganhou uma cópia só sua.
        Assert.StartsWith(FotoDoFuncionario.Prefixo, segunda!.Foto);
        Assert.True(File.Exists(Path.Combine(DiretorioDasFotos, compartilhada)));

        // Arquivo que não está nesta pasta: a conta não perde a referência.
        Assert.Equal(fotoForaDaPasta, terceira!.Foto);
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

    // --- Redefinir senha e zerar o segundo fator (UC04, #141) ---

    private const string SenhaNova = "Senha-nova-do-gerente-9";

    [Fact]
    public async Task RedefinirSenha_TrocaASenha_EDerrubaAsSessoesAbertas()
    {
        var email = NovoEmail("garcom");
        var usuario = await Cadastrar(email);
        var tokenAntigo = await factory.Login(factory.CreateClient(), email);

        var resposta = await _cliente.PostAsJsonAsync($"{Rota}/{usuario.Id}/senha", new { senha = SenhaNova }, Json);

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, await StatusComToken(tokenAntigo));
        Assert.Equal(HttpStatusCode.Unauthorized, (await TentarLogin(email)).StatusCode);

        var comSenhaNova = await factory.CreateClient().PostAsJsonAsync("/api/autenticacao/login",
            new { email, senha = SenhaNova }, Json);
        Assert.Equal(HttpStatusCode.OK, comSenhaNova.StatusCode);

        // RN06: a senha nunca aparece no registro de auditoria.
        var registro = (await factory.Auditoria(EventoAuditoria.SenhaRedefinida)).Last();
        Assert.Equal(usuario.Id, registro.IdEntidade);
        Assert.DoesNotContain(SenhaNova, registro.Detalhes ?? string.Empty);
    }

    [Fact]
    public async Task RedefinirSenha_ComSenhaCurta_Retorna400_ESenhaAntigaContinuaValendo()
    {
        var email = NovoEmail("garcom");
        var usuario = await Cadastrar(email);

        var resposta = await _cliente.PostAsJsonAsync($"{Rota}/{usuario.Id}/senha", new { senha = "1234" }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains(await LerErros(resposta), e => e.Contains("8 caracteres"));
        Assert.Equal(HttpStatusCode.OK, (await TentarLogin(email)).StatusCode);
    }

    [Fact]
    public async Task RedefinirSenha_DaPropriaConta_Retorna422()
    {
        var resposta = await _cliente.PostAsJsonAsync($"{Rota}/{factory.IdGerente}/senha", new { senha = SenhaNova }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task RedefinirSenha_DeContaInexistente_Retorna404()
    {
        var resposta = await _cliente.PostAsJsonAsync($"{Rota}/999999/senha", new { senha = SenhaNova }, Json);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task ReiniciarSegundoFator_FazOProximoLoginPedirAConfiguracaoDeNovo()
    {
        var email = NovoEmail("coordenador");
        var usuario = await Cadastrar(email, "Coordenador");
        var cliente = factory.CreateClient();
        var token = await VincularAutenticador(cliente, email);
        Assert.True((await _cliente.GetFromJsonAsync<UsuarioResponse>($"{Rota}/{usuario.Id}", Json))!.SegundoFatorConfigurado);

        var resposta = await _cliente.DeleteAsync($"{Rota}/{usuario.Id}/segundo-fator");

        Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, await StatusComToken(token));
        Assert.False((await _cliente.GetFromJsonAsync<UsuarioResponse>($"{Rota}/{usuario.Id}", Json))!.SegundoFatorConfigurado);

        var login = await (await factory.CreateClient().PostAsJsonAsync("/api/autenticacao/login",
            new { email, senha = SenhaPadrao }, Json)).Content.ReadFromJsonAsync<LoginResponse>(Json);
        Assert.True(login!.RequerConfiguracaoSegundoFator);
    }

    [Fact]
    public async Task ReiniciarSegundoFator_DeQuemNaoUsaSegundoFator_Retorna422()
    {
        var usuario = await Cadastrar(NovoEmail("garcom"));

        var resposta = await _cliente.DeleteAsync($"{Rota}/{usuario.Id}/segundo-fator");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task ReiniciarSegundoFator_DaPropriaConta_Retorna422()
    {
        var resposta = await _cliente.DeleteAsync($"{Rota}/{factory.IdGerente}/segundo-fator");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task RedefinirSenhaEZerarSegundoFator_ComoGarcom_Retorna403()
    {
        var alvo = await Cadastrar(NovoEmail("alvo"));
        var email = NovoEmail("garcom");
        await factory.CriarUsuario(email, PapelUsuario.Garcom);
        var garcom = factory.CreateClient();
        Autenticar(garcom, await factory.Login(garcom, email));

        var senha = await garcom.PostAsJsonAsync($"{Rota}/{alvo.Id}/senha", new { senha = SenhaNova }, Json);
        var segundoFator = await garcom.DeleteAsync($"{Rota}/{alvo.Id}/segundo-fator");

        Assert.Equal(HttpStatusCode.Forbidden, senha.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, segundoFator.StatusCode);
    }

    /// <summary>Primeiro acesso de Gerente ou Coordenador: vincula o autenticador e devolve o token de acesso.</summary>
    private async Task<string> VincularAutenticador(HttpClient cliente, string email)
    {
        var login = await (await cliente.PostAsJsonAsync("/api/autenticacao/login", new { email, senha = SenhaPadrao }, Json))
            .Content.ReadFromJsonAsync<LoginResponse>(Json);

        var configuracao = await (await cliente.PostAsJsonAsync("/api/autenticacao/segundo-fator/configurar",
            new { tokenSegundoFator = login!.TokenSegundoFator }, Json)).Content.ReadFromJsonAsync<ConfiguracaoSegundoFatorResponse>(Json);

        var confirmacao = await cliente.PostAsJsonAsync("/api/autenticacao/segundo-fator/confirmar",
            new { tokenSegundoFator = login.TokenSegundoFator, codigo = CodigoTotp(configuracao!.ChaveManual) }, Json);
        confirmacao.EnsureSuccessStatusCode();
        return (await confirmacao.Content.ReadFromJsonAsync<LoginResponse>(Json))!.TokenAcesso!;
    }
}
