using Gastra.Domain.Repositorios;
using Gastra.Infrastructure.DataAccess;
using Gastra.Infrastructure.DataAccess.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gastra.Infrastructure;

public static class DependencyInjectionExtension
{
    /// <summary>
    /// Registra os serviços da camada de infraestrutura (contexto do banco, repositórios).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext(services, configuration);
        AddRepositorios(services);

        return services;
    }

    private static void AddRepositorios(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRepositorioItemCardapio, RepositorioItemCardapio>();
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Gastra");

        // Versão fixa em vez de ServerVersion.AutoDetect: o AutoDetect conecta no banco na
        // inicialização, e a API não subiria com o MySQL desligado.
        var serverVersion = new MySqlServerVersion(new Version(8, 4, 0));

        services.AddDbContext<GastraDbContext>(options => options.UseMySql(connectionString, serverVersion));
    }
}
