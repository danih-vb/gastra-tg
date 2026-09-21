using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Indicadores;
using Gastra.Domain.Repositorios;
using Microsoft.Extensions.DependencyInjection;
using static Gastra.Api.Tests.Infraestrutura.GastraApiFactory;

namespace Gastra.Api.Tests.Controllers;

/// <summary>
/// UC16 e UC17 — relatórios de BI e índice de desempenho. As views são substituídas por IndicadoresFalsos;
/// as consultas SQL são testadas contra o MySQL em RepositorioIndicadoresTests.
/// </summary>
public class IndicadoresControllerTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>, IAsyncLifetime
{
    private const string Rota = "/api/indicadores";

    private readonly HttpClient _gerente = factory.CreateClient();

    public async Task InitializeAsync()
    {
        factory.Indicadores.Reiniciar();
        Autenticar(_gerente, await factory.TokenGerente(factory.CreateClient()));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<T> Ler<T>(HttpClient cliente, string url)
    {
        var resposta = await cliente.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<T>(Json))!;
    }

    private async Task<(HttpClient Cliente, Usuario Garcom)> ClienteGarcom(string nome = "Garçom")
    {
        var email = $"garcom-{Guid.NewGuid():N}@gastra.test";
        var garcom = await factory.CriarUsuario(email, PapelUsuario.Garcom);
        var cliente = factory.CreateClient();
        Autenticar(cliente, await factory.Login(factory.CreateClient(), email));
        return (cliente, garcom);
    }

    [Fact]
    public async Task Periodo_SemFiltro_SaoOsUltimos30Dias_ComFimInclusivo()
    {
        var relatorio = await Ler<RelatorioGarconsResponse>(_gerente, $"{Rota}/garcons");

        Assert.Equal(29, relatorio.Periodo.Fim.DayNumber - relatorio.Periodo.Inicio.DayNumber);
        // A consulta ao banco usa fim exclusivo: o dia seguinte ao fim pedido.
        Assert.Equal((relatorio.Periodo.Inicio, relatorio.Periodo.Fim.AddDays(1)), factory.Indicadores.PeriodosConsultados.Single());
    }

    [Theory]
    [InlineData("inicio=2026-09-10&fim=2026-09-01")]
    [InlineData("inicio=2025-01-01&fim=2026-09-01")]
    public async Task Periodo_InvalidoOuLongoDemais_Retorna400(string filtro)
    {
        var resposta = await _gerente.GetAsync($"{Rota}/garcons?{filtro}");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task RelatorioDeGarcons_CalculaMediasETotais_ComNome()
    {
        var (_, ana) = await ClienteGarcom();
        var (_, bia) = await ClienteGarcom();
        factory.Indicadores.Garcons.Add(new IndicadorGarcom(ana.Id, Faturamento: 1000m, Comandas: 4, Turnos: 2, MesasAtendidas: 4, SomaMinutosAtendimento: 200));
        factory.Indicadores.Garcons.Add(new IndicadorGarcom(bia.Id, Faturamento: 3000m, Comandas: 6, Turnos: 3, MesasAtendidas: 5, SomaMinutosAtendimento: 360));

        var relatorio = await Ler<RelatorioGarconsResponse>(_gerente, $"{Rota}/garcons?inicio=2026-09-01&fim=2026-09-15");

        var primeiro = relatorio.Garcons[0];
        Assert.Equal(bia.Id, primeiro.GarcomId);
        Assert.Equal(bia.Nome, primeiro.Nome);
        Assert.Equal((1000m, 500m, 60m), (primeiro.FaturamentoPorTurno, primeiro.TicketMedio, primeiro.TempoMedioAtendimentoMinutos));
        Assert.Equal(4000m, relatorio.Totais.Faturamento);
        Assert.Equal(400m, relatorio.Totais.TicketMedio);
        Assert.Equal(56m, relatorio.Totais.TempoMedioAtendimentoMinutos); // 560 minutos em 10 comandas
        Assert.Equal((new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 16)), factory.Indicadores.PeriodosConsultados.Single());
    }

