using Microsoft.Extensions.Configuration;

namespace Gastra.Infrastructure.ServicoAnalitico;

/// <summary>Endereço e tempo limite da camada analítica, lidos da seção "ServicoAnalitico" do appsettings.</summary>
public class OpcoesServicoAnalitico
{
    public const string Secao = "ServicoAnalitico";

    public string UrlBase { get; set; } = "http://localhost:8000";

    /// <summary>
    /// Curto de propósito: a sugestão é um extra na tela do garçom e não pode segurar o atendimento (D3).
    /// </summary>
    public int TempoLimiteMilissegundos { get; set; } = 2000;

    public static OpcoesServicoAnalitico Carregar(IConfiguration configuration)
    {
        var opcoes = configuration.GetSection(Secao).Get<OpcoesServicoAnalitico>() ?? new OpcoesServicoAnalitico();

        if (!Uri.TryCreate(opcoes.UrlBase, UriKind.Absolute, out _))
            throw new InvalidOperationException("Configure ServicoAnalitico:UrlBase com uma URL absoluta (ver backend/README.md).");

        if (opcoes.TempoLimiteMilissegundos <= 0)
            throw new InvalidOperationException("ServicoAnalitico:TempoLimiteMilissegundos precisa ser maior que zero.");

        return opcoes;
    }
}
