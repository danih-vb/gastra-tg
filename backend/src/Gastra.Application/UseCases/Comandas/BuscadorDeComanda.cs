using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

/// <summary>
/// Busca a comanda e devolve o erro certo: 404 quando não existe e 422 quando já foi fechada. Sem isso,
/// a exceção viria da entidade e o cliente receberia um 500 genérico.
/// </summary>
internal static class BuscadorDeComanda
{
    public static async Task<Comanda> Aberta(IRepositorioComanda repositorio, int id)
    {
        var comanda = await repositorio.ObterPorId(id)
                      ?? throw new NaoEncontradoException(MensagensErro.ComandaNaoEncontrada);

        if (comanda.Status != StatusComanda.Aberta)
            throw new RegraDeNegocioException(MensagensErro.ComandaNaoEstaAberta);

        return comanda;
    }
}
