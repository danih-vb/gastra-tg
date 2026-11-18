using Gastra.Communication.Enums;

namespace Gastra.Communication.Responses;

public class RestricaoAlimentarResponse
{
    public int Id { get; set; }
    public CategoriaRestricao Categoria { get; set; }
    public string? ObservacaoLivre { get; set; }
}
