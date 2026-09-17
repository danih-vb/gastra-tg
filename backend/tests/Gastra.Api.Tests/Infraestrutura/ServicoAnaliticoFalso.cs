using Gastra.Domain.Servicos;

namespace Gastra.Api.Tests.Infraestrutura;

/// <summary>
/// Substitui o Python nos testes da API: responde o que o teste mandar e guarda o que recebeu.
/// </summary>
public class ServicoAnaliticoFalso : IServicoAnalitico
{
    public record Chamada(IReadOnlyCollection<int> ItensPedidos, IReadOnlyCollection<int> ItensPermitidos, int Limite);

    public List<Chamada> Chamadas { get; } = [];

    public bool Indisponivel { get; set; }

    /// <summary>Resposta do "Python". Por padrão, os primeiros permitidos, na ordem recebida.</summary>
    public Func<Chamada, IReadOnlyList<int>>? Responder { get; set; }

    public record ChamadaAlocacao(
        IReadOnlyCollection<GarcomParaAlocacao> Garcons, IReadOnlyCollection<PracaParaAlocacao> Pracas, double PesoDesequilibrio, double PesoEspera);

    public List<ChamadaAlocacao> ChamadasAlocacao { get; } = [];

    /// <summary>Resposta da alocação. Por padrão, distribui os garçons pelas praças em rodízio.</summary>
    public Func<ChamadaAlocacao, IReadOnlyList<DesignacaoSugerida>>? ResponderAlocacao { get; set; }

    public void Reiniciar()
    {
        Chamadas.Clear();
        ChamadasAlocacao.Clear();
        Indisponivel = false;
        Responder = null;
        ResponderAlocacao = null;
    }

    public Task<IReadOnlyList<DesignacaoSugerida>> SugerirAlocacao(
        IReadOnlyCollection<GarcomParaAlocacao> garcons,
        IReadOnlyCollection<PracaParaAlocacao> pracas,
        double pesoDesequilibrio,
        double pesoEspera,
        CancellationToken cancellationToken = default)
    {
        var chamada = new ChamadaAlocacao(garcons.ToList(), pracas.ToList(), pesoDesequilibrio, pesoEspera);
        ChamadasAlocacao.Add(chamada);

        if (Indisponivel)
            throw new ServicoAnaliticoIndisponivelException("simulado no teste");

        var listaPracas = chamada.Pracas.ToList();
        var resposta = ResponderAlocacao?.Invoke(chamada)
                       ?? chamada.Garcons.Select((g, i) => new DesignacaoSugerida(g.GarcomId, listaPracas[i % listaPracas.Count].PracaId)).ToList();
        return Task.FromResult(resposta);
    }

    public Task<IReadOnlyList<int>> SugerirCombinacoes(
        IReadOnlyCollection<int> itensPedidos,
        IReadOnlyCollection<int> itensPermitidos,
        int limite,
        CancellationToken cancellationToken = default)
    {
        var chamada = new Chamada(itensPedidos.ToList(), itensPermitidos.ToList(), limite);
        Chamadas.Add(chamada);

        if (Indisponivel)
            throw new ServicoAnaliticoIndisponivelException("simulado no teste");

        var resposta = Responder?.Invoke(chamada) ?? chamada.ItensPermitidos.Take(limite).ToList();
        return Task.FromResult(resposta);
    }
}
