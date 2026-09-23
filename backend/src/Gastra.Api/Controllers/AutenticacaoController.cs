using Gastra.Api.Configuracao;
using Gastra.Application.UseCases.Autenticacao;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Gastra.Api.Controllers;

[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController : ControllerBase
{
    /// <summary>UC01 — Autenticar-se com e-mail e senha.</summary>
    [HttpPost("login")]
    [EnableRateLimiting(LimiteDeRequisicoes.Autenticacao)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromServices] IAutenticarUseCase useCase)
    {
        return Ok(await useCase.Executar(request));
    }

    /// <summary>Vincula o app autenticador no primeiro acesso de Gerente/Coordenador (RN07).</summary>
    [HttpPost("segundo-fator/configurar")]
    [EnableRateLimiting(LimiteDeRequisicoes.Autenticacao)]
    [ProducesResponseType(typeof(ConfiguracaoSegundoFatorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConfigurarSegundoFator(
        [FromBody] SegundoFatorRequest request,
        [FromServices] IConfigurarSegundoFatorUseCase useCase)
    {
        return Ok(await useCase.Executar(request));
    }

    /// <summary>UC02 — Confirmar segundo fator com o código do app autenticador.</summary>
    [HttpPost("segundo-fator/confirmar")]
    [EnableRateLimiting(LimiteDeRequisicoes.Autenticacao)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConfirmarSegundoFator(
        [FromBody] SegundoFatorRequest request,
        [FromServices] IConfirmarSegundoFatorUseCase useCase)
    {
        return Ok(await useCase.Executar(request));
    }

    /// <summary>UC03 — Encerrar sessão: invalida os tokens de acesso do usuário.</summary>
    [Authorize]
    [HttpPost("logoff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logoff([FromServices] IEncerrarSessaoUseCase useCase)
    {
        await useCase.Executar();
        return NoContent();
    }
}
