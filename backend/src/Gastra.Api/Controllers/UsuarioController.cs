using Gastra.Application.UseCases.Usuarios;
using Gastra.Communication.Enums;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gastra.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
// RF18: só o Gerente gerencia contas.
[Authorize(Roles = nameof(PapelUsuario.Gerente))]
public class UsuarioController : ControllerBase
{
    /// <summary>UC04 — Cadastrar conta de usuário com o papel correspondente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarUsuarioRequest request,
        [FromServices] ICadastrarUsuarioUseCase useCase)
    {
        var resposta = await useCase.Executar(request);
        return CreatedAtAction(nameof(Obter), new { id = resposta.Id }, resposta);
    }

    /// <summary>UC04 — Listar contas, ativas e inativas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromServices] IListarUsuariosUseCase useCase)
    {
        return Ok(await useCase.Executar());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(int id, [FromServices] IObterUsuarioUseCase useCase)
    {
        return Ok(await useCase.Executar(id));
    }

    /// <summary>UC04 — Editar nome, e-mail e papel.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Editar(
        int id,
        [FromBody] EditarUsuarioRequest request,
        [FromServices] IEditarUsuarioUseCase useCase)
    {
        await useCase.Executar(id, request);
        return NoContent();
    }

    /// <summary>UC04 — Inativar (soft delete) ou reativar a conta.</summary>
    [HttpPatch("{id:int}/situacao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AlterarSituacao(
        int id,
        [FromBody] SituacaoUsuarioRequest request,
        [FromServices] IAlterarSituacaoUsuarioUseCase useCase)
    {
        await useCase.Executar(id, request);
        return NoContent();
    }
}
