using Gastra.Communication.Enums;

namespace Gastra.Communication.Requests;

public class RestricaoAlimentarRequest
{
    public CategoriaRestricao Categoria { get; set; }

    /// <summary>Detalhe informado pelo cliente; apagado no fechamento da comanda.</summary>
    public string? ObservacaoLivre { get; set; }
}
