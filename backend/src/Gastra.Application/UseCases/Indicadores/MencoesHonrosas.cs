using Gastra.Application.Auditoria;
using Gastra.Application.UseCases.Promocoes;
using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Indicadores;

internal static class MencoesHonrosas
{
    public static MencaoHonrosaResponse ParaResposta(MencaoHonrosa mencao) => new()
    {
        Id = mencao.Id,
        Titulo = mencao.Titulo,
        Descricao = mencao.Descricao,
        Icone = mencao.Icone,
        Cor = mencao.Cor,
        Mes = mencao.Mes,
    };
}

/// <summary>
/// O Gerente dá uma menção honrosa a um garçom, por algo que ele avalia fora do sistema. O título e a descrição são
/// texto livre sobre o funcionário: ficam fora do log, que registra só quem recebeu, o ícone, a cor e o mês.
/// </summary>
public interface IConcederMencaoUseCase
{
    Task<MencaoHonrosaResponse> Executar(MencaoHonrosaRequest request);
}

public class ConcederMencaoUseCase(
    IRepositorioMencaoHonrosa repositorio,
    IRepositorioUsuario repositorioUsuario,
    IUsuarioLogado usuarioLogado,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IConcederMencaoUseCase
{
    public async Task<MencaoHonrosaResponse> Executar(MencaoHonrosaRequest request)
    {
        var quem = usuarioLogado.ObterIdentificacao()
                   ?? throw new InvalidOperationException("Menção honrosa sem usuário autenticado.");
        var hoje = HojeNoRestaurante.Data();
        var mes = request.Mes ?? hoje;
        Validar(request, mes, hoje);

        var garcom = await repositorioUsuario.ObterPorId(request.GarcomId);
        if (garcom is null || garcom.Papel != PapelUsuario.Garcom || !garcom.Ativo)
            throw new NaoEncontradoException(MensagensErro.UsuarioNaoEncontrado);

        var mencao = new MencaoHonrosa(garcom.Id, request.Titulo, request.Descricao, request.Icone, request.Cor, mes, quem.Id);
        await repositorio.Adicionar(mencao);
        await unitOfWork.Commit();

        await auditoria.Registrar(EventoAuditoria.MencaoConcedida, alvo: (nameof(MencaoHonrosa), mencao.Id),
            detalhes: new { GarcomId = garcom.Id, mencao.Icone, mencao.Cor, mencao.Mes });
        await unitOfWork.Commit();

        return MencoesHonrosas.ParaResposta(mencao);
    }

    private static void Validar(MencaoHonrosaRequest request, DateOnly mes, DateOnly hoje)
    {
        var erros = new List<string>();
        var titulo = request.Titulo?.Trim() ?? string.Empty;

        if (titulo.Length is 0 or > MencaoHonrosa.TamanhoMaximoDoTitulo)
            erros.Add(MensagensErro.TituloDaMencaoInvalido);
        if (request.Descricao?.Trim().Length > MencaoHonrosa.TamanhoMaximoDaDescricao)
            erros.Add(MensagensErro.DescricaoDaMencaoMuitoLonga);
        if (!MencaoHonrosa.Icones.Contains(request.Icone))
            erros.Add(MensagensErro.IconeDaMencaoInvalido);
        if (!MencaoHonrosa.Cores.Contains(request.Cor))
            erros.Add(MensagensErro.CorDaMencaoInvalida);
        if (new DateOnly(mes.Year, mes.Month, 1) > new DateOnly(hoje.Year, hoje.Month, 1))
            erros.Add(MensagensErro.MencaoNoFuturo);

        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }
}

public interface IRemoverMencaoUseCase
{
    Task Executar(int id);
}

public class RemoverMencaoUseCase(
    IRepositorioMencaoHonrosa repositorio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IRemoverMencaoUseCase
{
    public async Task Executar(int id)
    {
        var mencao = await repositorio.ObterPorId(id)
                     ?? throw new NaoEncontradoException(MensagensErro.MencaoNaoEncontrada);

        repositorio.Remover(mencao);
        await auditoria.Registrar(EventoAuditoria.MencaoRemovida, alvo: (nameof(MencaoHonrosa), id),
            detalhes: new { mencao.GarcomId });
        await unitOfWork.Commit();
    }
}
