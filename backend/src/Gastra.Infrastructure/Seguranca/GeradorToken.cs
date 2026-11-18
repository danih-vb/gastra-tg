using System.Security.Claims;
using Gastra.Domain.Entidades;
using Gastra.Domain.Seguranca;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Gastra.Infrastructure.Seguranca;

/// <summary>
/// Emite os tokens JWT. O token de segundo fator usa uma audiência diferente da do token de acesso:
/// por isso a API o rejeita automaticamente em qualquer endpoint protegido.
/// </summary>
public class GeradorToken(OpcoesJwt opcoes) : IGeradorToken
{
    private readonly JsonWebTokenHandler _handler = new();

    public string GerarTokenAcesso(Usuario usuario) => Gerar(
        usuario,
        opcoes.Audiencia,
        TimeSpan.FromMinutes(opcoes.MinutosExpiracao),
        [
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, usuario.Papel.ToString()),
            new Claim(OpcoesJwt.ClaimChaveSessao, usuario.ChaveSessao.ToString()),
        ]);

    public string GerarTokenSegundoFator(Usuario usuario) => Gerar(
        usuario,
        opcoes.Audiencia + OpcoesJwt.SufixoAudienciaSegundoFator,
        TimeSpan.FromMinutes(opcoes.MinutosExpiracaoSegundoFator),
        []);

    public async Task<int?> ValidarTokenSegundoFator(string token)
    {
        var resultado = await _handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = opcoes.Emissor,
            ValidAudience = opcoes.Audiencia + OpcoesJwt.SufixoAudienciaSegundoFator,
            IssuerSigningKey = opcoes.ObterChave(),
            ClockSkew = TimeSpan.Zero,
        });

        if (!resultado.IsValid)
            return null;

        return int.TryParse(resultado.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id)
            ? id
            : null;
    }

    private string Gerar(Usuario usuario, string audiencia, TimeSpan validade, IEnumerable<Claim> claims)
    {
        var identidade = new ClaimsIdentity(claims);
        identidade.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()));

        return _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = opcoes.Emissor,
            Audience = audiencia,
            Subject = identidade,
            Expires = DateTime.UtcNow.Add(validade),
            SigningCredentials = new SigningCredentials(opcoes.ObterChave(), SecurityAlgorithms.HmacSha256),
        });
    }
}
