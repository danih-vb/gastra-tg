namespace Gastra.Communication.Requests;

/// <summary>Endereço devolvido pelo envio de foto (POST /api/cardapio/fotos). Nulo tira a foto.</summary>
public class FotoUsuarioRequest
{
    public string? Foto { get; set; }
}
