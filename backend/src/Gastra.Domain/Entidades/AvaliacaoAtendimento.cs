namespace Gastra.Domain.Entidades;

/// <summary>
/// Avaliação que o cliente deixa sobre o atendimento depois de fechar a conta (RF25). Vive presa à
/// comanda: é criada por <see cref="Comanda.Avaliar"/>.
///
/// É <b>anônima por construção</b> (RN08): não guarda nome, contato nem qualquer identificador de quem
/// avaliou — só a nota, o comentário opcional e quando chegou. Quem sentou na mesa não é identificado
/// em lugar nenhum do sistema, e a avaliação não muda isso.
/// </summary>
public class AvaliacaoAtendimento : EntidadeBase
{
    public const int NotaMinima = 1;
    public const int NotaMaxima = 5;
    public const int TamanhoMaximoDoComentario = 280;

    public int ComandaId { get; private set; }
    public int Nota { get; private set; }

    /// <summary>
    /// Texto livre do cliente. Pode vir com dado pessoal sem que ninguém peça — a tela avisa para não
    /// escrever —, por isso é opcional e curto.
    /// </summary>
    public string? Comentario { get; private set; }

    public DateTime DataHoraEnvio { get; private set; }

    // Usado pelo Entity Framework ao ler do banco.
    private AvaliacaoAtendimento()
    {
    }

    internal AvaliacaoAtendimento(int nota, string? comentario)
    {
        if (nota is < NotaMinima or > NotaMaxima)
            throw new ArgumentOutOfRangeException(nameof(nota), $"A nota deve estar entre {NotaMinima} e {NotaMaxima}.");

        var limpo = comentario?.Trim();
        if (limpo?.Length > TamanhoMaximoDoComentario)
            throw new ArgumentOutOfRangeException(nameof(comentario), "O comentário passou do tamanho máximo.");

        Nota = nota;
        Comentario = string.IsNullOrWhiteSpace(limpo) ? null : limpo;
        DataHoraEnvio = DateTime.UtcNow;
    }
}
