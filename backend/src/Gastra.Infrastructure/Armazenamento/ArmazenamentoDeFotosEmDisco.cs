using System.Text.RegularExpressions;
using Gastra.Domain.Servicos;
using Microsoft.Extensions.Configuration;

namespace Gastra.Infrastructure.Armazenamento;

public partial class ArmazenamentoDeFotosEmDisco(IConfiguration configuration) : IArmazenamentoDeFotos
{
    private readonly string _diretorio = configuration["Fotos:Diretorio"] ?? Path.Combine(AppContext.BaseDirectory, "fotos");

    public async Task<string> Salvar(byte[] conteudo, string extensao)
    {
        Directory.CreateDirectory(_diretorio);
        var nome = $"{Guid.NewGuid():N}.{extensao}";
        await File.WriteAllBytesAsync(Path.Combine(_diretorio, nome), conteudo);
        return nome;
    }

    public Stream? Abrir(string nome)
    {
        // Só o formato que Salvar gera: é o que impede pedir "../appsettings.json" pela URL.
        if (!NomeDeFoto().IsMatch(nome))
            return null;

        var caminho = Path.Combine(_diretorio, nome);
        return File.Exists(caminho) ? File.OpenRead(caminho) : null;
    }

    [GeneratedRegex("^[0-9a-f]{32}\\.(jpg|png|webp)$")]
    private static partial Regex NomeDeFoto();
}
