using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;

namespace Gastra.Api.Configuracao;

/// <summary>
/// Cria o primeiro Gerente quando o banco ainda não tem nenhum usuário. Sem isso ninguém consegue
/// entrar para cadastrar os demais (UC04 exige Gerente logado). Os dados vêm da seção
/// "Administrador" da configuração local — nunca versionada.
/// </summary>
public static class InicializadorAdministrador
{
    public static async Task Executar(WebApplication app)
    {
        var secao = app.Configuration.GetSection("Administrador");
        var email = secao["Email"];
        var senha = secao["Senha"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            return;

        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(InicializadorAdministrador));

        try
        {
            using var escopo = app.Services.CreateScope();
            var repositorio = escopo.ServiceProvider.GetRequiredService<IRepositorioUsuario>();

            if (await repositorio.ExisteAlgum())
                return;

            var criptografia = escopo.ServiceProvider.GetRequiredService<ICriptografiaSenha>();
            var nome = secao["Nome"] ?? "Administrador";

            await repositorio.Adicionar(new Usuario(nome, email, criptografia.GerarHash(senha), PapelUsuario.Gerente));
            await escopo.ServiceProvider.GetRequiredService<IUnitOfWork>().Commit();

            logger.LogInformation("Primeiro usuário (Gerente) criado a partir da configuração.");
        }
        catch (Exception excecao)
        {
            logger.LogWarning(excecao, "Não foi possível verificar ou criar o primeiro usuário. O banco está disponível?");
        }
    }
}
