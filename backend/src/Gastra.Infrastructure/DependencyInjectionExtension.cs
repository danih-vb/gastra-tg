using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Infrastructure.DataAccess;
using Gastra.Infrastructure.DataAccess.Repositorios;
using Gastra.Infrastructure.Seguranca;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gastra.Infrastructure;

public static class DependencyInjectionExtension
{
    /// <summary>
    /// Registra os serviços da camada de infraestrutura (contexto do banco, repositórios, segurança).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext(services, configuration);
        AddRepositorios(services);
        AddSeguranca(services, configuration);

        return services;
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Gastra");

        // Versão fixa em vez de ServerVersion.AutoDetect: o AutoDetect conecta no banco na
        // inicialização, e a API não subiria com o MySQL desligado.
        var serverVersion = new MySqlServerVersion(new Version(8, 4, 0));

        services.AddDbContext<GastraDbContext>(options => options.UseMySql(connectionString, serverVersion));
    }

    private static void AddRepositorios(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRepositorioItemCardapio, RepositorioItemCardapio>();
        services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
    }

    private static void AddSeguranca(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(OpcoesJwt.Carregar(configuration));
        services.AddSingleton<IGeradorToken, GeradorToken>();
        services.AddSingleton<ICriptografiaSenha, CriptografiaSenha>();

        services.AddDataProtection().SetApplicationName("Gastra");
        services.AddSingleton<IValidadorTotp, ValidadorTotp>();

        services.AddHttpContextAccessor();
        services.AddScoped<IUsuarioLogado, UsuarioLogado>();
    }
}
