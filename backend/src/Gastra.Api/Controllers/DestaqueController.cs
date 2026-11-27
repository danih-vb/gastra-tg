using Gastra.Application.UseCases.Destaques;
using Gastra.Communication.Enums;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gastra.Api.Controllers;

/// <summary>
/// Destaques do dia e metas do mês. Quem cuida do cardápio cria e remove; o garçom e o Metre leem, para ver o que
/// a casa quer vender e quanto a equipe já vendeu.
/// </summary>
[ApiController]
[Route("api/destaques")]
[Authorize]
public class DestaqueController : ControllerBase
{
    private const string Gestao = nameof(PapelUsuario.Gerente) + "," + nameof(PapelUsuario.Coordenador);
    private const string Todos = Gestao + "," + nameof(PapelUsuario.Garcom) + "," + nameof(PapelUsuario.Metre);

    [Authorize(Roles = Todos)]
    [HttpGet]
    [ProducesResponseType(typeof(List<DestaqueResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromServices] IListarDestaquesUseCase useCase) => Ok(await useCase.Executar());

    [Authorize(Roles = Gestao)]
    [HttpPost]
    [ProducesResponseType(typeof(DestaqueResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar([FromBody] DestaqueRequest request, [FromServices] ICriarDestaqueUseCase useCase)
    {
        var resposta = await useCase.Executar(request);
        return Created($"api/destaques/{resposta.Id}", resposta);
    }

    [Authorize(Roles = Gestao)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, [FromServices] IRemoverDestaqueUseCase useCase)
    {
        await useCase.Executar(id);
        return NoContent();
    }
}
