using Gastra.Communication.Requests;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Salao;

public static class ValidadorSalao
{
    public static void ValidarPraca(PracaRequest request)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Codigo))
            erros.Add(MensagensErro.CodigoPracaObrigatorio);
        else if (request.Codigo.Trim().Length > 20)
            erros.Add(MensagensErro.CodigoPracaMuitoLongo);

        if (request.QuantidadeGarcons < 1)
            erros.Add(MensagensErro.QuantidadeGarconsInvalida);

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }

    public static void ValidarMesa(string numero, int capacidade)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(numero))
            erros.Add(MensagensErro.NumeroMesaObrigatorio);
        else if (numero.Trim().Length > 10)
            erros.Add(MensagensErro.NumeroMesaMuitoLongo);

        if (capacidade < 1)
            erros.Add(MensagensErro.CapacidadeMesaInvalida);

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }
}
