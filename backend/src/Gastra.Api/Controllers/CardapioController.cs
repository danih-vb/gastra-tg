using Gastra.Application.UseCases.Cardapio;
using Gastra.Communication.Requests;
using Gastra.Communication.Enums;
using Gastra.Communication.Responses;
using Gastra.Domain.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gastra.Api.Controllers;

[ApiController]
[Route("api/cardapio")]
// Exigências de papel ficam em cada ação: quando o controller também traz [Authorize] com papéis, o ASP.NET Core
// soma as duas condições, e o garçom seria barrado até na leitura.
[Authorize]
public class CardapioController : ControllerBase
{
    // RF19–RF21: gestão do cardápio só para Gerente e Coordenador.
    private const string Gestao = nameof(PapelUsuario.Gerente) + "," + nameof(PapelUsuario.Coordenador);

    // O garçom lê a lista completa para lançar pedidos e ver o que está indisponível (RF03); não altera nada.
    private const string GestaoOuGarcom = Gestao + "," + nameof(PapelUsuario.Garcom);

    /// <summary>UC05 — Cadastrar item do cardápio.</summary>
    [Authorize(Roles = Gestao)]
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

    /// <summary>Todos os itens, disponíveis ou não: gestão do cardápio e lançamento de pedidos.</summary>
    [Authorize(Roles = GestaoOuGarcom)]
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

    /// <summary>Consultar um item do cardápio (público).</summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ItemCardapioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(int id, [FromServices] IObterItemCardapioUseCase useCase)
    {
        return Ok(await useCase.Executar(id));
    }

    /// <summary>UC06 — Atualizar preço de item.</summary>
    [Authorize(Roles = Gestao)]
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

    /// <summary>RF05 — Definir ou tirar a foto do item mostrada no cardápio digital.</summary>
    [Authorize(Roles = Gestao)]
    [HttpPatch("{id:int}/imagem")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarImagem(
        int id,
        [FromBody] ImagemItemRequest request,
        [FromServices] IAlterarImagemItemUseCase useCase)
    {
        await useCase.Executar(id, request);
        return NoContent();
    }

    /// <summary>RF05 — Importar a foto do computador. Devolve o endereço, que o item grava como qualquer outra imagem.</summary>
    [Authorize(Roles = Gestao)]
    [HttpPost("fotos")]
    [RequestSizeLimit(EnviarFotoUseCase.TamanhoMaximo + 1024 * 1024)]
    [ProducesResponseType(typeof(FotoEnviadaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnviarFoto(IFormFile? arquivo, [FromServices] IEnviarFotoUseCase useCase)
    {
        using var memoria = new MemoryStream();
        if (arquivo is not null)
            await arquivo.CopyToAsync(memoria);

        return Ok(await useCase.Executar(memoria.ToArray()));
    }

    /// <summary>Foto importada (pública): é o que o cardápio digital mostra ao cliente, que não faz login.</summary>
    [AllowAnonymous]
    [HttpGet("fotos/{nome}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObterFoto(string nome, [FromServices] IArmazenamentoDeFotos armazenamento)
    {
        var foto = armazenamento.Abrir(nome);
        if (foto is null)
            return NotFound();

        var tipo = Path.GetExtension(nome) switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg",
        };
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(foto, tipo);
    }

    /// <summary>UC07 — Marcar item disponível/indisponível.</summary>
    [Authorize(Roles = Gestao)]
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
