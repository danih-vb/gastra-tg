using Gastra.Application.Mapeamento;
using Gastra.Application.UseCases.Cardapio;
using Microsoft.Extensions.DependencyInjection;

namespace Gastra.Application;

public static class DependencyInjectionExtension
{
    /// <summary>
    /// Registra os serviços da camada de aplicação (casos de uso, validadores, mapeamentos).
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        MapeamentoConfig.Registrar();
        AddCasosDeUso(services);

        return services;
    }

    private static void AddCasosDeUso(IServiceCollection services)
    {
        services.AddScoped<ICadastrarItemCardapioUseCase, CadastrarItemCardapioUseCase>();
        services.AddScoped<IListarItensCardapioUseCase, ListarItensCardapioUseCase>();
        services.AddScoped<IObterItemCardapioUseCase, ObterItemCardapioUseCase>();
        services.AddScoped<IAtualizarPrecoItemUseCase, AtualizarPrecoItemUseCase>();
        services.AddScoped<IAlterarDisponibilidadeItemUseCase, AlterarDisponibilidadeItemUseCase>();
    }
}
