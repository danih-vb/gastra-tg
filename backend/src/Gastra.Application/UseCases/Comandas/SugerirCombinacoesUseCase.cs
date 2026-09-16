using Gastra.Communication.Responses;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Servicos;
using Mapster;
using ComunicacaoEnums = Gastra.Communication.Enums;

namespace Gastra.Application.UseCases.Comandas;

// UC18 — sugestão de combinação de pratos para o garçom (RF09)
public interface ISugerirCombinacoesUseCase
{
    Task<SugestoesComandaResponse> Executar(int comandaId);
}

public class SugerirCombinacoesUseCase(
    IRepositorioComanda repositorio,
    IRepositorioItemCardapio repositorioCardapio,
    IServicoAnalitico servicoAnalitico) : ISugerirCombinacoesUseCase
{
    public const int LimiteDeSugestoes = 3;

    public async Task<SugestoesComandaResponse> Executar(int comandaId)
    {
        var comanda = await BuscadorDeComanda.Aberta(repositorio, comandaId);

        var resposta = new SugestoesComandaResponse
        {
            ServicoDisponivel = true,
            ConfirmarRestricaoComCliente = comanda.Restricoes.Any(r => !r.VerificavelPeloCardapio),
        };

        // Só padrão de consumo observável entra no cálculo: os itens pedidos, nunca dado do cliente (RN05).
        var pedidos = comanda.Itens
            .Where(i => i.Status != StatusItemPedido.Cancelado)
            .Select(i => i.ItemDoCardapioId)
            .Distinct()
            .ToList();

        // Sem nada pedido ainda, não há o que combinar: nem chama o serviço.
        if (pedidos.Count == 0)
            return resposta;

        // O backend decide o que pode ser oferecido; o Python só ordena dentro disso.
        var restricoes = comanda.Restricoes.Select(r => r.Categoria).ToList();
        var permitidos = (await repositorioCardapio.ListarDisponiveis())
            .Where(item => !pedidos.Contains(item.Id))
            .Where(item => restricoes.All(item.AtendeRestricao))
            .ToDictionary(item => item.Id);

        if (permitidos.Count == 0)
            return resposta;

        IReadOnlyList<int> sugeridos;
        try
        {
            sugeridos = await servicoAnalitico.SugerirCombinacoes(pedidos, permitidos.Keys, LimiteDeSugestoes);
        }
        catch (ServicoAnaliticoIndisponivelException)
        {
            // Já registrado no log pela infraestrutura. A comanda não depende do Python (D3, RNF05).
            resposta.ServicoDisponivel = false;
            return resposta;
        }

        // Confere de novo: um id fora da lista permitida nunca chega à tela, mesmo que o serviço erre.
        resposta.Itens = sugeridos
            .Where(permitidos.ContainsKey)
            .Distinct()
            .Take(LimiteDeSugestoes)
            .Select(id => permitidos[id])
            .Select(item => new ItemSugeridoResponse
            {
                ItemDoCardapioId = item.Id,
                Nome = item.Nome,
                Categoria = item.Categoria.Adapt<ComunicacaoEnums.CategoriaItemCardapio>(),
                Preco = item.Preco,
            })
            .ToList();

        return resposta;
    }
}
