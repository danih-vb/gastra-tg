using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Communication.Responses;
using Gastra.Domain.Alocacoes;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Servicos;
using Microsoft.Extensions.DependencyInjection;
using static Gastra.Api.Tests.Infraestrutura.GastraApiFactory;

namespace Gastra.Api.Tests.Controllers;

/// <summary>UC15, UC21 e UC22 — alocação de garçons por turno, com o Python substituído por um falso.</summary>
public class AlocacaoControllerTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>, IAsyncLifetime
{
    private const string Rota = "/api/alocacoes";
    private static int _dias;

    private readonly HttpClient _metre = factory.CreateClient();

    public async Task InitializeAsync()
    {
        factory.ServicoAnalitico.Reiniciar();
        var email = $"metre-{Guid.NewGuid():N}@gastra.test";
        await factory.CriarUsuario(email, PapelUsuario.Metre);
        Autenticar(_metre, await factory.Login(factory.CreateClient(), email));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // Cada teste usa um dia só dele: a fábrica (e o banco em memória) é compartilhada pela classe.
    private static DateOnly NovoDia() => new DateOnly(2026, 10, 1).AddDays(Interlocked.Increment(ref _dias) * 3);

    private async Task<Usuario> CriarGarcom() =>
        await factory.CriarUsuario($"garcom-{Guid.NewGuid():N}@gastra.test", PapelUsuario.Garcom);

    private async Task<Praca> CriarPraca(int vagas)
    {
        using var escopo = factory.Services.CreateScope();
        var praca = new Praca($"P{Guid.NewGuid():N}"[..8], vagas);
        await escopo.ServiceProvider.GetRequiredService<IRepositorioPraca>().Adicionar(praca);
        await escopo.ServiceProvider.GetRequiredService<IUnitOfWork>().Commit();
        return praca;
    }

    private Task<HttpResponseMessage> PedirSugestao(DateOnly data, params int[] garcomIds) =>
        _metre.PostAsJsonAsync($"{Rota}/sugestao", new { data, periodo = "Jantar", garcomIds }, Json);

    private static async Task<AlocacaoTurnoResponse> Ler(HttpResponseMessage resposta)
    {
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<AlocacaoTurnoResponse>(Json))!;
    }

    private async Task Confirmar(DateOnly data) =>
        (await _metre.PostAsync($"{Rota}/{data:yyyy-MM-dd}/Jantar/confirmacao", null)).EnsureSuccessStatusCode();

    // --- UC15: sugestão ---

