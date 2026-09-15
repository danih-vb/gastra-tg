using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using OtpNet;

namespace Gastra.Api.Tests.Infraestrutura;

/// <summary>
/// Sobe a API completa para os testes, trocando o MySQL por um banco em memória exclusivo de cada
/// instância da fábrica, com uma chave JWT só de teste.
/// </summary>
public class GastraApiFactory : WebApplicationFactory<Program>
{
    public const string SenhaPadrao = "Senha-de-teste-123";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _nomeBanco = $"gastra-testes-{Guid.NewGuid()}";
    private string? _tokenGerente;

    /// <summary>Id do Gerente dono do token de <see cref="TokenGerente"/>.</summary>
    public int IdGerente { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testes");
        builder.UseSetting("Jwt:ChaveAssinatura", "chave-exclusiva-dos-testes-de-integracao-do-gastra");

        builder.ConfigureServices(services =>
        {
            var configuracaoMySql = services
                .Where(s => s.ServiceType == typeof(DbContextOptions<GastraDbContext>)
                            || s.ServiceType == typeof(IDbContextOptionsConfiguration<GastraDbContext>))
                .ToList();

            foreach (var servico in configuracaoMySql)
                services.Remove(servico);

            services.AddDbContext<GastraDbContext>(opcoes => opcoes.UseInMemoryDatabase(_nomeBanco));
        });
    }

    public async Task<Usuario> CriarUsuario(string email, PapelUsuario papel, bool ativo = true, string senha = SenhaPadrao)
    {
        using var escopo = Services.CreateScope();
        var criptografia = escopo.ServiceProvider.GetRequiredService<ICriptografiaSenha>();
        var usuario = new Usuario($"Usuário {papel}", email, criptografia.GerarHash(senha), papel);

        if (!ativo)
            usuario.Inativar();

        await escopo.ServiceProvider.GetRequiredService<IRepositorioUsuario>().Adicionar(usuario);
        await escopo.ServiceProvider.GetRequiredService<IUnitOfWork>().Commit();
        return usuario;
    }

    /// <summary>Login de papel sem segundo fator (Garçom, Metre): devolve o token de acesso.</summary>
    public async Task<string> Login(HttpClient cliente, string email, string senha = SenhaPadrao)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/autenticacao/login", new { email, senha }, Json);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<LoginResponse>(Json))!.TokenAcesso!;
    }

    /// <summary>
    /// Fluxo completo de um Gerente: senha, configuração do autenticador e confirmação do código
    /// calculado a partir da chave, como o app do celular faria.
    /// </summary>
    public async Task<string> TokenGerente(HttpClient cliente)
    {
        if (_tokenGerente is not null)
            return _tokenGerente;

        var email = $"gerente-{Guid.NewGuid():N}@gastra.test";
        IdGerente = (await CriarUsuario(email, PapelUsuario.Gerente)).Id;

        var login = await (await cliente.PostAsJsonAsync("/api/autenticacao/login", new { email, senha = SenhaPadrao }, Json))
            .Content.ReadFromJsonAsync<LoginResponse>(Json);

        var configuracao = await (await cliente.PostAsJsonAsync("/api/autenticacao/segundo-fator/configurar",
                new { tokenSegundoFator = login!.TokenSegundoFator }, Json))
            .Content.ReadFromJsonAsync<ConfiguracaoSegundoFatorResponse>(Json);

        var confirmacao = await cliente.PostAsJsonAsync("/api/autenticacao/segundo-fator/confirmar",
            new { tokenSegundoFator = login.TokenSegundoFator, codigo = CodigoTotp(configuracao!.ChaveManual) }, Json);
        confirmacao.EnsureSuccessStatusCode();

        _tokenGerente = (await confirmacao.Content.ReadFromJsonAsync<LoginResponse>(Json))!.TokenAcesso!;
        return _tokenGerente;
    }

    public static string CodigoTotp(string chaveBase32) => new Totp(Base32Encoding.ToBytes(chaveBase32)).ComputeTotp();

    public static void Autenticar(HttpClient cliente, string token) =>
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
