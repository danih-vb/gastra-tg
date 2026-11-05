using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using static Gastra.Api.Tests.Infraestrutura.GastraApiFactory;

namespace Gastra.Api.Tests.Controllers;

/// <summary>
/// API com o limite por IP ligado e uma cota pequena, atrás de um "nginx" na rede do IP de teste (#230). Cada teste
/// manda o seu próprio IP no X-Forwarded-For, como o nginx faria, e por isso tem a sua cota: é também a prova de que
/// o limite enxerga o IP do navegador, e não o do proxy.
/// </summary>
public class LimiteDeRequisicoesTests(LimiteDeRequisicoesTests.FabricaComLimite factory)
    : IClassFixture<LimiteDeRequisicoesTests.FabricaComLimite>
{
    private const int Cota = 3;

    public class FabricaComLimite : GastraApiFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseSetting("LimiteRequisicoes:Habilitado", "true");
            builder.UseSetting("LimiteRequisicoes:AutenticacaoPorMinuto", Cota.ToString());
            builder.UseSetting("LimiteRequisicoes:ConsultaClientePorMinuto", Cota.ToString());
            builder.UseSetting("Proxy:RedesConfiaveis:0", "203.0.113.0/24");
        }
    }

    private static int _proximoIp;

    /// <summary>Um cliente que chega "pelo nginx" com um IP só dele.</summary>
    private HttpClient ClientePeloProxy(out string ip, string? idioma = null)
    {
        ip = $"198.51.100.{Interlocked.Increment(ref _proximoIp)}";
        var cliente = factory.CreateClient();
        // Um IP inventado pelo navegador vem antes; o nginx acrescenta o de verdade no fim.
        cliente.DefaultRequestHeaders.Add("X-Forwarded-For", $"10.66.66.66, {ip}");
        if (idioma is not null)
            cliente.DefaultRequestHeaders.AcceptLanguage.ParseAdd(idioma);
        return cliente;
    }

    private static Task<HttpResponseMessage> Login(HttpClient cliente) =>
        cliente.PostAsJsonAsync("/api/autenticacao/login", new { email = "ninguem@gastra.test", senha = "qualquer" }, Json);

    [Fact]
    public async Task Login_AlemDaCotaDoMinuto_Retorna429ComMensagemEEspera()
    {
        var cliente = ClientePeloProxy(out _);
        for (var i = 0; i < Cota; i++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await Login(cliente)).StatusCode);

        var excedente = await Login(cliente);

        Assert.Equal(HttpStatusCode.TooManyRequests, excedente.StatusCode);
        Assert.Equal(["Muitas requisições em pouco tempo. Aguarde um minuto e tente de novo."],
            (await excedente.Content.ReadFromJsonAsync<ErroResponse>(Json))!.Erros);
        Assert.True(excedente.Headers.RetryAfter?.Delta > TimeSpan.Zero);
    }

    [Fact]
    public async Task Login_AlemDaCota_RespondeNoIdiomaPedido()
    {
        var cliente = ClientePeloProxy(out _, idioma: "en");
        for (var i = 0; i < Cota; i++)
            await Login(cliente);

        var excedente = await Login(cliente);

        Assert.Equal(["Too many requests in a short time. Wait a minute and try again."],
            (await excedente.Content.ReadFromJsonAsync<ErroResponse>(Json))!.Erros);
    }

    [Fact]
    public async Task Cota_EPorIp_OutroEnderecoNaoEAfetado()
    {
        var esgotado = ClientePeloProxy(out _);
        for (var i = 0; i <= Cota; i++)
            await Login(esgotado);

        var outro = ClientePeloProxy(out _);

        Assert.Equal(HttpStatusCode.Unauthorized, (await Login(outro)).StatusCode);
    }

    [Fact]
    public async Task ConsultaDoCliente_AlemDaCota_Retorna429()
    {
        var cliente = ClientePeloProxy(out _);
        for (var i = 0; i < Cota; i++)
            Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync($"/api/comandas/consulta/palpite{i}")).StatusCode);

        var excedente = await cliente.GetAsync("/api/comandas/consulta/mais-um-palpite");

        Assert.Equal(HttpStatusCode.TooManyRequests, excedente.StatusCode);
    }

    [Fact]
    public async Task RotasComLogin_NaoEntramNoLimite()
    {
        var cliente = ClientePeloProxy(out _);
        Autenticar(cliente, await factory.TokenGarcom(ClientePeloProxy(out _)));

        // Comanda inexistente: 404 todas as vezes, nunca 429.
        for (var i = 0; i <= Cota; i++)
            Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync("/api/comandas/999999")).StatusCode);
    }

    [Fact]
    public async Task Auditoria_AtrasDoProxy_RegistraOIpDoNavegador()
    {
        var email = $"garcom-{Guid.NewGuid():N}@gastra.test";
        var garcom = await factory.CriarUsuario(email, PapelUsuario.Garcom);
        var cliente = ClientePeloProxy(out var ip);

        await factory.Login(cliente, email);

        var registro = (await factory.Auditoria(EventoAuditoria.LoginSucesso)).Last(r => r.UsuarioId == garcom.Id);
        // O último endereço do cabeçalho, o que o nginx escreveu; o 10.66.66.66 inventado é ignorado.
        Assert.Equal(ip, registro.Ip);
    }
}
