using Gastra.Application.UseCases.Salao;
using Gastra.Communication.Enums;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gastra.Api.Controllers;

/// <summary>
/// UC24 — Configuração do salão: praças e mesas (RF23). O cadastro é do Gerente; a leitura vale para
/// todo mundo do salão, porque o garçom precisa da lista de mesas para abrir comanda.
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
public class SalaoController : ControllerBase
{
    private const string Gerente = nameof(PapelUsuario.Gerente);

    [Authorize(Roles = Gerente)]
    [HttpPost("pracas")]
    [ProducesResponseType(typeof(PracaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CadastrarPraca(
        [FromBody] PracaRequest request,
        [FromServices] ICadastrarPracaUseCase useCase)
    {
        var resposta = await useCase.Executar(request);
        return CreatedAtAction(nameof(ListarPracas), new { }, resposta);
    }

    [Authorize(Roles = Gerente)]
    [HttpPut("pracas/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> EditarPraca(
        int id,
        [FromBody] PracaRequest request,
        [FromServices] IEditarPracaUseCase useCase)
    {
        await useCase.Executar(id, request);
        return NoContent();
    }

    /// <summary>Praças do salão, com quantos garçons cada uma comporta (entrada da alocação, RN03).</summary>
    [HttpGet("pracas")]
    [ProducesResponseType(typeof(List<PracaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPracas([FromServices] IListarPracasUseCase useCase)
    {
        return Ok(await useCase.Executar());
    }

    [Authorize(Roles = Gerente)]
    [HttpPost("mesas")]
    [ProducesResponseType(typeof(MesaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CadastrarMesa(
        [FromBody] CadastrarMesaRequest request,
        [FromServices] ICadastrarMesaUseCase useCase)
    {
        var resposta = await useCase.Executar(request);
        return CreatedAtAction(nameof(ListarMesas), new { }, resposta);
    }

    /// <summary>Edita número e capacidade. A praça não muda: o vínculo é fixo (REL01).</summary>
    [Authorize(Roles = Gerente)]
    [HttpPut("mesas/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> EditarMesa(
        int id,
        [FromBody] EditarMesaRequest request,
        [FromServices] IEditarMesaUseCase useCase)
    {
        await useCase.Executar(id, request);
        return NoContent();
    }

    /// <summary>Mesas do salão: é a lista que o garçom usa para abrir a comanda (UC10).</summary>
    [HttpGet("mesas")]
    [ProducesResponseType(typeof(List<MesaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarMesas([FromServices] IListarMesasUseCase useCase)
    {
        return Ok(await useCase.Executar());
    }
}
