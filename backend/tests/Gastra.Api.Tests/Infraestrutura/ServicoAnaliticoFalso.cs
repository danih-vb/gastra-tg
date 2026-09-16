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

    public void Reiniciar()
    {
        Chamadas.Clear();
        Indisponivel = false;
        Responder = null;
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
