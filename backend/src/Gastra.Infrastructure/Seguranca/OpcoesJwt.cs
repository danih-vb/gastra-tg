using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Gastra.Infrastructure.Seguranca;

/// <summary>Configuração dos tokens, lida da seção "Jwt" do appsettings.</summary>
public class OpcoesJwt
{
    public const string Secao = "Jwt";
    public const string SufixoAudienciaSegundoFator = "-segundo-fator";
    public const string ClaimChaveSessao = "sessao";

    public string Emissor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;

    /// <summary>Segredo de assinatura. Nunca versionado: fica no appsettings.Development.json ou em variável de ambiente.</summary>
    public string ChaveAssinatura { get; set; } = string.Empty;

    public int MinutosExpiracao { get; set; } = 480;
    public int MinutosExpiracaoSegundoFator { get; set; } = 5;

    public SymmetricSecurityKey ObterChave() => new(Encoding.UTF8.GetBytes(ChaveAssinatura));

    public static OpcoesJwt Carregar(IConfiguration configuration)
    {
        var opcoes = configuration.GetSection(Secao).Get<OpcoesJwt>() ?? new OpcoesJwt();

        if (string.IsNullOrWhiteSpace(opcoes.ChaveAssinatura) || opcoes.ChaveAssinatura.Length < 32)
            throw new InvalidOperationException(
                "Configure Jwt:ChaveAssinatura com pelo menos 32 caracteres (ver backend/README.md).");

        return opcoes;
    }
}
