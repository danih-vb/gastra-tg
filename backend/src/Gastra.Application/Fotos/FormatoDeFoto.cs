namespace Gastra.Application.Fotos;

/// <summary>Regras de formato comuns às fotos do cardápio e às dos funcionários.</summary>
public static class FormatoDeFoto
{
    public const int TamanhoMaximo = 5 * 1024 * 1024;

    /// <summary>
    /// Extensão da foto, ou nulo se o arquivo não for JPG, PNG ou WEBP dentro do tamanho. O tipo vem dos primeiros
    /// bytes do arquivo, e não do nome nem do que o navegador declara, que qualquer um troca. SVG fica de fora de
    /// propósito: é um formato que pode carregar script, e a foto vai para o navegador.
    /// </summary>
    public static string? Extensao(byte[] c)
    {
        if (c.Length > TamanhoMaximo)
            return null;
        if (c.Length > 3 && c[0] == 0xFF && c[1] == 0xD8 && c[2] == 0xFF)
            return "jpg";
        if (c.Length > 8 && c[0] == 0x89 && c[1] == 0x50 && c[2] == 0x4E && c[3] == 0x47
            && c[4] == 0x0D && c[5] == 0x0A && c[6] == 0x1A && c[7] == 0x0A)
            return "png";
        if (c.Length > 12 && c[0] == 0x52 && c[1] == 0x49 && c[2] == 0x46 && c[3] == 0x46
            && c[8] == 0x57 && c[9] == 0x45 && c[10] == 0x42 && c[11] == 0x50)
            return "webp";
        return null;
    }
}
