namespace Gastra.Communication.Requests;

/// <summary>
/// Avaliação do atendimento deixada pelo cliente (RF25). Não tem campo de identificação, e é de
/// propósito: a avaliação é anônima (RN08).
/// </summary>
public class AvaliacaoRequest
{
    /// <summary>De 1 a 5.</summary>
    public int Nota { get; set; }

    /// <summary>Opcional, no máximo 280 caracteres.</summary>
    public string? Comentario { get; set; }
}
