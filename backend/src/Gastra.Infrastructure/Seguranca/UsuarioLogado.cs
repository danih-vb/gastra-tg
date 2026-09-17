using System.Security.Claims;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

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

    public (int Id, PapelUsuario Papel)? ObterIdentificacao()
    {
        var usuario = httpContextAccessor.HttpContext?.User;
        var id = usuario?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? usuario?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(id, out var idUsuario) || !Enum.TryParse<PapelUsuario>(usuario?.FindFirstValue(ClaimTypes.Role), out var papel))
            return null;

        return (idUsuario, papel);
    }

    public PapelUsuario ObterPapel()
    {
        var papel = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

        return Enum.TryParse<PapelUsuario>(papel, out var resultado)
            ? resultado
            : throw new InvalidOperationException("Requisição sem papel no token.");
    }
}