    [Fact]
    public async Task RelatorioDePracas_MostraTodasAsPracas_InclusiveSemMovimento()
    {
        Praca comMovimento, semMovimento;
        using (var escopo = factory.Services.CreateScope())
        {
            comMovimento = new Praca($"A{Guid.NewGuid():N}"[..8], 2);
            semMovimento = new Praca($"Z{Guid.NewGuid():N}"[..8], 2);
            await escopo.ServiceProvider.GetRequiredService<IRepositorioPraca>().Adicionar(comMovimento);
            await escopo.ServiceProvider.GetRequiredService<IRepositorioPraca>().Adicionar(semMovimento);
            await escopo.ServiceProvider.GetRequiredService<IUnitOfWork>().Commit();
        }
        factory.Indicadores.Pracas.Add(new IndicadorPraca(comMovimento.Id, Faturamento: 1500m, Comandas: 5, Turnos: 3));
        factory.Indicadores.FaturamentoMedioPorPraca[comMovimento.Id] = 480m;

        var relatorio = await Ler<RelatorioPracasResponse>(_gerente, $"{Rota}/pracas");

        var boa = relatorio.Pracas.Single(p => p.PracaId == comMovimento.Id);
        var vazia = relatorio.Pracas.Single(p => p.PracaId == semMovimento.Id);
        Assert.Equal((500m, 300m, 480m), (boa.FaturamentoPorTurno, boa.TicketMedio, boa.FaturamentoMedioHistorico));
        Assert.Equal((0m, 0, 0m), (vazia.Faturamento, vazia.Comandas, vazia.TicketMedio));
    }

    [Fact]
    public async Task RelatorioDoCardapio_AgrupaPorCategoriaComParticipacao()
    {
        var moqueca = await factory.CriarItemCardapio(preco: 100m);
        var suco = await factory.CriarItemCardapio(preco: 10m);
        factory.Indicadores.Itens.Add(new IndicadorItemCardapio(moqueca, "PratoPrincipal", Quantidade: 3, Faturamento: 300m));
        factory.Indicadores.Itens.Add(new IndicadorItemCardapio(suco, "Bebida", Quantidade: 10, Faturamento: 100m));

        var relatorio = await Ler<RelatorioCardapioResponse>(_gerente, $"{Rota}/cardapio");

        Assert.Equal(moqueca, relatorio.Itens[0].ItemCardapioId);
        Assert.StartsWith("Prato", relatorio.Itens[0].Nome);
        Assert.Equal(75m, relatorio.Itens[0].ParticipacaoPercentual);
        Assert.Equal(["PratoPrincipal", "Bebida"], relatorio.Categorias.Select(c => c.Categoria));
        Assert.Equal(25m, relatorio.Categorias[1].ParticipacaoPercentual);
    }

    [Fact]
    public async Task RelatorioDeAvaliacoes_TrazMediaEAsCincoFaixas()
    {
        factory.Indicadores.Avaliacoes.Add(new LinhaAvaliacao(Nota: 5, Quantidade: 6));
        factory.Indicadores.Avaliacoes.Add(new LinhaAvaliacao(Nota: 4, Quantidade: 3));
        factory.Indicadores.Avaliacoes.Add(new LinhaAvaliacao(Nota: 1, Quantidade: 1));

        var relatorio = await Ler<RelatorioAvaliacoesResponse>(_gerente, $"{Rota}/avaliacoes");

        Assert.Equal(10, relatorio.Quantidade);
        Assert.Equal(4.3m, relatorio.Media);
        // As cinco notas sempre aparecem: as faixas em que ninguém votou informam tanto quanto as outras.
        Assert.Equal([1, 2, 3, 4, 5], relatorio.Distribuicao.Select(f => f.Nota));
        Assert.Equal(0, relatorio.Distribuicao[1].Quantidade);
        Assert.Equal(60m, relatorio.Distribuicao[4].Percentual);
    }

    [Fact]
    public async Task RelatorioDeAvaliacoes_SemNenhumaAvaliacao_NaoDividePorZero()
    {
        var relatorio = await Ler<RelatorioAvaliacoesResponse>(_gerente, $"{Rota}/avaliacoes");

        Assert.Equal(0, relatorio.Quantidade);
        Assert.Equal(0m, relatorio.Media);
        Assert.All(relatorio.Distribuicao, f => Assert.Equal(0m, f.Percentual));
    }

