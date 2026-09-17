using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

// Consulta da comanda pelo salão (garçom, metre, coordenador, gerente)
public interface IObterComandaUseCase
{
    Task<ComandaResponse> Executar(int comandaId);
}

public class ObterComandaUseCase(
    IRepositorioComanda repositorio,
    IRepositorioItemCardapio repositorioCardapio,
    IRepositorioUsuario repositorioUsuario,
    IUsuarioLogado usuarioLogado)
    : IObterComandaUseCase
{
    public async Task<ComandaResponse> Executar(int comandaId)
    {
        var comanda = await repositorio.ObterPorId(comandaId)
                      ?? throw new NaoEncontradoException(MensagensErro.ComandaNaoEncontrada);

        var nomes = await LeitorDeNomesDoCardapio.Obter(repositorioCardapio, comanda);
        var garcom = await LeitorDeNomesDeGarcons.Obter(repositorioUsuario, comanda);
        return MapeadorComanda.Montar(comanda, nomes, usuarioLogado.ObterPapel(), garcom);
    }
}
