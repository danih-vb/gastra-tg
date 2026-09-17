using Gastra.Application.UseCases.Promocoes;
using Gastra.Communication.Enums;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gastra.Api.Controllers;

/// <summary>Promoções do cardápio (RF22): mesma permissão da gestão do cardápio.</summary>
[ApiController]
[Route("api/promocoes")]
[Authorize(Roles = nameof(PapelUsuario.Gerente) + "," + nameof(PapelUsuario.Coordenador))]
public class PromocaoController : ControllerBase
{
    /// <summary>UC08 — Criar promoção para um ou mais itens.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PromocaoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar([FromBody] PromocaoRequest request, [FromServices] ICriarPromocaoUseCase useCase)
    {
        var resposta = await useCase.Executar(request);
        return Created($"api/promocoes/{resposta.Id}", resposta);
    }

    /// <summary>Promoções cadastradas. Por padrão só as ativas; <c>?todas=true</c> inclui as removidas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PromocaoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] bool todas, [FromServices] IListarPromocoesUseCase useCase)
    {
        return Ok(await useCase.Executar(somenteAtivas: !todas));
    }

    /// <summary>UC09 — Remover promoção. Ela deixa de valer, mas fica no histórico.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Remover(int id, [FromServices] IRemoverPromocaoUseCase useCase)
    {
        await useCase.Executar(id);
        return NoContent();
    }
}
