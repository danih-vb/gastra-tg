using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Gastra.Infrastructure.Armazenamento;

/// <summary>Uma pasta de fotos no disco. O nome do arquivo é sempre gerado aqui, nunca o que veio do usuário.</summary>
public abstract partial class FotosEmDisco(string diretorio, ILogger logger)
{
    public async Task<string> Salvar(byte[] conteudo, string extensao)
    {
        Directory.CreateDirectory(diretorio);
        var nome = $"{Guid.NewGuid():N}.{extensao}";
        await File.WriteAllBytesAsync(Path.Combine(diretorio, nome), conteudo);
        return nome;
    }

    public Stream? Abrir(string nome)
    {
        var caminho = Caminho(nome);
        return caminho is not null && File.Exists(caminho) ? File.OpenRead(caminho) : null;
    }

    public void Apagar(string nome)
    {
        var caminho = Caminho(nome);
        if (caminho is null)
            return;

        // Quem chama já gravou a mudança no banco: se o disco falhar, a requisição não deve falhar junto. Fica no
        // log para alguém apagar à mão.
        try
        {
            File.Delete(caminho);
        }
        catch (Exception excecao) when (excecao is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(excecao, "Não foi possível apagar a foto {Nome}.", nome);
        }
    }

    // Só o formato que Salvar gera: é o que impede pedir "../appsettings.json" pela URL.
    private string? Caminho(string nome) => NomeDeFoto().IsMatch(nome) ? Path.Combine(diretorio, nome) : null;

    [GeneratedRegex(@"^[0-9a-f]{32}\.(jpg|png|webp)$")]
    private static partial Regex NomeDeFoto();
}
