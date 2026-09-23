using Gastra.Application.Auditoria;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Enums;
using Gastra.Domain.Indicadores;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Indicadores;

/// <summary>Período dos relatórios: datas inclusivas, padrão dos últimos 30 dias no horário de Brasília.</summary>
internal static class PeriodoDeRelatorio
{
    public const int DiasPadrao = 30;
    public const int DiasMaximos = 366;

    public static (DateOnly Inicio, DateOnly Fim, PeriodoResponse Resposta) Resolver(DateOnly? inicio, DateOnly? fim)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3));
        var ultimo = fim ?? hoje;
        var primeiro = inicio ?? ultimo.AddDays(-(DiasPadrao - 1));

        if (primeiro > ultimo)
            throw new ErroValidacaoException([MensagensErro.PeriodoRelatorioInvalido]);
        if (ultimo.DayNumber - primeiro.DayNumber + 1 > DiasMaximos)
            throw new ErroValidacaoException([MensagensErro.PeriodoRelatorioLongoDemais]);

        // As consultas usam fim exclusivo; a resposta mostra as datas como o usuário pediu.
        return (primeiro, ultimo.AddDays(1), new PeriodoResponse { Inicio = primeiro, Fim = ultimo });
    }

    public static decimal Dividir(decimal valor, decimal divisor, int casas = 2) =>
        divisor == 0 ? 0 : Math.Round(valor / divisor, casas);
}

// UC16 — relatório por garçom (RF10)
public interface IRelatorioGarconsUseCase
{
    Task<RelatorioGarconsResponse> Executar(DateOnly? inicio, DateOnly? fim);
}

