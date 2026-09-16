using Gastra.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Gastra.Api.Tests.BancoDeDados;

/// <summary>
/// Teste que só roda com um MySQL de verdade: views e triggers não existem no banco em memória.
/// Sem a variável <c>GASTRA_TESTES_MYSQL</c>, o teste aparece como ignorado (e não como aprovado).
/// </summary>
public sealed class FactComMySqlAttribute : FactAttribute
{
    public FactComMySqlAttribute()
    {
        if (string.IsNullOrWhiteSpace(BancoMySqlDeTeste.ConnectionStringDoServidor))
            Skip = $"Defina {BancoMySqlDeTeste.VariavelDeAmbiente} para rodar os testes no MySQL.";
    }
}

/// <summary>
/// Cria um schema descartável, aplica todas as migrations e o apaga no fim. O usuário da variável
/// precisa poder criar e apagar schemas (no ambiente local, o root do contêiner).
/// </summary>
public sealed class BancoMySqlDeTeste : IAsyncLifetime
{
    public const string VariavelDeAmbiente = "GASTRA_TESTES_MYSQL";

    public static string? ConnectionStringDoServidor => Environment.GetEnvironmentVariable(VariavelDeAmbiente);

    private readonly string _schema = $"gastra_teste_{Guid.NewGuid():N}"[..28];

    public string ConnectionString => new MySqlConnectionStringBuilder(ConnectionStringDoServidor!)
    {
        Database = _schema,
    }.ConnectionString;

    public GastraDbContext CriarContexto() => new(new DbContextOptionsBuilder<GastraDbContext>()
        .UseMySql(ConnectionString, new MySqlServerVersion(new Version(8, 4, 0)))
        .Options);

    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(ConnectionStringDoServidor))
            return;

        await using var contexto = CriarContexto();
        await contexto.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (string.IsNullOrWhiteSpace(ConnectionStringDoServidor))
            return;

        await using var contexto = CriarContexto();
        await contexto.Database.EnsureDeletedAsync();
    }

    public async Task Executar(string sql)
    {
        await using var conexao = new MySqlConnection(ConnectionString);
        await conexao.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        await comando.ExecuteNonQueryAsync();
    }

    /// <summary>Lê o resultado como lista de dicionários coluna → valor.</summary>
    public async Task<List<Dictionary<string, object?>>> Consultar(string sql)
    {
        await using var conexao = new MySqlConnection(ConnectionString);
        await conexao.OpenAsync();
        await using var comando = new MySqlCommand(sql, conexao);
        await using var leitor = await comando.ExecuteReaderAsync();

        var linhas = new List<Dictionary<string, object?>>();
        while (await leitor.ReadAsync())
        {
            var linha = new Dictionary<string, object?>();
            for (var i = 0; i < leitor.FieldCount; i++)
                linha[leitor.GetName(i)] = leitor.IsDBNull(i) ? null : leitor.GetValue(i);
            linhas.Add(linha);
        }

        return linhas;
    }
}
