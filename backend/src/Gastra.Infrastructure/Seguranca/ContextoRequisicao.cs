using System.Diagnostics;
using Gastra.Domain.Seguranca;
using Microsoft.AspNetCore.Http;

namespace Gastra.Infrastructure.Seguranca;

public class ContextoRequisicao(IHttpContextAccessor httpContextAccessor) : IContextoRequisicao
{
    public string? ObterIp() => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    // O mesmo identificador que o ASP.NET Core usa no log técnico da requisição.
    public string? ObterIdCorrelacao() =>
        Activity.Current?.TraceId.ToString() ?? httpContextAccessor.HttpContext?.TraceIdentifier;
}
