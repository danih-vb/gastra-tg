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

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }

    public static void ValidarPreco(AtualizarPrecoRequest request)
    {
        if (request.Preco <= 0)
            throw new ErroValidacaoException([MensagensErro.PrecoInvalido]);
    }
}
