using Microsoft.AspNetCore.Mvc;

namespace Gastra.Api.Controllers;

/// <summary>Resposta comum às fotos do cardápio e às dos funcionários.</summary>
internal static class ArquivoDeFoto
{
    public static FileStreamResult Responder(ControllerBase controller, Stream foto, string nome)
    {
        var tipo = Path.GetExtension(nome) switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg",
        };
        controller.Response.Headers.XContentTypeOptions = "nosniff";
        return controller.File(foto, tipo);
    }
}
