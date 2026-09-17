namespace Gastra.Communication.Requests;

/// <summary>UC22 — a praça escolhida pelo Metre para o garçom.</summary>
public class AjusteAlocacaoRequest
{
    public int PracaId { get; set; }

    /// <summary>
    /// Opcional: garçom da praça escolhida que troca de lugar com este. É o jeito de ajustar quando a praça está cheia,
    /// o caso comum no turno em que há tantos garçons quanto vagas (#140). O outro vai para a praça de origem deste,
    /// ou fica sem praça se este ainda não tinha uma.
    /// </summary>
    public int? TrocarComGarcomId { get; set; }
}