    [Fact]
    public async Task Sugestao_EnviaAoPythonOsFatoresDaRN03_EGravaOResultado()
    {
        var boa = await CriarPraca(vagas: 1);
        var fraca = await CriarPraca(vagas: 1);
        var a = await CriarGarcom();
        var b = await CriarGarcom();
        factory.Indicadores.FaturamentoMedioPorPraca[boa.Id] = 1_000_000m; // muito acima da média: alto potencial
        factory.Indicadores.FaturamentoMedioPorPraca[fraca.Id] = 1m;
        factory.Indicadores.FaturamentoPorTurnoDoGarcom[a.Id] = 900m;
        factory.Indicadores.FaturamentoPorTurnoDoGarcom[b.Id] = 300m;

        // Histórico: b trabalhou dois turnos confirmados na praça fraca; a esteve na boa no último turno.
        var dia = NovoDia();
        await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia.AddDays(-2):yyyy-MM-dd}/Jantar/garcons/{b.Id}", new { pracaId = fraca.Id }, Json));
        await Confirmar(dia.AddDays(-2));
        await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia.AddDays(-1):yyyy-MM-dd}/Jantar/garcons/{b.Id}", new { pracaId = fraca.Id }, Json));
        await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia.AddDays(-1):yyyy-MM-dd}/Jantar/garcons/{a.Id}", new { pracaId = boa.Id }, Json));
        await Confirmar(dia.AddDays(-1));

        factory.ServicoAnalitico.ResponderAlocacao = _ => [new DesignacaoSugerida(b.Id, boa.Id), new DesignacaoSugerida(a.Id, fraca.Id)];

        var turno = await Ler(await PedirSugestao(dia, a.Id, b.Id));

        var chamada = Assert.Single(factory.ServicoAnalitico.ChamadasAlocacao);
        var fatorA = chamada.Garcons.Single(g => g.GarcomId == a.Id);
        var fatorB = chamada.Garcons.Single(g => g.GarcomId == b.Id);
        Assert.Equal((900m, 0), (fatorA.FaturamentoPorTurno, fatorA.TurnosDesdePracaDeAltoPotencial));
        Assert.Equal((300m, 2), (fatorB.FaturamentoPorTurno, fatorB.TurnosDesdePracaDeAltoPotencial));
        Assert.Contains(new PracaParaAlocacao(boa.Id, 1, 1_000_000m), chamada.Pracas);
        Assert.Equal(RegraDeDistribuicao.PesoDesequilibrio, chamada.PesoDesequilibrio);
        Assert.Contains((dia.AddDays(-RegraDeDistribuicao.DiasDaJanelaDeFaturamento), dia), factory.Indicadores.PeriodosConsultados);

        Assert.True(turno.ServicoDisponivel);
        Assert.False(turno.Confirmada);
        Assert.Equal(boa.Id, turno.Designacoes.Single(d => d.GarcomId == b.Id).PracaId);
        Assert.Equal(boa.Codigo, turno.Designacoes.Single(d => d.GarcomId == b.Id).PracaCodigo);
        Assert.StartsWith("Usuário", turno.Designacoes[0].GarcomNome);

        var gravado = await _metre.GetFromJsonAsync<AlocacaoTurnoResponse>($"{Rota}/{dia:yyyy-MM-dd}/Jantar", Json);
        Assert.Equal(2, gravado!.Designacoes.Count);
    }

    [Fact]
    public async Task Sugestao_GeradaDeNovo_SubstituiAAnteriorNaoConfirmada()
    {
        await CriarPraca(vagas: 2);
        var a = await CriarGarcom();
        var b = await CriarGarcom();
        var dia = NovoDia();

        await Ler(await PedirSugestao(dia, a.Id, b.Id));
        var segunda = await Ler(await PedirSugestao(dia, a.Id));

        Assert.Equal([a.Id], segunda.Designacoes.Select(d => d.GarcomId));
        var gravado = await _metre.GetFromJsonAsync<AlocacaoTurnoResponse>($"{Rota}/{dia:yyyy-MM-dd}/Jantar", Json);
        Assert.Equal([a.Id], gravado!.Designacoes.Select(d => d.GarcomId));
    }

    [Fact]
    public async Task Sugestao_ComPythonFora_Responde200ParaAlocacaoManual_ERegistraAFalha()
    {
        await CriarPraca(vagas: 2);
        var a = await CriarGarcom();
        factory.ServicoAnalitico.Indisponivel = true;
        var dia = NovoDia();

        var turno = await Ler(await PedirSugestao(dia, a.Id));

        Assert.False(turno.ServicoDisponivel);
        Assert.Empty(turno.Designacoes);
        var falha = (await factory.Auditoria(EventoAuditoria.SugestaoAlocacaoGerada)).Last();
        Assert.Equal(ResultadoAuditoria.Falha, falha.Resultado);
        Assert.Contains("servico_analitico_indisponivel", falha.Detalhes);
    }

    [Fact]
    public async Task Sugestao_ComRespostaIncoerenteDoPython_NaoGravaNada()
    {
        var praca = await CriarPraca(vagas: 1);
        var a = await CriarGarcom();
        var b = await CriarGarcom();
        // Deixa o garçom b de fora: nunca pode ir para o banco.
        factory.ServicoAnalitico.ResponderAlocacao = _ => [new DesignacaoSugerida(a.Id, praca.Id)];
        var dia = NovoDia();

        var turno = await Ler(await PedirSugestao(dia, a.Id, b.Id));

        Assert.False(turno.ServicoDisponivel);
        var gravado = await _metre.GetFromJsonAsync<AlocacaoTurnoResponse>($"{Rota}/{dia:yyyy-MM-dd}/Jantar", Json);
        Assert.Empty(gravado!.Designacoes);
    }

    [Fact]
    public async Task Sugestao_SemGarcons_Retorna400()
    {
        var resposta = await PedirSugestao(NovoDia());

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Sugestao_ComQuemNaoEGarcom_Retorna422()
    {
        await CriarPraca(vagas: 2);
        var metre = await factory.CriarUsuario($"outro-metre-{Guid.NewGuid():N}@gastra.test", PapelUsuario.Metre);

        var resposta = await PedirSugestao(NovoDia(), metre.Id);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Sugestao_SemPracasCadastradas_Retorna422()
    {
        await using var semPracas = new GastraApiFactory();
        var email = $"metre-{Guid.NewGuid():N}@gastra.test";
        await semPracas.CriarUsuario(email, PapelUsuario.Metre);
        var garcom = await semPracas.CriarUsuario($"garcom-{Guid.NewGuid():N}@gastra.test", PapelUsuario.Garcom);
        var cliente = semPracas.CreateClient();
        Autenticar(cliente, await semPracas.Login(semPracas.CreateClient(), email));

        var resposta = await cliente.PostAsJsonAsync($"{Rota}/sugestao", new { data = NovoDia(), periodo = "Almoco", garcomIds = new[] { garcom.Id } }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Sugestao_DepoisDeConfirmado_Retorna422()
    {
        await CriarPraca(vagas: 2);
        var a = await CriarGarcom();
        var dia = NovoDia();
        await Ler(await PedirSugestao(dia, a.Id));
        await Confirmar(dia);

        var resposta = await PedirSugestao(dia, a.Id);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    // --- UC22: ajuste ---

    [Fact]
    public async Task Ajuste_TrocaAPraca_ERegistraSugeridaEEscolhida()
    {
        var primeira = await CriarPraca(vagas: 1);
        var segunda = await CriarPraca(vagas: 1);
        var a = await CriarGarcom();
        factory.ServicoAnalitico.ResponderAlocacao = _ => [new DesignacaoSugerida(a.Id, primeira.Id)];
        var dia = NovoDia();
        await Ler(await PedirSugestao(dia, a.Id));

        var turno = await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{a.Id}", new { pracaId = segunda.Id }, Json));

        Assert.Equal(segunda.Id, turno.Designacoes.Single().PracaId);
        var ajuste = (await factory.Auditoria(EventoAuditoria.AlocacaoAjustada)).Last();
        Assert.Contains($"\"praca_sugerida\":{primeira.Id}", ajuste.Detalhes);
        Assert.Contains($"\"praca_escolhida\":{segunda.Id}", ajuste.Detalhes);
    }

    [Fact]
    public async Task Ajuste_ParaPracaLotada_Retorna422()
    {
        var praca = await CriarPraca(vagas: 1);
        var a = await CriarGarcom();
        var b = await CriarGarcom();
        var dia = NovoDia();
        await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{a.Id}", new { pracaId = praca.Id }, Json));

        var resposta = await _metre.PutAsJsonAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{b.Id}", new { pracaId = praca.Id }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Ajuste_DepoisDeConfirmado_Retorna422()
    {
        var praca = await CriarPraca(vagas: 2);
        var a = await CriarGarcom();
        var dia = NovoDia();
        await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{a.Id}", new { pracaId = praca.Id }, Json));
        await Confirmar(dia);

        var resposta = await _metre.PutAsJsonAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{a.Id}", new { pracaId = praca.Id }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    // --- UC22: troca entre garçons (#140) ---

    [Fact]
    public async Task Ajuste_ComTroca_ComAsPracasCheias_TrocaOsDoisLados_ERegistraAsDuasMudancas()
    {
        // Turno com um garçom para cada vaga: sem troca, nenhum ajuste seria possível.
        var primeira = await CriarPraca(vagas: 1);
        var segunda = await CriarPraca(vagas: 1);
        var a = await CriarGarcom();
        var b = await CriarGarcom();
        var dia = NovoDia();
        factory.ServicoAnalitico.ResponderAlocacao = _ => [new DesignacaoSugerida(a.Id, primeira.Id), new DesignacaoSugerida(b.Id, segunda.Id)];
        await Ler(await PedirSugestao(dia, a.Id, b.Id));

        var turno = await Ler(await _metre.PutAsJsonAsync(
            $"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{a.Id}", new { pracaId = segunda.Id, trocarComGarcomId = b.Id }, Json));

        Assert.Equal(segunda.Id, turno.Designacoes.Single(d => d.GarcomId == a.Id).PracaId);
        Assert.Equal(primeira.Id, turno.Designacoes.Single(d => d.GarcomId == b.Id).PracaId);

        var gravado = await _metre.GetFromJsonAsync<AlocacaoTurnoResponse>($"{Rota}/{dia:yyyy-MM-dd}/Jantar", Json);
        Assert.Equal(primeira.Id, gravado!.Designacoes.Single(d => d.GarcomId == b.Id).PracaId);

        var ajustes = (await factory.Auditoria(EventoAuditoria.AlocacaoAjustada)).TakeLast(2).ToList();
        Assert.Contains(ajustes, x => x.Detalhes.Contains($"\"garcom_id\":{a.Id}")
                                      && x.Detalhes.Contains($"\"praca_escolhida\":{segunda.Id}")
                                      && x.Detalhes.Contains($"\"troca_com_garcom_id\":{b.Id}"));
        Assert.Contains(ajustes, x => x.Detalhes.Contains($"\"garcom_id\":{b.Id}")
                                      && x.Detalhes.Contains($"\"praca_escolhida\":{primeira.Id}")
                                      && x.Detalhes.Contains($"\"troca_com_garcom_id\":{a.Id}"));
    }

    [Fact]
    public async Task Ajuste_ComTroca_DeQuemAindaNaoTemPraca_DeixaOOutroSemPraca()
    {
        var praca = await CriarPraca(vagas: 1);
        var comPraca = await CriarGarcom();
        var semPraca = await CriarGarcom();
        var dia = NovoDia();
        await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{comPraca.Id}", new { pracaId = praca.Id }, Json));

        var turno = await Ler(await _metre.PutAsJsonAsync(
            $"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{semPraca.Id}", new { pracaId = praca.Id, trocarComGarcomId = comPraca.Id }, Json));

        Assert.Equal([semPraca.Id], turno.Designacoes.Select(d => d.GarcomId));
        var gravado = await _metre.GetFromJsonAsync<AlocacaoTurnoResponse>($"{Rota}/{dia:yyyy-MM-dd}/Jantar", Json);
        Assert.Equal([semPraca.Id], gravado!.Designacoes.Select(d => d.GarcomId));
    }

    [Fact]
    public async Task Ajuste_ComTroca_ComGarcomDeOutraPraca_Retorna422_ENaoMudaNada()
    {
        var destino = await CriarPraca(vagas: 1);
        var outra = await CriarPraca(vagas: 1);
        var a = await CriarGarcom();
        var b = await CriarGarcom();
        var dia = NovoDia();
        await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{a.Id}", new { pracaId = outra.Id }, Json));
        await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{b.Id}", new { pracaId = destino.Id }, Json));

        // b está na praça de destino; a troca pede o próprio a, que está na outra praça.
        var resposta = await _metre.PutAsJsonAsync(
            $"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{b.Id}", new { pracaId = outra.Id, trocarComGarcomId = b.Id }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        var gravado = await _metre.GetFromJsonAsync<AlocacaoTurnoResponse>($"{Rota}/{dia:yyyy-MM-dd}/Jantar", Json);
        Assert.Equal(destino.Id, gravado!.Designacoes.Single(d => d.GarcomId == b.Id).PracaId);
    }

    [Fact]
    public async Task Ajuste_ComTroca_ComQuemNaoEstaNaPracaDeDestino_Retorna422()
    {
        var destino = await CriarPraca(vagas: 1);
        var outra = await CriarPraca(vagas: 2);
        var a = await CriarGarcom();
        var b = await CriarGarcom();
        var dia = NovoDia();
        await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{a.Id}", new { pracaId = outra.Id }, Json));
        await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{b.Id}", new { pracaId = outra.Id }, Json));

        var resposta = await _metre.PutAsJsonAsync(
            $"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{a.Id}", new { pracaId = destino.Id, trocarComGarcomId = b.Id }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [Fact]
    public async Task Ajuste_ComTroca_NaMesmaPraca_Retorna422()
    {
        var praca = await CriarPraca(vagas: 2);
        var a = await CriarGarcom();
        var b = await CriarGarcom();
        var dia = NovoDia();
        await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{a.Id}", new { pracaId = praca.Id }, Json));
        await Ler(await _metre.PutAsJsonAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{b.Id}", new { pracaId = praca.Id }, Json));

        var resposta = await _metre.PutAsJsonAsync(
            $"{Rota}/{dia:yyyy-MM-dd}/Jantar/garcons/{a.Id}", new { pracaId = praca.Id, trocarComGarcomId = b.Id }, Json);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    // --- UC21: confirmação ---

    [Fact]
    public async Task Confirmacao_TravaOTurno_ERegistraNaAuditoria()
    {
        await CriarPraca(vagas: 2);
        var a = await CriarGarcom();
        var dia = NovoDia();
        await Ler(await PedirSugestao(dia, a.Id));

        var turno = await Ler(await _metre.PostAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar/confirmacao", null));

        Assert.True(turno.Confirmada);
        Assert.Contains($"\"data\":\"{dia:yyyy-MM-dd}\"", (await factory.Auditoria(EventoAuditoria.AlocacaoConfirmada)).Last().Detalhes);
    }

    [Fact]
    public async Task Confirmacao_DeTurnoSemAlocacao_Retorna404()
    {
        var resposta = await _metre.PostAsync($"{Rota}/{NovoDia():yyyy-MM-dd}/Almoco/confirmacao", null);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    // --- Permissões e rota ---

    [Fact]
    public async Task Garcom_NaoGeraSugestao_MasConsultaOTurno()
    {
        var email = $"garcom-{Guid.NewGuid():N}@gastra.test";
        var garcom = await factory.CriarUsuario(email, PapelUsuario.Garcom);
        var cliente = factory.CreateClient();
        Autenticar(cliente, await factory.Login(factory.CreateClient(), email));
        var dia = NovoDia();

        var sugestao = await cliente.PostAsJsonAsync($"{Rota}/sugestao", new { data = dia, periodo = "Jantar", garcomIds = new[] { garcom.Id } }, Json);
        var consulta = await cliente.GetAsync($"{Rota}/{dia:yyyy-MM-dd}/Jantar");

        Assert.Equal(HttpStatusCode.Forbidden, sugestao.StatusCode);
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
    }

    [Fact]
    public async Task PeriodoInexistenteNaRota_Retorna400()
    {
        var resposta = await _metre.GetAsync($"{Rota}/{NovoDia():yyyy-MM-dd}/Madrugada");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
