using Gastra.Application.Fotos;
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

    /// <summary>UC04 — Consultar uma conta.</summary>
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

    /// <summary>
    /// UC04 — Definir uma senha nova para quem esqueceu a sua. A senha antiga deixa de valer, e as sessões abertas
    /// daquela conta caem na hora.
    /// </summary>
    [HttpPost("{id:int}/senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RedefinirSenha(
        int id,
        [FromBody] RedefinirSenhaRequest request,
        [FromServices] IRedefinirSenhaUsuarioUseCase useCase)
    {
        await useCase.Executar(id, request);
        return NoContent();
    }

    /// <summary>
    /// Pôr ou trocar a foto do crachá do funcionário (JPG, PNG ou WEBP, até 5 MB). A foto anterior é apagada do
    /// servidor, e a nova só é vista por quem está logado (GET /api/usuarios/fotos/{nome}).
    /// </summary>
    [HttpPut("{id:int}/foto")]
    [RequestSizeLimit(FormatoDeFoto.TamanhoMaximo + 1024 * 1024)]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DefinirFoto(
        int id,
        IFormFile? arquivo,
        [FromServices] IDefinirFotoUsuarioUseCase useCase)
    {
        using var memoria = new MemoryStream();
        if (arquivo is not null)
            await arquivo.CopyToAsync(memoria);

        return Ok(await useCase.Executar(id, memoria.ToArray()));
    }

    /// <summary>Tirar a foto do crachá. O arquivo é apagado do servidor.</summary>
    [HttpDelete("{id:int}/foto")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TirarFoto(int id, [FromServices] IDefinirFotoUsuarioUseCase useCase)
    {
        return Ok(await useCase.Executar(id, conteudo: null));
    }

    /// <summary>
    /// UC04 — Desfazer a vinculação do app autenticador de um Gerente ou Coordenador que perdeu o celular. O próximo
    /// login volta a pedir a configuração, com um segredo novo (RN07).
    /// </summary>
    [HttpDelete("{id:int}/segundo-fator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReiniciarSegundoFator(
        int id,
        [FromServices] IReiniciarSegundoFatorUseCase useCase)
    {
        await useCase.Executar(id);
        return NoContent();
    }
}
