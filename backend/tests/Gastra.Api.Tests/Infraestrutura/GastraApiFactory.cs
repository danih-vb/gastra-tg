using Gastra.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Gastra.Api.Tests.Infraestrutura;

/// <summary>
/// Sobe a API completa para os testes, trocando o MySQL por um banco em memória exclusivo
/// de cada instância da fábrica.
/// </summary>
public class GastraApiFactory : WebApplicationFactory<Program>
{
    private readonly string _nomeBanco = $"gastra-testes-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testes");

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
}
