using Gastra.Api.Configuracao;
using Gastra.Application.UseCases.Comandas;
using Gastra.Communication.Enums;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Gastra.Api.Controllers;

[ApiController]
[Route("api/comandas")]
// Exigências de papel ficam em cada ação: quando o controller também traz [Authorize], o ASP.NET Core
// soma as duas condições, e o gerente seria barrado até na leitura.
[Authorize]
public class ComandaController : ControllerBase
{
    // O núcleo de comandas é do Garçom (RF01–RF04); metre e gestão acompanham só de leitura (RNF04).
    private const string Garcom = nameof(PapelUsuario.Garcom);

    private const string PapeisDeLeitura =
        nameof(PapelUsuario.Garcom) + "," + nameof(PapelUsuario.Metre) + "," +
        nameof(PapelUsuario.Coordenador) + "," + nameof(PapelUsuario.Gerente);

    /// <summary>UC10 — Abrir comanda numa mesa, informando o número de pessoas.</summary>
    [Authorize(Roles = Garcom)]
    [HttpPost]
    [ProducesResponseType(typeof(ComandaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Abrir(
        [FromBody] AbrirComandaRequest request,
        [FromServices] IAbrirComandaUseCase useCase)
    {
        var resposta = await useCase.Executar(request);
        return CreatedAtAction(nameof(Obter), new { id = resposta.Id }, resposta);
    }

    /// <summary>UC11 — Confirmar ou ajustar a composição sugerida pelo sistema.</summary>
    [Authorize(Roles = Garcom)]
    [HttpPatch("{id:int}/composicao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConfirmarComposicao(
        int id,
        [FromBody] ComposicaoRequest request,
        [FromServices] IConfirmarComposicaoUseCase useCase)
    {
        await useCase.Executar(id, request);
        return NoContent();
    }

    /// <summary>UC12 — Registrar item do pedido na comanda.</summary>
    [Authorize(Roles = Garcom)]
    [HttpPost("{id:int}/itens")]
    [ProducesResponseType(typeof(ItemPedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarItem(
        int id,
        [FromBody] ItemPedidoRequest request,
        [FromServices] IRegistrarItemPedidoUseCase useCase)
    {
        var resposta = await useCase.Executar(id, request);
        return CreatedAtAction(nameof(Obter), new { id }, resposta);
    }

    /// <summary>UC23 — Marcar item como entregue ou cancelá-lo com justificativa (RN02).</summary>
    [Authorize(Roles = Garcom)]
    [HttpPatch("{id:int}/itens/{itemId:int}/situacao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AtualizarSituacaoDoItem(
        int id,
        int itemId,
        [FromBody] SituacaoItemRequest request,
        [FromServices] IAtualizarSituacaoItemUseCase useCase)
    {
        await useCase.Executar(id, itemId, request);
        return NoContent();
    }

    /// <summary>UC13 — Registrar restrição alimentar informada pelo cliente (RF14).</summary>
    [Authorize(Roles = Garcom)]
    [HttpPost("{id:int}/restricoes")]
    [ProducesResponseType(typeof(RestricaoAlimentarResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarRestricao(
        int id,
        [FromBody] RestricaoAlimentarRequest request,
        [FromServices] IRegistrarRestricaoUseCase useCase)
    {
        var resposta = await useCase.Executar(id, request);
        return CreatedAtAction(nameof(Obter), new { id }, resposta);
    }

    /// <summary>RF04 — Remover a taxa de serviço a pedido do cliente.</summary>
    [Authorize(Roles = Garcom)]
    [HttpDelete("{id:int}/taxa-servico")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RemoverTaxaServico(int id, [FromServices] IRemoverTaxaServicoUseCase useCase)
    {
        await useCase.Executar(id);
        return NoContent();
    }

    /// <summary>UC14 — Fechar a comanda e calcular o total (RN02, RF04).</summary>
    [Authorize(Roles = Garcom)]
    [HttpPost("{id:int}/fechamento")]
    [ProducesResponseType(typeof(ComandaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Fechar(int id, [FromServices] IFecharComandaUseCase useCase)
    {
        return Ok(await useCase.Executar(id));
    }

    /// <summary>
    /// UC18 — Sugestões de itens para oferecer na mesa (RF09). Se a camada analítica estiver fora do ar,
    /// responde 200 com a lista vazia e <c>servicoDisponivel = false</c>: o atendimento não para (D3).
    /// </summary>
    [Authorize(Roles = Garcom)]
    [HttpGet("{id:int}/sugestoes")]
    [ProducesResponseType(typeof(SugestoesComandaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Sugestoes(int id, [FromServices] ISugerirCombinacoesUseCase useCase)
    {
        return Ok(await useCase.Executar(id));
    }

    /// <summary>Consultar uma comanda pelo salão.</summary>
    [Authorize(Roles = PapeisDeLeitura)]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ComandaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(int id, [FromServices] IObterComandaUseCase useCase)
    {
        return Ok(await useCase.Executar(id));
    }

    /// <summary>Comandas abertas agora: é o painel do salão.</summary>
    [Authorize(Roles = PapeisDeLeitura)]
    [HttpGet]
    [ProducesResponseType(typeof(List<ComandaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarAbertas([FromServices] IListarComandasAbertasUseCase useCase)
    {
        return Ok(await useCase.Executar());
    }

    /// <summary>
    /// UC20 — Consulta do cliente pelo QR code da mesa, sem login (RF13). O código de acesso funciona
    /// como senha da comanda, por isso vai na rota e nunca em log (política de log, seção 4.4).
    /// </summary>
    [AllowAnonymous]
    [HttpGet("consulta/{codigoAcesso}")]
    [EnableRateLimiting(LimiteDeRequisicoes.ConsultaCliente)]
    [ProducesResponseType(typeof(ComandaClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarPeloCliente(
        string codigoAcesso,
        [FromServices] IConsultarComandaClienteUseCase useCase)
    {
        return Ok(await useCase.Executar(codigoAcesso));
    }

    /// <summary>
    /// UC25 — Avaliação do atendimento pelo cliente, sem login (RF25). É a única escrita que o código de
    /// acesso permite, e só depois de a conta fechar, uma vez e dentro da janela (RN08).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("consulta/{codigoAcesso}/avaliacao")]
    [EnableRateLimiting(LimiteDeRequisicoes.ConsultaCliente)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AvaliarAtendimento(
        string codigoAcesso,
        [FromBody] AvaliacaoRequest request,
        [FromServices] IAvaliarAtendimentoUseCase useCase)
    {
        await useCase.Executar(codigoAcesso, request);
        return NoContent();
    }
}
