namespace Gastra.Domain.Servicos;

/// <summary>Um item que caracteriza o perfil: aparece em <c>Presenca</c> das comandas dele, <c>Destaque</c> vezes mais que no restaurante.</summary>
public record ItemDoPerfil(int ItemId, double Presenca, double Destaque);

/// <summary>Um grupo de comandas com consumo parecido, encontrado pela clusterização (RF09).</summary>
public record PerfilDeConsumo(int Id, int Comandas, double Participacao, IReadOnlyList<ItemDoPerfil> Itens);

/// <summary>
/// Os perfis de consumo que o serviço analítico encontrou. <c>HistoricoReal</c> é falso quando o serviço ainda usa o
/// histórico simulado: aí os ids são os do simulador, e não do cardápio, e os perfis não descrevem o restaurante.
/// </summary>
public record PerfisDeConsumo(
    IReadOnlyList<PerfilDeConsumo> Perfis,
    double? Silhueta,
    bool SegmentaARecomendacao,
    int ComandasAnalisadas,
    bool HistoricoReal);
