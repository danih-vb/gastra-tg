using Gastra.Application.UseCases.Alocacoes;
using Gastra.Communication.Enums;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gastra.Api.Controllers;

/// <summary>Alocação de garçons às praças por turno (RF06, RF07, RF08, RN03).</summary>
[ApiController]
[Route("api/alocacoes")]
[Authorize]
public class AlocacaoController : ControllerBase
{
    // UC15, UC21 e UC22 são do Metre; a consulta do turno é de todo o salão.
    private const string Metre = nameof(PapelUsuario.Metre);

    /// <summary>UC15 — Gerar a sugestão do turno a partir dos garçons presentes.</summary>
    [Authorize(Roles = Metre)]
    [HttpPost("sugestao")]
    [ProducesResponseType(typeof(AlocacaoTurnoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GerarSugestao(
        [FromBody] SugestaoAlocacaoRequest request,
        [FromServices] IGerarSugestaoAlocacaoUseCase useCase)
    {
        return Ok(await useCase.Executar(request));
    }

    /// <summary>UC22 — Colocar um garçom numa praça: ajuste da sugestão ou alocação manual.</summary>
    [Authorize(Roles = Metre)]
    [HttpPut("{data}/{periodo}/garcons/{garcomId:int}")]
    [ProducesResponseType(typeof(AlocacaoTurnoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Ajustar(
        DateOnly data,
        PeriodoAlocacao periodo,
        int garcomId,
        [FromBody] AjusteAlocacaoRequest request,
        [FromServices] IAjustarAlocacaoUseCase useCase)
    {
        return Ok(await useCase.Executar(data, periodo, garcomId, request));
    }

    /// <summary>UC21 — Confirmar o turno. Depois disso, a alocação não muda mais.</summary>
    [Authorize(Roles = Metre)]
    [HttpPost("{data}/{periodo}/confirmacao")]
    [ProducesResponseType(typeof(AlocacaoTurnoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Confirmar(
        DateOnly data,
        PeriodoAlocacao periodo,
        [FromServices] IConfirmarAlocacaoUseCase useCase)
    {
        return Ok(await useCase.Executar(data, periodo));
    }

    /// <summary>Quem está em qual praça no turno.</summary>
    [HttpGet("{data}/{periodo}")]
    [ProducesResponseType(typeof(AlocacaoTurnoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Obter(
        DateOnly data,
        PeriodoAlocacao periodo,
        [FromServices] IObterAlocacaoTurnoUseCase useCase)
    {
        return Ok(await useCase.Executar(data, periodo));
    }
}
