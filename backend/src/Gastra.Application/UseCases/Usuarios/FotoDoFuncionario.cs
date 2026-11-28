namespace Gastra.Application.UseCases.Usuarios;

/// <summary>Endereço da foto de um funcionário: a rota que só atende quem está logado.</summary>
public static class FotoDoFuncionario
{
    public const string Prefixo = "/api/usuarios/fotos/";

    public static string Endereco(string nome) => Prefixo + nome;

    /// <summary>Nome do arquivo guardado por trás do endereço; nulo se não for uma foto de funcionário.</summary>
    public static string? Nome(string? endereco) =>
        endereco is not null && endereco.StartsWith(Prefixo, StringComparison.Ordinal) ? endereco[Prefixo.Length..] : null;
}
