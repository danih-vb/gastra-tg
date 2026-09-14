using Microsoft.Extensions.DependencyInjection;

namespace Gastra.Application;

public static class DependencyInjectionExtension
{
    /// <summary>
    /// Registra os serviços da camada de aplicação (casos de uso, validadores, mapeamentos).
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
