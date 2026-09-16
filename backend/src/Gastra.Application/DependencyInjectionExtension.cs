using Gastra.Application.Mapeamento;
using Gastra.Application.UseCases.Autenticacao;
using Gastra.Application.UseCases.Cardapio;
using Gastra.Application.UseCases.Comandas;
using Gastra.Application.UseCases.Usuarios;
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

        services.AddScoped<IAutenticarUseCase, AutenticarUseCase>();
        services.AddScoped<IConfigurarSegundoFatorUseCase, ConfigurarSegundoFatorUseCase>();
        services.AddScoped<IConfirmarSegundoFatorUseCase, ConfirmarSegundoFatorUseCase>();
        services.AddScoped<IEncerrarSessaoUseCase, EncerrarSessaoUseCase>();

        services.AddScoped<ICadastrarUsuarioUseCase, CadastrarUsuarioUseCase>();
        services.AddScoped<IListarUsuariosUseCase, ListarUsuariosUseCase>();
        services.AddScoped<IObterUsuarioUseCase, ObterUsuarioUseCase>();
        services.AddScoped<IEditarUsuarioUseCase, EditarUsuarioUseCase>();
        services.AddScoped<IAlterarSituacaoUsuarioUseCase, AlterarSituacaoUsuarioUseCase>();

        services.AddScoped<IAbrirComandaUseCase, AbrirComandaUseCase>();
        services.AddScoped<IConfirmarComposicaoUseCase, ConfirmarComposicaoUseCase>();
        services.AddScoped<IRegistrarItemPedidoUseCase, RegistrarItemPedidoUseCase>();
        services.AddScoped<IAtualizarSituacaoItemUseCase, AtualizarSituacaoItemUseCase>();
        services.AddScoped<IRegistrarRestricaoUseCase, RegistrarRestricaoUseCase>();
        services.AddScoped<IRemoverTaxaServicoUseCase, RemoverTaxaServicoUseCase>();
        services.AddScoped<IFecharComandaUseCase, FecharComandaUseCase>();
        services.AddScoped<IObterComandaUseCase, ObterComandaUseCase>();
        services.AddScoped<IListarComandasAbertasUseCase, ListarComandasAbertasUseCase>();
        services.AddScoped<IConsultarComandaClienteUseCase, ConsultarComandaClienteUseCase>();
    }
}
