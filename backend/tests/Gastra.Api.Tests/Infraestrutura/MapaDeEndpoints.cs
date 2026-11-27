using System.Reflection;
using Gastra.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Gastra.Api.Tests.Infraestrutura;

/// <summary>Um endpoint da API e quem pode chamá-lo, lido dos atributos do controller e da ação.</summary>
public sealed record RotaDaApi(string Metodo, string Rota, string Controller, string Acao, bool Publico,
    IReadOnlySet<string>? Papeis)
{
    /// <summary>Exige login, mas vale para qualquer papel: <c>[Authorize]</c> sem <c>Roles</c>.</summary>
    public bool QualquerUsuarioLogado => !Publico && Papeis is null;

    /// <summary>A rota com valores de exemplo no lugar dos parâmetros, para chamar de verdade.</summary>
    public string RotaDeExemplo =>
        "/" + System.Text.RegularExpressions.Regex.Replace(Rota, @"\{([a-zA-Z]+)(:[^}]*)?\}", m => m.Groups[1].Value switch
        {
            "nome" => "0123456789abcdef0123456789abcdef.png",
            "codigoAcesso" => "codigo-que-nao-existe",
            _ => "999999",
        });

    public override string ToString() => $"{Metodo} /{Rota}";
}

/// <summary>
/// Todos os endpoints dos controllers, por reflection. Os atributos se somam como no ASP.NET Core: um
/// <c>[Authorize(Roles = ...)]</c> no controller e outro na ação exigem os dois, então os papéis são a interseção.
/// </summary>
public static class MapaDeEndpoints
{
    public static readonly string[] TodosOsPapeis = ["Gerente", "Coordenador", "Metre", "Garcom"];

    public static IReadOnlyList<RotaDaApi> Todos { get; } = Ler();

    private static List<RotaDaApi> Ler()
    {
        var controllers = typeof(AutenticacaoController).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

        var lista = new List<RotaDaApi>();
        foreach (var controller in controllers)
        {
            var prefixo = controller.GetCustomAttribute<RouteAttribute>()?.Template ?? string.Empty;
            foreach (var acao in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                foreach (var http in acao.GetCustomAttributes<HttpMethodAttribute>())
                {
                    var rota = string.IsNullOrEmpty(http.Template) ? prefixo : $"{prefixo}/{http.Template}";
                    var anonimo = acao.GetCustomAttribute<AllowAnonymousAttribute>() is not null
                                  || controller.GetCustomAttribute<AllowAnonymousAttribute>() is not null;
                    var autorizacoes = controller.GetCustomAttributes<AuthorizeAttribute>()
                        .Concat(acao.GetCustomAttributes<AuthorizeAttribute>()).ToList();

                    IReadOnlySet<string>? papeis = null;
                    foreach (var exigencia in autorizacoes.Where(a => !string.IsNullOrWhiteSpace(a.Roles)))
                    {
                        var destes = exigencia.Roles!.Split(',', StringSplitOptions.TrimEntries).ToHashSet();
                        papeis = papeis is null ? destes : papeis.Intersect(destes).ToHashSet();
                    }

                    // Sem [Authorize] nenhum e sem [AllowAnonymous], o endpoint seria público por omissão: fica com papéis
                    // vazios, que marcam "sem política", para o teste pegar.
                    var publico = anonimo;
                    lista.Add(new RotaDaApi(http.HttpMethods.Single(), rota, controller.Name, acao.Name, publico,
                        autorizacoes.Count == 0 && !anonimo ? new HashSet<string>() : papeis));
                }
            }
        }

        return lista.OrderBy(e => e.Rota, StringComparer.Ordinal).ThenBy(e => e.Metodo).ToList();
    }

    /// <summary>Endpoints sem <c>[Authorize]</c> nem <c>[AllowAnonymous]</c> (marcados com papéis vazios).</summary>
    public static IEnumerable<RotaDaApi> SemPolitica => Todos.Where(e => !e.Publico && e.Papeis is { Count: 0 });
}
