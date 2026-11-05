namespace Gastra.Communication.Responses;

/// <summary>Dados para vincular o app autenticador. Exibidos uma única vez (RN07).</summary>
public class ConfiguracaoSegundoFatorResponse
{
    /// <summary>URI otpauth:// para gerar o QR code.</summary>
    public string UriConfiguracao { get; set; } = string.Empty;

    /// <summary>Chave para digitar no app, caso não seja possível ler o QR code.</summary>
    public string ChaveManual { get; set; } = string.Empty;
}
