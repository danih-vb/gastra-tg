using Gastra.Application.Auditoria;
using Gastra.Application.UseCases.Promocoes;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;
using Mapster;
using ComunicacaoEnums = Gastra.Communication.Enums;
using DominioEnums = Gastra.Domain.Enums;

namespace Gastra.Application.UseCases.Destaques;

internal static class MapeadorDestaque
{
    public static async Task<DestaqueResponse> Montar(
        DestaqueDoCardapio destaque, string nome, IRepositorioDestaque repositorio) => new()
    {
        Id = destaque.Id,
        ItemCardapioId = destaque.ItemDoCardapioId,
        Nome = nome,
        Tipo = destaque.Tipo.Adapt<ComunicacaoEnums.TipoDestaque>(),
        DataInicio = destaque.DataInicio,
        DataFim = destaque.DataFim,
        Meta = destaque.Meta,
        Vendidos = await repositorio.QuantidadeVendida(destaque.ItemDoCardapioId, destaque.DataInicio, destaque.DataFim),
        VigenteHoje = destaque.VigenteEm(HojeNoRestaurante.Data()),
    };
}

// O Gerente ou o Coordenador cria um destaque do dia ou uma meta do mês.
public interface ICriarDestaqueUseCase
{
    Task<DestaqueResponse> Executar(DestaqueRequest request);
}

public class CriarDestaqueUseCase(
    IRepositorioDestaque repositorio,
    IRepositorioItemCardapio repositorioCardapio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : ICriarDestaqueUseCase
{
    public async Task<DestaqueResponse> Executar(DestaqueRequest request)
    {
        var hoje = HojeNoRestaurante.Data();
        var data = request.Data ?? hoje;
        Validar(request, data, hoje);

        var item = await repositorioCardapio.ObterPorId(request.ItemCardapioId)
                   ?? throw new NaoEncontradoException(MensagensErro.ItemCardapioNaoEncontrado);

        var destaque = new DestaqueDoCardapio(item.Id, request.Tipo.Adapt<DominioEnums.TipoDestaque>(), data,
            request.Tipo == ComunicacaoEnums.TipoDestaque.MetaDoMes ? request.Meta : null);

        if ((await repositorio.ListarAPartirDe(destaque.DataInicio)).Any(d => d.ColideCom(destaque)))
            throw new RegraDeNegocioException(MensagensErro.DestaqueRepetido);

        await repositorio.Adicionar(destaque);
        await unitOfWork.Commit();

        await auditoria.Registrar(EventoAuditoria.DestaqueCriado, alvo: (nameof(DestaqueDoCardapio), destaque.Id),
            detalhes: new { ItemCardapioId = item.Id, destaque.Tipo, destaque.DataInicio, destaque.DataFim, destaque.Meta });
        await unitOfWork.Commit();

        return await MapeadorDestaque.Montar(destaque, item.Nome, repositorio);
    }

    private static void Validar(DestaqueRequest request, DateOnly data, DateOnly hoje)
    {
        var erros = new List<string>();

        if (!Enum.IsDefined(request.Tipo))
            erros.Add(MensagensErro.TipoDestaqueInvalido);
        else if (request.Tipo == ComunicacaoEnums.TipoDestaque.MetaDoMes)
        {
            if (request.Meta is null or < 1 or > DestaqueDoCardapio.MetaMaxima)
                erros.Add(MensagensErro.MetaDestaqueInvalida);
            if (new DateOnly(data.Year, data.Month, 1) < new DateOnly(hoje.Year, hoje.Month, 1))
                erros.Add(MensagensErro.DestaqueNoPassado);
        }
        else if (data < hoje)
            erros.Add(MensagensErro.DestaqueNoPassado);

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }
}

// Destaques de hoje em diante, com o quanto já foi vendido. O garçom usa para colorir a lista de lançar.
public interface IListarDestaquesUseCase
{
    Task<List<DestaqueResponse>> Executar();
}

public class ListarDestaquesUseCase(IRepositorioDestaque repositorio, IRepositorioItemCardapio repositorioCardapio)
    : IListarDestaquesUseCase
{
    public async Task<List<DestaqueResponse>> Executar()
    {
        var destaques = await repositorio.ListarAPartirDe(HojeNoRestaurante.Data());
        var nomes = (await repositorioCardapio.ListarPorIds(destaques.Select(d => d.ItemDoCardapioId).Distinct()))
            .ToDictionary(i => i.Id, i => i.Nome);

        var resposta = new List<DestaqueResponse>();
        foreach (var destaque in destaques)
            resposta.Add(await MapeadorDestaque.Montar(destaque,
                nomes.TryGetValue(destaque.ItemDoCardapioId, out var nome) ? nome : string.Empty, repositorio));
        return resposta;
    }
}

public interface IRemoverDestaqueUseCase
{
    Task Executar(int id);
}

public class RemoverDestaqueUseCase(
    IRepositorioDestaque repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IRemoverDestaqueUseCase
{
    public async Task Executar(int id)
    {
        var destaque = await repositorio.ObterPorId(id)
                       ?? throw new NaoEncontradoException(MensagensErro.DestaqueNaoEncontrado);

        repositorio.Remover(destaque);
        await auditoria.Registrar(EventoAuditoria.DestaqueRemovido, alvo: (nameof(DestaqueDoCardapio), id),
            detalhes: new { ItemCardapioId = destaque.ItemDoCardapioId, destaque.Tipo });
        await unitOfWork.Commit();
    }
}
