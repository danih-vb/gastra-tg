using Gastra.Communication.Requests;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Cardapio;

/// <summary>
/// Valida o formato dos dados do item do cardápio antes de chegar à entidade.
/// Regras de negócio que dependem do estado ficam na entidade (Domain).
/// </summary>
public static class ValidadorItemCardapio
{
    public static void Validar(ItemCardapioRequest request)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Nome))
            erros.Add(MensagensErro.NomeObrigatorio);
        else if (request.Nome.Length > 100)
            erros.Add(MensagensErro.NomeMuitoLongo);

        if (request.Descricao?.Length > 500)
            erros.Add(MensagensErro.DescricaoMuitoLonga);

        if (request.Preco <= 0)
            erros.Add(MensagensErro.PrecoInvalido);

        if (!Enum.IsDefined(request.Categoria))
            erros.Add(MensagensErro.CategoriaInvalida);

        if (request.FlagsDieteticas.Any(flag => !Enum.IsDefined(flag)))
            erros.Add(MensagensErro.FlagDieteticaInvalida);

        if (!ImagemValida(request.Imagem))
            erros.Add(MensagensErro.ImagemInvalida);

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }

    public static void ValidarImagem(ImagemItemRequest request)
    {
        if (!ImagemValida(request.Imagem))
            throw new ErroValidacaoException([MensagensErro.ImagemInvalida]);
    }

    /// <summary>
    /// A API guarda o endereço da foto, e não o arquivo (decisão da #141). Aceita link externo (http/https) ou
    /// caminho do próprio servidor; qualquer outra coisa (javascript:, data:) fica de fora, porque o endereço vai
    /// direto para o navegador do cliente no cardápio digital.
    /// </summary>
    private static bool ImagemValida(string? imagem)
    {
        if (string.IsNullOrWhiteSpace(imagem))
            return true;

        var valor = imagem.Trim();
        if (valor.Length > 300)
            return false;

        if (valor.StartsWith('/'))
            return !valor.StartsWith("//");

        return Uri.TryCreate(valor, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public static void ValidarPreco(AtualizarPrecoRequest request)
    {
        if (request.Preco <= 0)
            throw new ErroValidacaoException([MensagensErro.PrecoInvalido]);
    }
}
