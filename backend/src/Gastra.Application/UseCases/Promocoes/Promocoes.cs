using Gastra.Application.Auditoria;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;
using Mapster;
using ComunicacaoEnums = Gastra.Communication.Enums;
using DominioEnums = Gastra.Domain.Enums;

namespace Gastra.Application.UseCases.Promocoes;

/// <summary>Data de hoje no horário de Brasília: é ela que decide se uma promoção está valendo.</summary>
public static class HojeNoRestaurante
{
    public static DateOnly Data() => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3));
}

internal static class MapeadorPromocao
{
    public static PromocaoResponse Montar(Promocao promocao, IReadOnlyDictionary<int, ItemDoCardapio> itens) => new()
    {
        Id = promocao.Id,
        Descricao = promocao.Descricao,
        TipoDesconto = promocao.TipoDesconto.Adapt<ComunicacaoEnums.TipoDesconto>(),
        ValorDesconto = promocao.ValorDesconto,
        DataInicio = promocao.DataInicio,
        DataFim = promocao.DataFim,
        Ativa = promocao.Ativa,
        VigenteHoje = promocao.VigenteEm(HojeNoRestaurante.Data()),
        Itens = promocao.ItemCardapioIds
            .Where(itens.ContainsKey)
            .Select(id => new ItemPromocaoResponse
            {
                ItemCardapioId = id,
                Nome = itens[id].Nome,
                PrecoOriginal = itens[id].Preco,
                PrecoComDesconto = promocao.AplicarDesconto(itens[id].Preco),
            })
            .ToList(),
    };
}

// UC08 — Criar promoção (RF22)
public interface ICriarPromocaoUseCase
{
    Task<PromocaoResponse> Executar(PromocaoRequest request);
}

public class CriarPromocaoUseCase(
    IRepositorioPromocao repositorio,
    IRepositorioItemCardapio repositorioCardapio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : ICriarPromocaoUseCase
{
    public async Task<PromocaoResponse> Executar(PromocaoRequest request)
    {
        Validar(request);

        var ids = request.ItemCardapioIds.Distinct().ToList();
        var itens = await repositorioCardapio.ListarPorIds(ids);
        if (itens.Count != ids.Count)
            throw new NaoEncontradoException(MensagensErro.ItemCardapioNaoEncontrado);

        var tipo = request.TipoDesconto.Adapt<DominioEnums.TipoDesconto>();
        if (tipo == DominioEnums.TipoDesconto.ValorFixo && itens.Any(i => request.ValorDesconto >= i.Preco))
            throw new RegraDeNegocioException(MensagensErro.DescontoFixoMaiorQuePreco);

        var promocao = new Promocao(request.Descricao, tipo, request.ValorDesconto, request.DataInicio, request.DataFim, itens);

        await repositorio.Adicionar(promocao);
        await unitOfWork.Commit();

        // Política de log, 4.2: ator, alvo e itens vinculados. Dados do cardápio não são pessoais.
        await auditoria.Registrar(EventoAuditoria.PromocaoCriada, alvo: (nameof(Promocao), promocao.Id), detalhes: new
        {
            ItensVinculados = ids,
            TipoDesconto = tipo,
            promocao.ValorDesconto,
            promocao.DataInicio,
            promocao.DataFim,
        });
        await unitOfWork.Commit();

        return MapeadorPromocao.Montar(promocao, itens.ToDictionary(i => i.Id));
    }

    private static void Validar(PromocaoRequest request)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Descricao))
            erros.Add(MensagensErro.DescricaoPromocaoObrigatoria);
        else if (request.Descricao.Trim().Length > Promocao.TamanhoMaximoDescricao)
            erros.Add(MensagensErro.DescricaoPromocaoMuitoLonga);

        if (!Enum.IsDefined(request.TipoDesconto))
            erros.Add(MensagensErro.TipoDescontoInvalido);
        else if (request.ValorDesconto <= 0 || (request.TipoDesconto == ComunicacaoEnums.TipoDesconto.Percentual && request.ValorDesconto >= 100))
            erros.Add(MensagensErro.ValorDescontoInvalido);

        if (request.DataInicio == default || request.DataFim == default || request.DataFim < request.DataInicio)
            erros.Add(MensagensErro.PeriodoPromocaoInvalido);

        if (request.ItemCardapioIds.Count == 0)
            erros.Add(MensagensErro.PromocaoSemItens);

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }
}

// Lista de promoções para a gestão do cardápio
public interface IListarPromocoesUseCase
{
    Task<List<PromocaoResponse>> Executar(bool somenteAtivas);
}

public class ListarPromocoesUseCase(IRepositorioPromocao repositorio, IRepositorioItemCardapio repositorioCardapio)
    : IListarPromocoesUseCase
{
    public async Task<List<PromocaoResponse>> Executar(bool somenteAtivas)
    {
        var promocoes = await repositorio.Listar(somenteAtivas);
        var itens = (await repositorioCardapio.ListarPorIds(promocoes.SelectMany(p => p.ItemCardapioIds))).ToDictionary(i => i.Id);

        return promocoes.Select(p => MapeadorPromocao.Montar(p, itens)).ToList();
    }
}

// UC09 — Remover promoção (RF22): desativa, sem apagar
public interface IRemoverPromocaoUseCase
{
    Task Executar(int id);
}

public class RemoverPromocaoUseCase(
    IRepositorioPromocao repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IRemoverPromocaoUseCase
{
    public async Task Executar(int id)
    {
        var promocao = await repositorio.ObterPorId(id)
                       ?? throw new NaoEncontradoException(MensagensErro.PromocaoNaoEncontrada);

        if (!promocao.Ativa)
            throw new RegraDeNegocioException(MensagensErro.PromocaoJaDesativada);

        promocao.Desativar();

        await auditoria.Registrar(EventoAuditoria.PromocaoRemovida, alvo: (nameof(Promocao), promocao.Id),
            detalhes: new { ItensVinculados = promocao.ItemCardapioIds });
        await unitOfWork.Commit();
    }
}