    [Fact]
    public async Task RelatorioDeHorarios_TrazHoraEDiaDaSemanaComNome()
    {
        factory.Indicadores.PorHora.Add(new IndicadorPracaNoTempo(1, Fatia: 20, Faturamento: 900m, Comandas: 3));
        factory.Indicadores.PorDiaDaSemana.Add(new IndicadorPracaNoTempo(1, Fatia: 7, Faturamento: 900m, Comandas: 3));

        var relatorio = await Ler<RelatorioHorariosResponse>(_gerente, $"{Rota}/horarios");

        Assert.Equal(20, relatorio.PorHora.Single().Hora);
        Assert.Equal("Sábado", relatorio.PorDiaDaSemana.Single().NomeDoDia);
    }

    [Fact]
    public async Task Relatorios_SaoDoGerente_ERegistramAConsulta()
    {
        var (garcom, _) = await ClienteGarcom();

        var negado = await garcom.GetAsync($"{Rota}/garcons");
        await Ler<RelatorioPracasResponse>(_gerente, $"{Rota}/pracas?inicio=2026-09-01&fim=2026-09-10");

        Assert.Equal(HttpStatusCode.Forbidden, negado.StatusCode);
        var registro = (await factory.Auditoria(EventoAuditoria.RelatorioBiConsultado)).Last();
        Assert.Equal("""{"relatorio":"pracas","inicio":"2026-09-01","fim":"2026-09-10"}""", registro.Detalhes);
        Assert.Equal(factory.IdGerente, registro.UsuarioId);
    }

    [Fact]
    public async Task Ranking_OGerenteVeTodos()
    {
        var (_, ana) = await ClienteGarcom();
        var (_, bia) = await ClienteGarcom();
        factory.Indicadores.Garcons.Add(new IndicadorGarcom(ana.Id, 1000m, 4, Turnos: 1, MesasAtendidas: 4, 0));
        factory.Indicadores.Garcons.Add(new IndicadorGarcom(bia.Id, 500m, 2, Turnos: 1, MesasAtendidas: 2, 0));

        var ranking = await Ler<RankingDesempenhoResponse>(_gerente, $"{Rota}/desempenho");

        Assert.Equal(2, ranking.TotalNoRanking);
        Assert.Equal([ana.Id, bia.Id], ranking.Posicoes.Select(p => p.GarcomId));
        Assert.Equal(ana.Nome, ranking.Posicoes[0].Nome);
        Assert.Equal((0.5m, 0.5m), (ranking.PesoFaturamento, ranking.PesoMesasAtendidas));
        Assert.Contains("\"garcom_consultado\":\"todos\"", (await factory.Auditoria(EventoAuditoria.IndiceDesempenhoConsultado)).Last().Detalhes);
    }

    [Fact]
    public async Task Ranking_OGarcomVeSoAPropriaPosicao_SemNomeDosColegas()
    {
        var (cliente, eu) = await ClienteGarcom();
        var (_, colega) = await ClienteGarcom();
        factory.Indicadores.Garcons.Add(new IndicadorGarcom(colega.Id, 1000m, 4, Turnos: 1, MesasAtendidas: 4, 0));
        factory.Indicadores.Garcons.Add(new IndicadorGarcom(eu.Id, 500m, 2, Turnos: 1, MesasAtendidas: 2, 0));

        var resposta = await cliente.GetAsync($"{Rota}/desempenho");
        var ranking = await resposta.Content.ReadFromJsonAsync<RankingDesempenhoResponse>(Json);
        var json = await (await cliente.GetAsync($"{Rota}/desempenho")).Content.ReadAsStringAsync();

        var minha = Assert.Single(ranking!.Posicoes);
        Assert.Equal((eu.Id, 2), (minha.GarcomId, minha.Posicao));
        Assert.Equal(2, ranking.TotalNoRanking);
        Assert.DoesNotContain($"\"garcomId\":{colega.Id},", json);
        Assert.Contains($"\"garcom_consultado\":\"{eu.Id}\"", (await factory.Auditoria(EventoAuditoria.IndiceDesempenhoConsultado)).Last().Detalhes);
    }
}
