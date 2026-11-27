using Gastra.Application.UseCases.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gastra.Api.Controllers;

/// <summary>
/// Fotos dos funcionários, fora do UsuarioController porque lá tudo exige Gerente, e o crachá no cabeçalho é de
/// todos os papéis. Diferente das fotos do cardápio, nada aqui é público: a foto de uma pessoa é dado pessoal (LGPD).
/// </summary>
[ApiController]
[Route("api/usuarios/fotos")]
[Authorize]
public class FotoDoUsuarioController : ControllerBase
{
    /// <summary>Foto do crachá: o Gerente vê a de qualquer conta; os demais, só a própria.</summary>
    [HttpGet("{nome}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(string nome, [FromServices] IObterFotoUsuarioUseCase useCase)
    {
        var foto = await useCase.Executar(nome);
        if (foto is null)
            return NotFound();

        // A tela busca a foto com o token e guarda na memória da página; nada fica no cache do navegador, que num
        // computador compartilhado do salão outra pessoa abriria depois.
        Response.Headers.CacheControl = "private, no-store";
        return ArquivoDeFoto.Responder(this, foto, nome);
    }
}
