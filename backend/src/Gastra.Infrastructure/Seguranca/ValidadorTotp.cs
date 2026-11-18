using System.Security.Cryptography;
using Gastra.Domain.Seguranca;
using Microsoft.AspNetCore.DataProtection;
using OtpNet;

namespace Gastra.Infrastructure.Seguranca;

/// <summary>
/// TOTP (RFC 6238, 6 dígitos, período de 30 s). O segredo é criptografado com o Data Protection do
/// ASP.NET Core antes de ir para o banco: quem ler a tabela não consegue gerar códigos.
/// </summary>
public class ValidadorTotp(IDataProtectionProvider dataProtection) : IValidadorTotp
{
    private const string Emissor = "GASTRA";
    private readonly IDataProtector _protetor = dataProtection.CreateProtector("Gastra.SegredoTotp");

    public SegredoTotpGerado GerarSegredo(string email)
    {
        var segredoBase32 = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
        var uri = $"otpauth://totp/{Emissor}:{Uri.EscapeDataString(email)}" +
                  $"?secret={segredoBase32}&issuer={Emissor}&digits=6&period=30";

        return new SegredoTotpGerado(_protetor.Protect(segredoBase32), uri, segredoBase32);
    }

    public bool Validar(string segredoProtegido, string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Length != 6 || !codigo.All(char.IsDigit))
            return false;

        string segredoBase32;
        try
        {
            segredoBase32 = _protetor.Unprotect(segredoProtegido);
        }
        catch (CryptographicException)
        {
            return false;
        }

        var totp = new Totp(Base32Encoding.ToBytes(segredoBase32), step: 30, totpSize: 6);

        // Aceita o período anterior e o seguinte para tolerar pequena diferença de relógio do celular.
        return totp.VerifyTotp(codigo, out _, new VerificationWindow(previous: 1, future: 1));
    }
}
