using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Mapster;
using ComunicacaoEnums = Gastra.Communication.Enums;

namespace Gastra.Application.UseCases.Comandas;

/// <summary>
/// Monta as respostas da comanda. É explícito (e não só Mapster) porque o nome de cada item vem do
/// cardápio, e a resposta do cliente mostra menos coisas que a do garçom (RN04).
/// </summary>
internal static class MapeadorComanda
{
    public static ComandaResponse Montar(Comanda comanda, IReadOnlyDictionary<int, string> nomesDosItens) => new()
    {
        Id = comanda.Id,
        MesaId = comanda.MesaId,
        Status = comanda.Status.Adapt<ComunicacaoEnums.StatusComanda>(),
        QuantidadePessoas = comanda.QuantidadePessoas,
        Composicao = comanda.Composicao.Adapt<ComunicacaoEnums.ComposicaoMesa>(),
        ComposicaoAjustadaManualmente = comanda.ComposicaoAjustadaManualmente,
        TaxaServicoRemovida = comanda.TaxaServicoRemovida,
        CodigoAcessoCliente = comanda.CodigoAcessoCliente,
        DataHoraAbertura = comanda.DataHoraAbertura,
        DataHoraFechamento = comanda.DataHoraFechamento,
        Itens = comanda.Itens.Select(i => MontarItem(i, nomesDosItens)).ToList(),
        Restricoes = comanda.Restricoes.Select(r => new RestricaoAlimentarResponse
        {
            Id = r.Id,
            Categoria = r.Categoria.Adapt<ComunicacaoEnums.CategoriaRestricao>(),
            ObservacaoLivre = r.ObservacaoLivre,
        }).ToList(),
        Subtotal = comanda.CalcularSubtotal(),
        TaxaServico = comanda.CalcularTaxaServico(),
        Total = comanda.CalcularTotal(),
    };

    public static ItemPedidoResponse MontarItem(ItemDoPedido item, IReadOnlyDictionary<int, string> nomes) => new()
    {
        Id = item.Id,
        ItemDoCardapioId = item.ItemDoCardapioId,
        Nome = nomes.TryGetValue(item.ItemDoCardapioId, out var nome) ? nome : string.Empty,
        Quantidade = item.Quantidade,
        PrecoUnitarioNoMomento = item.PrecoUnitarioNoMomento,
        Valor = item.CalcularValor(),
        Status = item.Status.Adapt<ComunicacaoEnums.StatusItemPedido>(),
        MotivoCancelamento = item.MotivoCancelamento?.Adapt<ComunicacaoEnums.MotivoCancelamento>(),
        DataHoraRegistro = item.DataHoraRegistro,
    };

    public static ComandaClienteResponse MontarParaCliente(
        Comanda comanda, string numeroDaMesa, IReadOnlyDictionary<int, string> nomes) => new()
    {
        Mesa = numeroDaMesa,
        DataHoraAbertura = comanda.DataHoraAbertura,
        Fechada = comanda.DataHoraFechamento is not null,
        Itens = comanda.Itens.Select(i => new ItemConsultaCliente
        {
            Nome = nomes.TryGetValue(i.ItemDoCardapioId, out var nome) ? nome : string.Empty,
            Quantidade = i.Quantidade,
            Valor = i.CalcularValor(),
            Situacao = i.Status.ToString(),
        }).ToList(),
        Subtotal = comanda.CalcularSubtotal(),
        TaxaServico = comanda.CalcularTaxaServico(),
        Total = comanda.CalcularTotal(),
    };
}