public class RelatorioGarconsUseCase(
    IRepositorioIndicadores indicadores,
    IRepositorioUsuario repositorioUsuario,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IRelatorioGarconsUseCase
{
    public async Task<RelatorioGarconsResponse> Executar(DateOnly? inicio, DateOnly? fim)
    {
        var periodo = PeriodoDeRelatorio.Resolver(inicio, fim);
        var linhas = await indicadores.ObterIndicadoresPorGarcom(periodo.Inicio, periodo.Fim);
        var nomes = (await repositorioUsuario.ListarPorIds(linhas.Select(l => l.GarcomId))).ToDictionary(u => u.Id, u => u.Nome);

        await Auditar(auditoria, unitOfWork, "garcons", periodo.Resposta);

        return new RelatorioGarconsResponse
        {
            Periodo = periodo.Resposta,
            Garcons = linhas
                .OrderByDescending(l => l.Faturamento)
                .Select(l => new IndicadorGarcomResponse
                {
                    GarcomId = l.GarcomId,
                    Nome = nomes.GetValueOrDefault(l.GarcomId, string.Empty),
                    Faturamento = l.Faturamento,
                    Comandas = l.Comandas,
                    Turnos = l.Turnos,
                    FaturamentoPorTurno = PeriodoDeRelatorio.Dividir(l.Faturamento, l.Turnos),
                    MesasAtendidas = l.MesasAtendidas,
                    TicketMedio = PeriodoDeRelatorio.Dividir(l.Faturamento, l.Comandas),
                    TempoMedioAtendimentoMinutos = PeriodoDeRelatorio.Dividir(l.SomaMinutosAtendimento, l.Comandas, 1),
                })
                .ToList(),
            Totais = new TotaisResponse
            {
                Faturamento = linhas.Sum(l => l.Faturamento),
                Comandas = linhas.Sum(l => l.Comandas),
                TicketMedio = PeriodoDeRelatorio.Dividir(linhas.Sum(l => l.Faturamento), linhas.Sum(l => l.Comandas)),
                TempoMedioAtendimentoMinutos = PeriodoDeRelatorio.Dividir(linhas.Sum(l => l.SomaMinutosAtendimento), linhas.Sum(l => l.Comandas), 1),
            },
        };
    }

    /// <summary>Política de log, 4.5: quem consultou qual relatório, com o filtro de período.</summary>
    internal static async Task Auditar(IRegistradorAuditoria auditoria, IUnitOfWork unitOfWork, string relatorio, PeriodoResponse periodo)
    {
        await auditoria.Registrar(EventoAuditoria.RelatorioBiConsultado,
            detalhes: new { Relatorio = relatorio, periodo.Inicio, periodo.Fim });
        await unitOfWork.Commit();
    }
}

// UC16 — relatório por praça (RF10)
public interface IRelatorioPracasUseCase
{
    Task<RelatorioPracasResponse> Executar(DateOnly? inicio, DateOnly? fim);
}

public class RelatorioPracasUseCase(
    IRepositorioIndicadores indicadores,
    IRepositorioPraca repositorioPraca,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IRelatorioPracasUseCase
{
    public async Task<RelatorioPracasResponse> Executar(DateOnly? inicio, DateOnly? fim)
    {
        var periodo = PeriodoDeRelatorio.Resolver(inicio, fim);
        var linhas = (await indicadores.ObterIndicadoresPorPraca(periodo.Inicio, periodo.Fim)).ToDictionary(l => l.PracaId);
        var historico = await indicadores.ObterFaturamentoMedioPorPraca();
        var pracas = await repositorioPraca.ListarTodas();

        await RelatorioGarconsUseCase.Auditar(auditoria, unitOfWork, "pracas", periodo.Resposta);

        // Todas as praças aparecem, inclusive as que não tiveram movimento no período.
        return new RelatorioPracasResponse
        {
            Periodo = periodo.Resposta,
            Pracas = pracas
                .Select(p =>
                {
                    var linha = linhas.GetValueOrDefault(p.Id) ?? new IndicadorPraca(p.Id, 0, 0, 0);
                    return new IndicadorPracaResponse
                    {
                        PracaId = p.Id,
                        Codigo = p.Codigo,
                        Faturamento = linha.Faturamento,
                        Comandas = linha.Comandas,
                        TurnosComMovimento = linha.Turnos,
                        FaturamentoPorTurno = PeriodoDeRelatorio.Dividir(linha.Faturamento, linha.Turnos),
                        TicketMedio = PeriodoDeRelatorio.Dividir(linha.Faturamento, linha.Comandas),
                        FaturamentoMedioHistorico = historico.GetValueOrDefault(p.Id),
                    };
                })
                .OrderByDescending(p => p.Faturamento).ThenBy(p => p.Codigo)
                .ToList(),
        };
    }
}

// UC16 — faturamento por item e por categoria do cardápio (RF10)
public interface IRelatorioCardapioUseCase
{
    Task<RelatorioCardapioResponse> Executar(DateOnly? inicio, DateOnly? fim);
}

public class RelatorioCardapioUseCase(
    IRepositorioIndicadores indicadores,
    IRepositorioItemCardapio repositorioCardapio,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IRelatorioCardapioUseCase
{
    public async Task<RelatorioCardapioResponse> Executar(DateOnly? inicio, DateOnly? fim)
    {
        var periodo = PeriodoDeRelatorio.Resolver(inicio, fim);
        var linhas = await indicadores.ObterIndicadoresPorItem(periodo.Inicio, periodo.Fim);
        var nomes = (await repositorioCardapio.ListarPorIds(linhas.Select(l => l.ItemCardapioId))).ToDictionary(i => i.Id, i => i.Nome);
        var total = linhas.Sum(l => l.Faturamento);

        await RelatorioGarconsUseCase.Auditar(auditoria, unitOfWork, "cardapio", periodo.Resposta);

        return new RelatorioCardapioResponse
        {
            Periodo = periodo.Resposta,
            Itens = linhas
                .OrderByDescending(l => l.Faturamento)
                .Select(l => new IndicadorItemResponse
                {
                    ItemCardapioId = l.ItemCardapioId,
                    Nome = nomes.GetValueOrDefault(l.ItemCardapioId, string.Empty),
                    Categoria = l.Categoria,
                    Quantidade = l.Quantidade,
                    Faturamento = l.Faturamento,
                    ParticipacaoPercentual = PeriodoDeRelatorio.Dividir(100 * l.Faturamento, total, 1),
                })
                .ToList(),
            Categorias = linhas
                .GroupBy(l => l.Categoria)
                .Select(g => new IndicadorCategoriaResponse
                {
                    Categoria = g.Key,
                    Quantidade = g.Sum(l => l.Quantidade),
                    Faturamento = g.Sum(l => l.Faturamento),
                    ParticipacaoPercentual = PeriodoDeRelatorio.Dividir(100 * g.Sum(l => l.Faturamento), total, 1),
                })
                .OrderByDescending(c => c.Faturamento)
                .ToList(),
        };
    }
}

// UC16 — faturamento por praça por hora do dia e por dia da semana (RF10)
public interface IRelatorioHorariosUseCase
{
    Task<RelatorioHorariosResponse> Executar(DateOnly? inicio, DateOnly? fim);
}

public class RelatorioHorariosUseCase(
    IRepositorioIndicadores indicadores,
    IRepositorioPraca repositorioPraca,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IRelatorioHorariosUseCase
{
    private static readonly string[] NomesDosDias = ["", "Domingo", "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado"];

    public async Task<RelatorioHorariosResponse> Executar(DateOnly? inicio, DateOnly? fim)
    {
        var periodo = PeriodoDeRelatorio.Resolver(inicio, fim);
        var porHora = await indicadores.ObterFaturamentoPorPracaEHora(periodo.Inicio, periodo.Fim);
        var porDia = await indicadores.ObterFaturamentoPorPracaEDiaDaSemana(periodo.Inicio, periodo.Fim);
        var codigos = (await repositorioPraca.ListarTodas()).ToDictionary(p => p.Id, p => p.Codigo);

        await RelatorioGarconsUseCase.Auditar(auditoria, unitOfWork, "horarios", periodo.Resposta);

        return new RelatorioHorariosResponse
        {
            Periodo = periodo.Resposta,
            PorHora = porHora
                .OrderBy(l => codigos.GetValueOrDefault(l.PracaId)).ThenBy(l => l.Fatia)
                .Select(l => new FaturamentoNaHoraResponse
                {
                    PracaId = l.PracaId,
                    PracaCodigo = codigos.GetValueOrDefault(l.PracaId, string.Empty),
                    Hora = l.Fatia,
                    Faturamento = l.Faturamento,
                    Comandas = l.Comandas,
                })
                .ToList(),
            PorDiaDaSemana = porDia
                .OrderBy(l => codigos.GetValueOrDefault(l.PracaId)).ThenBy(l => l.Fatia)
                .Select(l => new FaturamentoNoDiaResponse
                {
                    PracaId = l.PracaId,
                    PracaCodigo = codigos.GetValueOrDefault(l.PracaId, string.Empty),
                    DiaDaSemana = l.Fatia,
                    NomeDoDia = l.Fatia is >= 1 and <= 7 ? NomesDosDias[l.Fatia] : string.Empty,
                    Faturamento = l.Faturamento,
                    Comandas = l.Comandas,
                })
                .ToList(),
        };
    }
}

// UC17 — índice de desempenho e ranking (RF11)
public interface IRankingDesempenhoUseCase
{
    Task<RankingDesempenhoResponse> Executar(DateOnly? inicio, DateOnly? fim);
}

public class RankingDesempenhoUseCase(
    IRepositorioIndicadores indicadores,
    IRepositorioUsuario repositorioUsuario,
    IUsuarioLogado usuarioLogado,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IRankingDesempenhoUseCase
{
    public async Task<RankingDesempenhoResponse> Executar(DateOnly? inicio, DateOnly? fim)
    {
        var periodo = PeriodoDeRelatorio.Resolver(inicio, fim);
        var linhas = await indicadores.ObterIndicadoresPorGarcom(periodo.Inicio, periodo.Fim);
        var avaliacoes = (await indicadores.ObterAvaliacoesPorGarcom(periodo.Inicio, periodo.Fim)).ToDictionary(a => a.GarcomId);
        var calculo = IndiceDeDesempenho.Calcular(linhas.Select(l => new DesempenhoNoPeriodo(
            l.GarcomId, l.Faturamento, l.Turnos, l.MesasAtendidas,
            avaliacoes.GetValueOrDefault(l.GarcomId)?.Quantidade ?? 0,
            avaliacoes.GetValueOrDefault(l.GarcomId)?.SomaDasNotas ?? 0)));
        var ranking = calculo.Posicoes;

        var quem = usuarioLogado.ObterIdentificacao()
                   ?? throw new InvalidOperationException("Consulta de desempenho sem usuário autenticado.");

        // O índice é avaliação de desempenho, dado pessoal do funcionário: o garçom vê só a própria posição.
        var visiveis = quem.Papel == PapelUsuario.Garcom
            ? ranking.Where(p => p.GarcomId == quem.Id).ToList()
            : ranking.ToList();

        var nomes = (await repositorioUsuario.ListarPorIds(visiveis.Select(p => p.GarcomId))).ToDictionary(u => u.Id, u => u.Nome);

        // Política de log, 4.5: quem consultou o índice de quem.
        await auditoria.Registrar(EventoAuditoria.IndiceDesempenhoConsultado, detalhes: new
        {
            GarcomConsultado = quem.Papel == PapelUsuario.Garcom ? quem.Id.ToString() : "todos",
            periodo.Resposta.Inicio,
            periodo.Resposta.Fim,
        });
        await unitOfWork.Commit();

        return new RankingDesempenhoResponse
        {
            Periodo = periodo.Resposta,
            PesoFaturamento = calculo.Pesos.Faturamento,
            PesoMesasAtendidas = calculo.Pesos.MesasAtendidas,
            PesoAvaliacao = calculo.Pesos.Avaliacao,
            MinimoDeAvaliacoes = IndiceDeDesempenho.MinimoDeAvaliacoes,
            TotalNoRanking = ranking.Count,
            Posicoes = visiveis
                .Select(p => new PosicaoRankingResponse
                {
                    Posicao = p.Posicao,
                    GarcomId = p.GarcomId,
                    Nome = nomes.GetValueOrDefault(p.GarcomId, string.Empty),
                    Indice = p.Indice,
                    FaturamentoPorTurno = p.FaturamentoPorTurno,
                    MesasPorTurno = p.MesasPorTurno,
                    Turnos = p.Turnos,
                    NotaConsiderada = p.NotaConsiderada,
                })
                .ToList(),
        };
    }
}

// UC16 — avaliações do atendimento (RF25)
public interface IRelatorioAvaliacoesUseCase
{
    Task<RelatorioAvaliacoesResponse> Executar(DateOnly? inicio, DateOnly? fim);
}

/// <summary>
/// Só números agregados. O comentário do cliente fica no banco e não sai por aqui: é texto livre, pode ter
/// vindo com dado pessoal sem ninguém pedir, e uma nota atrelada a uma mesa apontaria para quem atendeu.
/// </summary>
public class RelatorioAvaliacoesUseCase(
    IRepositorioIndicadores indicadores,
    IRegistradorAuditoria auditoria,
    IUnitOfWork unitOfWork) : IRelatorioAvaliacoesUseCase
{
    public async Task<RelatorioAvaliacoesResponse> Executar(DateOnly? inicio, DateOnly? fim)
    {
        var periodo = PeriodoDeRelatorio.Resolver(inicio, fim);
        var linhas = await indicadores.ObterDistribuicaoDeAvaliacoes(periodo.Inicio, periodo.Fim);

        await RelatorioGarconsUseCase.Auditar(auditoria, unitOfWork, "avaliacoes", periodo.Resposta);

        var total = linhas.Sum(l => l.Quantidade);
        var soma = linhas.Sum(l => l.Nota * l.Quantidade);

        return new RelatorioAvaliacoesResponse
        {
            Periodo = periodo.Resposta,
            Quantidade = total,
            Media = PeriodoDeRelatorio.Dividir(soma, total, casas: 1),
            // As cinco notas sempre aparecem: barra vazia informa tanto quanto barra cheia.
            Distribuicao = Enumerable.Range(1, 5).Select(nota =>
            {
                var quantidade = linhas.FirstOrDefault(l => l.Nota == nota)?.Quantidade ?? 0;
                return new FaixaDeNotaResponse
                {
                    Nota = nota,
                    Quantidade = quantidade,
                    Percentual = PeriodoDeRelatorio.Dividir(quantidade * 100, total, casas: 1),
                };
            }).ToList(),
        };
    }
}
