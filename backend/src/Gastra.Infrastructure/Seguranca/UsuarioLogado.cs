using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Gastra.Infrastructure.Seguranca;

public class UsuarioLogado(IHttpContextAccessor httpContextAccessor, IRepositorioUsuario repositorio) : IUsuarioLogado
{
    public async Task<Usuario> Obter()
    {
        var usuario = httpContextAccessor.HttpContext?.User;
        var id = usuario?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? usuario?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(id, out var idUsuario))
            throw new InvalidOperationException("Requisição sem usuário autenticado.");

        return await repositorio.ObterPorId(idUsuario)
               ?? throw new InvalidOperationException("Usuário do token não existe.");
    }
}
