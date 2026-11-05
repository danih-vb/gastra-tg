namespace Gastra.Communication.Responses;

/// <summary>Formato único de erro devolvido pela API.</summary>
public class ErroResponse
{
    public List<string> Erros { get; set; } = [];

    // Construtor sem parâmetros: necessário para desserializar o JSON em clientes .NET.
    public ErroResponse()
    {
    }

    public ErroResponse(List<string> erros) => Erros = erros;

    public ErroResponse(string erro) => Erros = [erro];
}
