namespace Gastra.Domain.Seguranca;

public interface IValidadorTotp
{
    /// <summary>
    /// Gera o segredo do app autenticador. Devolve o segredo já protegido (para gravar) e os dados
    /// para configurar o app (exibidos uma única vez — RN07).
    /// </summary>
    SegredoTotpGerado GerarSegredo(string email);

    /// <summary>RN07: código TOTP no padrão RFC 6238, período de 30 segundos.</summary>
    bool Validar(string segredoProtegido, string codigo);
}

/// <param name="SegredoProtegido">Segredo criptografado, para gravar no usuário.</param>
/// <param name="UriConfiguracao">URI otpauth:// usada para gerar o QR code.</param>
/// <param name="ChaveManual">Mesmo segredo em texto, para digitar no app sem QR code.</param>
public record SegredoTotpGerado(string SegredoProtegido, string UriConfiguracao, string ChaveManual);
