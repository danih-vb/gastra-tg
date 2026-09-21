using Gastra.Application.UseCases.Indicadores;
using Gastra.Communication.Enums;
using Gastra.Communication.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gastra.Api.Controllers;

/// <summary>
/// Relatórios de BI (UC16, RF10) e índice de desempenho (UC17, RF11). Filtro de período por
/// <c>inicio</c> e <c>fim</c> (datas inclusivas, formato aaaa-mm-dd); sem filtro, os últimos 30 dias.
/// </summary>
[ApiController]
[Route("api/indicadores")]
[Authorize]
public class IndicadoresController : ControllerBase
{
    private const string Gerente = nameof(PapelUsuario.Gerente);
    private const string GerenteOuGarcom = nameof(PapelUsuario.Gerente) + "," + nameof(PapelUsuario.Garcom);

    /// <summary>
    /// Média e distribuição das notas que os clientes deixaram no período (RF25). Só agregado: comentário
    /// e garçom não saem daqui.
    /// </summary>
    [Authorize(Roles = Gerente)]
    [HttpGet("avaliacoes")]
    [ProducesResponseType(typeof(RelatorioAvaliacoesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Avaliacoes(DateOnly? inicio, DateOnly? fim, [FromServices] IRelatorioAvaliacoesUseCase useCase) =>
        Ok(await useCase.Executar(inicio, fim));

    /// <summary>Faturamento total e por turno, comandas, mesas atendidas, ticket médio e tempo de atendimento por garçom.</summary>
    [Authorize(Roles = Gerente)]
    [HttpGet("garcons")]
    [ProducesResponseType(typeof(RelatorioGarconsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Garcons(DateOnly? inicio, DateOnly? fim, [FromServices] IRelatorioGarconsUseCase useCase) =>
        Ok(await useCase.Executar(inicio, fim));

    /// <summary>Faturamento, ticket médio e faturamento médio histórico por praça.</summary>
    [Authorize(Roles = Gerente)]
    [HttpGet("pracas")]
    [ProducesResponseType(typeof(RelatorioPracasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Pracas(DateOnly? inicio, DateOnly? fim, [FromServices] IRelatorioPracasUseCase useCase) =>
        Ok(await useCase.Executar(inicio, fim));

    /// <summary>Faturamento por item e por categoria do cardápio.</summary>
    [Authorize(Roles = Gerente)]
    [HttpGet("cardapio")]
    [ProducesResponseType(typeof(RelatorioCardapioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cardapio(DateOnly? inicio, DateOnly? fim, [FromServices] IRelatorioCardapioUseCase useCase) =>
        Ok(await useCase.Executar(inicio, fim));

    /// <summary>Faturamento por praça por hora do dia e por dia da semana.</summary>
    [Authorize(Roles = Gerente)]
    [HttpGet("horarios")]
    [ProducesResponseType(typeof(RelatorioHorariosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Horarios(DateOnly? inicio, DateOnly? fim, [FromServices] IRelatorioHorariosUseCase useCase) =>
        Ok(await useCase.Executar(inicio, fim));

    /// <summary>UC17 — ranking pelo índice de desempenho. O Gerente vê todos; o Garçom vê só a própria posição.</summary>
    [Authorize(Roles = GerenteOuGarcom)]
    [HttpGet("desempenho")]
    [ProducesResponseType(typeof(RankingDesempenhoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Desempenho(DateOnly? inicio, DateOnly? fim, [FromServices] IRankingDesempenhoUseCase useCase) =>
        Ok(await useCase.Executar(inicio, fim));
}
