using System.Security.Claims;
using Gastra.Communication.Responses;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;
using Gastra.Infrastructure.Seguranca;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Gastra.Api.Configuracao;

public static class AutenticacaoJwt
{
    public static IServiceCollection AddAutenticacaoJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var opcoes = OpcoesJwt.Carregar(configuration);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = opcoes.Emissor,
                    ValidAudience = opcoes.Audiencia,
                    IssuerSigningKey = opcoes.ObterChave(),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                };
                jwt.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ValidarSessao,
                    OnChallenge = async contexto =>
                    {
                        contexto.HandleResponse();
                        contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await contexto.Response.WriteAsJsonAsync(new ErroResponse(MensagensErro.NaoAutenticado));
                    },
                    OnForbidden = async contexto =>
                    {
                        contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await contexto.Response.WriteAsJsonAsync(new ErroResponse(MensagensErro.AcessoNegado));
                    },
                };
            });

        services.AddAuthorization();
        return services;
    }

    /// <summary>
    /// Assinatura e validade corretas não bastam: o usuário precisa continuar ativo e a chave de
    /// sessão do token precisa ser a atual. É o que faz logoff e inativação valerem na hora.
    /// </summary>
    private static async Task ValidarSessao(TokenValidatedContext contexto)
    {
        var id = contexto.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var chaveSessao = contexto.Principal?.FindFirst(OpcoesJwt.ClaimChaveSessao)?.Value;

        var repositorio = contexto.HttpContext.RequestServices.GetRequiredService<IRepositorioUsuario>();
        var usuario = int.TryParse(id, out var idUsuario) ? await repositorio.ObterPorId(idUsuario) : null;

        if (usuario is null || !usuario.Ativo || usuario.ChaveSessao.ToString() != chaveSessao)
            contexto.Fail("Sessão encerrada ou usuário inativo.");
    }
}
