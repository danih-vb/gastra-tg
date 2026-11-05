using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

// Painel do salão: o que está aberto agora
public interface IListarComandasAbertasUseCase
{
    Task<List<ComandaResponse>> Executar();
}

public class ListarComandasAbertasUseCase(
    IRepositorioComanda repositorio,
    IRepositorioItemCardapio repositorioCardapio,
    IRepositorioUsuario repositorioUsuario,
    IUsuarioLogado usuarioLogado) : IListarComandasAbertasUseCase
{
    public async Task<List<ComandaResponse>> Executar()
    {
        var comandas = await repositorio.ListarAbertas();
        var respostas = new List<ComandaResponse>();
        var papel = usuarioLogado.ObterPapel();
        var garcons = await LeitorDeNomesDeGarcons.Obter(repositorioUsuario, comandas);

        foreach (var comanda in comandas)
        {
            var nomes = await LeitorDeNomesDoCardapio.Obter(repositorioCardapio, comanda);
            respostas.Add(MapeadorComanda.Montar(
                comanda, nomes, papel, garcons.TryGetValue(comanda.GarcomId, out var nome) ? nome : string.Empty));
        }

        return respostas;
    }
}
