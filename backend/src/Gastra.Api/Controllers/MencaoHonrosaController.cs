using Gastra.Application.UseCases.Indicadores;
using Gastra.Communication.Enums;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gastra.Api.Controllers;

/// <summary>
/// Menções honrosas: o Gerente dá e remove. O garçom as vê junto das medalhas, em GET api/indicadores/medalhas.
/// </summary>
[ApiController]
[Route("api/mencoes")]
[Authorize(Roles = nameof(PapelUsuario.Gerente))]
public class MencaoHonrosaController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(MencaoHonrosaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Conceder(
        [FromBody] MencaoHonrosaRequest request, [FromServices] IConcederMencaoUseCase useCase)
    {
        var resposta = await useCase.Executar(request);
        return Created($"api/mencoes/{resposta.Id}", resposta);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, [FromServices] IRemoverMencaoUseCase useCase)
    {
        await useCase.Executar(id);
        return NoContent();
    }
}
