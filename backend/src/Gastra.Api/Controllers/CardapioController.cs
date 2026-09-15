using Gastra.Application.UseCases.Cardapio;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Gastra.Api.Controllers;

[ApiController]
[Route("api/cardapio")]
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

    /// <summary>Lista os itens; com <c>somenteDisponiveis=true</c>, atende o cardápio digital (UC19).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ItemCardapioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] bool somenteDisponiveis,
        [FromServices] IListarItensCardapioUseCase useCase)
    {
        return Ok(await useCase.Executar(somenteDisponiveis));
    }

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
