using Gastra.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace GastraSemeador;

/// <summary>
/// O que o semeador precisa saber para rodar, vindo do ambiente. Nada de senha no código: a senha
/// inicial das contas fictícias sai do <c>infra/.env</c>, que não é versionado.
/// </summary>
internal sealed record Configuracao(string Conexao, string SenhaPadrao, int Semente)
{
    private const string VariavelAmbiente = "GASTRA_AMBIENTE";
    private const string VariavelConexao = "GASTRA_SEMEADOR_CONEXAO";
    private const string VariavelSenha = "GASTRA_SEMEADOR_SENHA";

    /// <summary>
    /// Lê o ambiente e recusa rodar fora de Development. O semeador escreve dados fictícios em cima do
    /// banco; um engano apontando para um banco de verdade não teria volta, então a checagem vem antes
    /// de qualquer coisa. Devolve <c>null</c> quando falta alguma peça, já tendo explicado o motivo.
    /// </summary>
    public static Configuracao? Ler(string[] argumentos, TextWriter saida)
    {
        var ambiente = Environment.GetEnvironmentVariable(VariavelAmbiente);
        if (!string.Equals(ambiente, "Development", StringComparison.OrdinalIgnoreCase))
        {
            saida.WriteLine($"O semeador só roda com {VariavelAmbiente}=Development (encontrado: '{ambiente ?? "vazio"}').");
            saida.WriteLine("Ele prepara a máquina de desenvolvimento, e não um banco real.");
            return null;
        }

        var conexao = Environment.GetEnvironmentVariable(VariavelConexao);
        if (string.IsNullOrWhiteSpace(conexao))
        {
            saida.WriteLine($"Defina {VariavelConexao} com a conexão do banco de desenvolvimento.");
            saida.WriteLine("Exemplo: Server=localhost;Port=3307;Database=gastra_dev;User ID=gastra_app;Password=...");
            return null;
        }

        var senha = Environment.GetEnvironmentVariable(VariavelSenha);
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8)
        {
            saida.WriteLine($"Defina {VariavelSenha} com a senha inicial das contas fictícias (8 caracteres ou mais).");
            return null;
        }

        return new Configuracao(conexao, senha, LerSemente(argumentos));
    }

    /// <summary>A mesma semente gera exatamente os mesmos dados; 42 é a usada na calibração da RN03.</summary>
    private static int LerSemente(string[] argumentos)
    {
        var indice = Array.IndexOf(argumentos, "--semente");
        return indice >= 0 && indice + 1 < argumentos.Length && int.TryParse(argumentos[indice + 1], out var valor)
            ? valor
            : 42;
    }

    public GastraDbContext AbrirContexto()
    {
        // Versão fixa em vez de AutoDetect, como na API: uma ida a menos ao banco antes de começar.
        var opcoes = new DbContextOptionsBuilder<GastraDbContext>()
            .UseMySql(Conexao, new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;

        return new GastraDbContext(opcoes);
    }
}
