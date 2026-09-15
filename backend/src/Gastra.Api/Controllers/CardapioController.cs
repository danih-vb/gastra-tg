using Gastra.Application.UseCases.Cardapio;
using Gastra.Communication.Requests;
using Gastra.Communication.Enums;
using Gastra.Communication.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gastra.Api.Controllers;

[ApiController]
[Route("api/cardapio")]
// RF19–RF21: gestão do cardápio só para Gerente e Coordenador.
[Authorize(Roles = nameof(PapelUsuario.Gerente) + "," + nameof(PapelUsuario.Coordenador))]
public class CardapioController : ControllerBase
{
    /// <summary>UC05 — Cadastrar item do cardápio.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ItemCardapioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] ItemCardapioRequest request,
        [FromServices] ICadastrarItemCardapioUseCase useCase)
    {
        var resposta = await useCase.Executar(request);
        return CreatedAtAction(nameof(Obter), new { id = resposta.Id }, resposta);
    }

    /// <summary>Gestão do cardápio: todos os itens, disponíveis ou não.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ItemCardapioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromServices] IListarItensCardapioUseCase useCase)
    {
        return Ok(await useCase.Executar(somenteDisponiveis: false));
    }

    /// <summary>UC19 — Cardápio digital: público (o cliente não faz login), só itens disponíveis.</summary>
    [AllowAnonymous]
    [HttpGet("digital")]
    [ProducesResponseType(typeof(List<ItemCardapioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarCardapioDigital([FromServices] IListarItensCardapioUseCase useCase)
    {
        return Ok(await useCase.Executar(somenteDisponiveis: true));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ItemCardapioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(int id, [FromServices] IObterItemCardapioUseCase useCase)
    {
        return Ok(await useCase.Executar(id));
    }

    /// <summary>UC06 — Atualizar preço de item.</summary>
    [HttpPatch("{id:int}/preco")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarPreco(
        int id,
        [FromBody] AtualizarPrecoRequest request,
        [FromServices] IAtualizarPrecoItemUseCase useCase)
    {
        await useCase.Executar(id, request);
        return NoContent();
    }

    /// <summary>UC07 — Marcar item disponível/indisponível.</summary>
    [HttpPatch("{id:int}/disponibilidade")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarDisponibilidade(
        int id,
        [FromBody] DisponibilidadeRequest request,
        [FromServices] IAlterarDisponibilidadeItemUseCase useCase)
    {
        await useCase.Executar(id, request);
        return NoContent();
    }
}
