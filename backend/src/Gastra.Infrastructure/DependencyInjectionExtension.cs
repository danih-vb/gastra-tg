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
        return services;
    }
}
