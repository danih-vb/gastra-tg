using Gastra.Application.Auditoria;
using Gastra.Application.Mapeamento;
using Gastra.Application.UseCases.Alocacoes;
using Gastra.Application.UseCases.Auditoria;
using Gastra.Application.UseCases.Autenticacao;
using Gastra.Application.UseCases.Cardapio;
using Gastra.Application.UseCases.Comandas;
using Gastra.Application.UseCases.Indicadores;
using Gastra.Application.UseCases.Promocoes;
using Gastra.Application.UseCases.Salao;
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
        services.AddScoped<IRegistradorAuditoria, RegistradorAuditoria>();
        services.AddScoped<IEliminarAuditoriaVencidaUseCase, EliminarAuditoriaVencidaUseCase>();

        services.AddScoped<IGerarSugestaoAlocacaoUseCase, GerarSugestaoAlocacaoUseCase>();
        services.AddScoped<IAjustarAlocacaoUseCase, AjustarAlocacaoUseCase>();
        services.AddScoped<IListarGarconsDoTurnoUseCase, ListarGarconsDoTurnoUseCase>();
        services.AddScoped<IConfirmarAlocacaoUseCase, ConfirmarAlocacaoUseCase>();
        services.AddScoped<IObterAlocacaoTurnoUseCase, ObterAlocacaoTurnoUseCase>();

        services.AddScoped<IRelatorioGarconsUseCase, RelatorioGarconsUseCase>();
        services.AddScoped<IRelatorioPracasUseCase, RelatorioPracasUseCase>();
        services.AddScoped<IRelatorioCardapioUseCase, RelatorioCardapioUseCase>();
        services.AddScoped<IRelatorioHorariosUseCase, RelatorioHorariosUseCase>();
        services.AddScoped<IRankingDesempenhoUseCase, RankingDesempenhoUseCase>();

        services.AddScoped<ICriarPromocaoUseCase, CriarPromocaoUseCase>();
        services.AddScoped<IListarPromocoesUseCase, ListarPromocoesUseCase>();
        services.AddScoped<IRemoverPromocaoUseCase, RemoverPromocaoUseCase>();

        return services;
    }

    private static void AddCasosDeUso(IServiceCollection services)
    {
        services.AddScoped<ICadastrarItemCardapioUseCase, CadastrarItemCardapioUseCase>();
        services.AddScoped<IListarItensCardapioUseCase, ListarItensCardapioUseCase>();
        services.AddScoped<IObterItemCardapioUseCase, ObterItemCardapioUseCase>();
        services.AddScoped<IAtualizarPrecoItemUseCase, AtualizarPrecoItemUseCase>();
        services.AddScoped<IAlterarDisponibilidadeItemUseCase, AlterarDisponibilidadeItemUseCase>();
        services.AddScoped<IAlterarImagemItemUseCase, AlterarImagemItemUseCase>();

        services.AddScoped<IAutenticarUseCase, AutenticarUseCase>();
        services.AddScoped<IConfigurarSegundoFatorUseCase, ConfigurarSegundoFatorUseCase>();
        services.AddScoped<IConfirmarSegundoFatorUseCase, ConfirmarSegundoFatorUseCase>();
        services.AddScoped<IEncerrarSessaoUseCase, EncerrarSessaoUseCase>();

        services.AddScoped<ICadastrarUsuarioUseCase, CadastrarUsuarioUseCase>();
        services.AddScoped<IListarUsuariosUseCase, ListarUsuariosUseCase>();
        services.AddScoped<IObterUsuarioUseCase, ObterUsuarioUseCase>();
        services.AddScoped<IEditarUsuarioUseCase, EditarUsuarioUseCase>();
        services.AddScoped<IAlterarSituacaoUsuarioUseCase, AlterarSituacaoUsuarioUseCase>();
        services.AddScoped<IRedefinirSenhaUsuarioUseCase, RedefinirSenhaUsuarioUseCase>();
        services.AddScoped<IReiniciarSegundoFatorUseCase, ReiniciarSegundoFatorUseCase>();

        services.AddScoped<IAbrirComandaUseCase, AbrirComandaUseCase>();
        services.AddScoped<IConfirmarComposicaoUseCase, ConfirmarComposicaoUseCase>();
        services.AddScoped<IRegistrarItemPedidoUseCase, RegistrarItemPedidoUseCase>();
        services.AddScoped<IAtualizarSituacaoItemUseCase, AtualizarSituacaoItemUseCase>();
        services.AddScoped<IRegistrarRestricaoUseCase, RegistrarRestricaoUseCase>();
        services.AddScoped<IAvaliarAtendimentoUseCase, AvaliarAtendimentoUseCase>();
        services.AddScoped<IRelatorioAvaliacoesUseCase, RelatorioAvaliacoesUseCase>();
        services.AddScoped<IRemoverTaxaServicoUseCase, RemoverTaxaServicoUseCase>();
        services.AddScoped<IFecharComandaUseCase, FecharComandaUseCase>();
        services.AddScoped<IObterComandaUseCase, ObterComandaUseCase>();
        services.AddScoped<IListarComandasAbertasUseCase, ListarComandasAbertasUseCase>();
        services.AddScoped<IConsultarComandaClienteUseCase, ConsultarComandaClienteUseCase>();
        services.AddScoped<ISugerirCombinacoesUseCase, SugerirCombinacoesUseCase>();

        services.AddScoped<ICadastrarPracaUseCase, CadastrarPracaUseCase>();
        services.AddScoped<IEditarPracaUseCase, EditarPracaUseCase>();
        services.AddScoped<IListarPracasUseCase, ListarPracasUseCase>();
        services.AddScoped<ICadastrarMesaUseCase, CadastrarMesaUseCase>();
        services.AddScoped<IEditarMesaUseCase, EditarMesaUseCase>();
        services.AddScoped<IListarMesasUseCase, ListarMesasUseCase>();
    }
}
