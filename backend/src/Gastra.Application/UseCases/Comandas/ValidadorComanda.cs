using Gastra.Communication.Enums;
using Gastra.Communication.Requests;
using Gastra.Domain.Entidades;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

/// <summary>Valida o formato dos dados antes de chegar à entidade; as regras de estado ficam na Comanda.</summary>
public static class ValidadorComanda
{
    public static void ValidarAbertura(AbrirComandaRequest request)
    {
        if (request.QuantidadePessoas < 1)
            throw new ErroValidacaoException([MensagensErro.QuantidadePessoasInvalida]);
    }

    public static void ValidarComposicao(ComposicaoRequest request)
    {
        var erros = new List<string>();

        if (request.QuantidadePessoas < 1)
            erros.Add(MensagensErro.QuantidadePessoasInvalida);

        if (!Enum.IsDefined(request.Composicao))
            erros.Add(MensagensErro.ComposicaoInvalida);

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }

    public static void ValidarItem(ItemPedidoRequest request)
    {
        if (request.Quantidade < 1)
            throw new ErroValidacaoException([MensagensErro.QuantidadeItemInvalida]);
    }

    public const int QuantidadeMaximaDeCortesia = 20;
    public const decimal PrecoMaximo = 99_999.99m;

    public static void ValidarCortesia(CortesiaRequest request)
    {
        var erros = new List<string>();

        if (request.Quantidade is < 1 or > QuantidadeMaximaDeCortesia)
            erros.Add(MensagensErro.QuantidadeCortesiaInvalida);

        ValidarMotivo(request.Motivo, erros);

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }

    public static void ValidarAlteracaoDePreco(AlterarPrecoItemRequest request)
    {
        var erros = new List<string>();

        if (request.Preco is < 0 or > PrecoMaximo || decimal.Round(request.Preco, 2) != request.Preco)
            erros.Add(MensagensErro.PrecoAjustadoInvalido);

        ValidarMotivo(request.Motivo, erros);

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }

    private static void ValidarMotivo(string? motivo, List<string> erros)
    {
        var limpo = motivo?.Trim() ?? string.Empty;
        if (limpo.Length == 0)
            erros.Add(MensagensErro.MotivoDoAjusteObrigatorio);
        else if (limpo.Length > ItemDoPedido.TamanhoMaximoDoMotivo)
            erros.Add(MensagensErro.MotivoDoAjusteMuitoLongo);
    }

    /// <summary>RF25: nota dentro da escala e comentário curto — texto longo do cliente não serve a ninguém.</summary>
    public static void ValidarAvaliacao(AvaliacaoRequest request)
    {
        var erros = new List<string>();

        if (request.Nota is < AvaliacaoAtendimento.NotaMinima or > AvaliacaoAtendimento.NotaMaxima)
            erros.Add(MensagensErro.NotaAvaliacaoInvalida);

        if (request.Comentario?.Trim().Length > AvaliacaoAtendimento.TamanhoMaximoDoComentario)
            erros.Add(MensagensErro.ComentarioMuitoLongo);

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }

    public static void ValidarRestricao(RestricaoAlimentarRequest request)
    {
        var erros = new List<string>();

        if (!Enum.IsDefined(request.Categoria))
            erros.Add(MensagensErro.CategoriaRestricaoInvalida);

        if (request.ObservacaoLivre?.Length > 200)
            erros.Add(MensagensErro.ObservacaoMuitoLonga);

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }

    public static void ValidarSituacao(SituacaoItemRequest request)
    {
        var erros = new List<string>();

        if (request.Situacao is not (StatusItemPedido.Entregue or StatusItemPedido.Cancelado))
            erros.Add(MensagensErro.SituacaoItemInvalida);

        if (request.Situacao == StatusItemPedido.Cancelado)
        {
            if (request.MotivoCancelamento is null)
                erros.Add(MensagensErro.MotivoCancelamentoObrigatorio);
            else if (!Enum.IsDefined(request.MotivoCancelamento.Value))
                erros.Add(MensagensErro.MotivoCancelamentoInvalido);
        }

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }
}
