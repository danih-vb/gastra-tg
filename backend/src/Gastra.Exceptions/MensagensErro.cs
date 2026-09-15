using System.Globalization;
using System.Resources;

namespace Gastra.Exceptions;

/// <summary>
/// Mensagens de erro traduzidas. O idioma vem de <see cref="CultureInfo.CurrentUICulture"/>, definido
/// por requisição a partir do cabeçalho Accept-Language: MensagensErro.resx (pt-BR, padrão) ou
/// MensagensErro.en.resx (inglês).
/// </summary>
public static class MensagensErro
{
    private static readonly ResourceManager Recursos =
        new("Gastra.Exceptions.MensagensErro", typeof(MensagensErro).Assembly);

    public static string NomeObrigatorio => Obter(nameof(NomeObrigatorio));
    public static string NomeMuitoLongo => Obter(nameof(NomeMuitoLongo));
    public static string DescricaoMuitoLonga => Obter(nameof(DescricaoMuitoLonga));
    public static string PrecoInvalido => Obter(nameof(PrecoInvalido));
    public static string CategoriaInvalida => Obter(nameof(CategoriaInvalida));
    public static string FlagDieteticaInvalida => Obter(nameof(FlagDieteticaInvalida));
    public static string ItemCardapioNaoEncontrado => Obter(nameof(ItemCardapioNaoEncontrado));
    public static string RequisicaoInvalida => Obter(nameof(RequisicaoInvalida));
    public static string ErroInesperado => Obter(nameof(ErroInesperado));

    private static string Obter(string chave) =>
        Recursos.GetString(chave, CultureInfo.CurrentUICulture) ?? chave;
}
