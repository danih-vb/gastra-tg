namespace Gastra.Domain.Servicos;

/// <summary>
/// A camada analítica não respondeu a tempo ou respondeu algo inutilizável. Quem chama decide como
/// seguir sem ela: o núcleo de comandas nunca depende do Python (decisão D3, RNF05).
/// </summary>
public class ServicoAnaliticoIndisponivelException(string motivo, Exception? causa = null)
    : Exception($"Serviço analítico indisponível: {motivo}", causa);
