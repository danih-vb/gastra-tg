using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Domain.Servicos;
using Gastra.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OtpNet;

namespace Gastra.Api.Tests.Infraestrutura;

/// <summary>
/// Sobe a API completa para os testes, trocando o MySQL por um banco em memória exclusivo de cada
/// instância da fábrica, o Python por um <see cref="ServicoAnaliticoFalso"/> e usando uma chave JWT só de teste.
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

    /// <summary>O "Python" desta fábrica: os testes dizem o que ele responde e conferem o que recebeu.</summary>
    public ServicoAnaliticoFalso ServicoAnalitico { get; } = new();

    /// <summary>Valores das views analíticas nesta fábrica.</summary>
    public IndicadoresFalsos Indicadores { get; } = new();

    /// <summary>Id do Gerente dono do token de <see cref="TokenGerente"/>.</summary>
    public int IdGerente { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testes");
        builder.UseSetting("Jwt:ChaveAssinatura", "chave-exclusiva-dos-testes-de-integracao-do-gastra");
        builder.UseSetting("Auditoria:EliminacaoAutomatica", "false");

        // Todos os testes saem do mesmo IP falso: com limite, uma classe esgotaria a cota da outra.
        // Os testes do limite (LimiteDeRequisicoesTests) religam com uma cota pequena.
        builder.UseSetting("LimiteRequisicoes:Habilitado", "false");

        builder.ConfigureServices(services =>
        {
            var configuracaoMySql = services
                .Where(s => s.ServiceType == typeof(DbContextOptions<GastraDbContext>)
                            || s.ServiceType == typeof(IDbContextOptionsConfiguration<GastraDbContext>))
                .ToList();

            foreach (var servico in configuracaoMySql)
                services.Remove(servico);

            services.AddDbContext<GastraDbContext>(opcoes => opcoes.UseInMemoryDatabase(_nomeBanco));

            // O servidor de testes não tem conexão de rede: um IP fixo permite conferir a auditoria de login.
            services.AddSingleton<IStartupFilter, IpDeTesteStartupFilter>();

            // Nenhum teste da API depende do serviço Python estar no ar.
            services.RemoveAll<IServicoAnalitico>();
            services.AddSingleton<IServicoAnalitico>(ServicoAnalitico);

            // O banco em memória não tem as views analíticas.
            services.RemoveAll<IRepositorioIndicadores>();
            services.AddSingleton<IRepositorioIndicadores>(Indicadores);
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

    /// <summary>Cria uma praça com uma mesa e devolve o id da mesa (pré-requisito das comandas).</summary>
    public async Task<int> CriarMesa(int capacidade = 4)
    {
        using var escopo = Services.CreateScope();
        var praca = new Praca($"P{Guid.NewGuid():N}"[..8], quantidadeGarcons: 2);
        await escopo.ServiceProvider.GetRequiredService<IRepositorioPraca>().Adicionar(praca);
        await escopo.ServiceProvider.GetRequiredService<IUnitOfWork>().Commit();

        var mesa = new Mesa($"M{Guid.NewGuid():N}"[..6], capacidade, praca.Id);
        await escopo.ServiceProvider.GetRequiredService<IRepositorioMesa>().Adicionar(mesa);
        await escopo.ServiceProvider.GetRequiredService<IUnitOfWork>().Commit();
        return mesa.Id;
    }

    /// <summary>Cria um item do cardápio e devolve o id.</summary>
    public async Task<int> CriarItemCardapio(decimal preco = 50m, bool disponivel = true, params FlagDietetica[] flags)
    {
        using var escopo = Services.CreateScope();
        var item = new ItemDoCardapio($"Prato {Guid.NewGuid():N}"[..12], CategoriaItemCardapio.PratoPrincipal,
            preco, "Descrição", flags);

        if (!disponivel)
            item.MarcarDisponibilidade(false);

        await escopo.ServiceProvider.GetRequiredService<IRepositorioItemCardapio>().Adicionar(item);
        await escopo.ServiceProvider.GetRequiredService<IUnitOfWork>().Commit();
        return item.Id;
    }

    /// <summary>Cria um garçom e devolve o token de acesso dele.</summary>
    public async Task<string> TokenGarcom(HttpClient cliente)
    {
        var email = $"garcom-{Guid.NewGuid():N}@gastra.test";
        await CriarUsuario(email, PapelUsuario.Garcom);
        return await Login(cliente, email);
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

    public const string IpDeTeste = "203.0.113.7";

    /// <summary>Registros de auditoria de um evento, do mais antigo para o mais novo.</summary>
    public async Task<List<RegistroAuditoria>> Auditoria(string evento)
    {
        using var escopo = Services.CreateScope();
        return await escopo.ServiceProvider.GetRequiredService<GastraDbContext>().RegistrosAuditoria
            .AsNoTracking().Where(r => r.Evento == evento).OrderBy(r => r.Id).ToListAsync();
    }

    private sealed class IpDeTesteStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use((contexto, proximo) =>
            {
                contexto.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(IpDeTeste);
                return proximo(contexto);
            });
            next(app);
        };
    }

    public static string CodigoTotp(string chaveBase32) => new Totp(Base32Encoding.ToBytes(chaveBase32)).ComputeTotp();

    public static void Autenticar(HttpClient cliente, string token) =>
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
