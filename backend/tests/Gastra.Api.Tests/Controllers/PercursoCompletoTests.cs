using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Domain.Servicos;

namespace Gastra.Api.Tests.Controllers;

/// <summary>
/// O percurso do <c>Gastra.Api.Percurso.http</c> (#162), executado: sem tela nenhuma, do cadastro do salão ao
/// fechamento da conta e à alocação confirmada. Se este teste passa, o arquivo .http descreve um caminho que funciona.
/// </summary>
public class PercursoCompletoTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>
{
    private static readonly JsonSerializerOptions Json = GastraApiFactory.Json;

    [Fact]
    public async Task DoCadastroDoSalao_AoFechamentoDaConta_EAAlocacaoConfirmada()
    {
        var cliente = factory.CreateClient();

        // 1. Gerente, com senha e segundo fator, monta o salão, o cardápio e a equipe.
        GastraApiFactory.Autenticar(cliente, await factory.TokenGerente(cliente));
        var pracaA = await Id(cliente.PostAsJsonAsync("/api/pracas", new { codigo = "A", quantidadeGarcons = 1 }, Json));
        var pracaB = await Id(cliente.PostAsJsonAsync("/api/pracas", new { codigo = "B", quantidadeGarcons = 1 }, Json));
        var mesa = await Id(cliente.PostAsJsonAsync("/api/mesas", new { numero = "01", capacidade = 4, pracaId = pracaA }, Json));
        var prato = await Id(cliente.PostAsJsonAsync("/api/cardapio", new
        {
            nome = "Moqueca de peixe", categoria = "PratoPrincipal", preco = 89.00m, descricao = "Para dois.",
            flagsDieteticas = new[] { "SemGluten" },
        }, Json));
        var garcomEmail = $"garcom-{Guid.NewGuid():N}@gastra.test";
        var metreEmail = $"metre-{Guid.NewGuid():N}@gastra.test";
        var garcom = await Id(cliente.PostAsJsonAsync("/api/usuarios", new
        {
            nome = "Ana Souza", email = garcomEmail, senha = GastraApiFactory.SenhaPadrao, papel = "Garcom",
        }, Json));
        await Id(cliente.PostAsJsonAsync("/api/usuarios", new
        {
            nome = "Helena Prado", email = metreEmail, senha = GastraApiFactory.SenhaPadrao, papel = "Metre",
        }, Json));

        // 2. Garçom: abre a mesa, lança, entrega e fecha a conta.
        GastraApiFactory.Autenticar(cliente, await factory.Login(cliente, garcomEmail));
        var comanda = await Id(cliente.PostAsJsonAsync("/api/comandas", new { mesaId = mesa, quantidadePessoas = 2 }, Json));
        (await cliente.PostAsJsonAsync($"/api/comandas/{comanda}/itens", new { itemCardapioId = prato, quantidade = 1 }, Json))
            .EnsureSuccessStatusCode();
        var aberta = await Corpo(cliente.GetAsync($"/api/comandas/{comanda}"));
        var item = aberta.GetProperty("itens")[0].GetProperty("id").GetInt32();
        (await cliente.PatchAsJsonAsync($"/api/comandas/{comanda}/itens/{item}/situacao", new { situacao = "Entregue" }, Json))
            .EnsureSuccessStatusCode();
        var fechada = await Corpo(cliente.PostAsync($"/api/comandas/{comanda}/fechamento", null));
        Assert.Equal(97.90m, fechada.GetProperty("total").GetDecimal()); // 89,00 + 10% de serviço

        // 3. Metre: gera a sugestão do turno, ajusta à mão e confirma.
        GastraApiFactory.Autenticar(cliente, await factory.Login(cliente, metreEmail));
        factory.ServicoAnalitico.ResponderAlocacao = _ => [new DesignacaoSugerida(garcom, pracaA)];
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3)).ToString("yyyy-MM-dd");
        (await cliente.PostAsJsonAsync("/api/alocacoes/sugestao", new { data = hoje, periodo = "Almoco", garcomIds = new[] { garcom } }, Json))
            .EnsureSuccessStatusCode();
        (await cliente.PutAsJsonAsync($"/api/alocacoes/{hoje}/Almoco/garcons/{garcom}", new { pracaId = pracaB }, Json))
            .EnsureSuccessStatusCode();
        var turno = await Corpo(cliente.PostAsync($"/api/alocacoes/{hoje}/Almoco/confirmacao", null));

        Assert.True(turno.GetProperty("confirmada").GetBoolean());
        Assert.Contains(pracaB.ToString(), turno.GetRawText());
    }

    private static async Task<int> Id(Task<HttpResponseMessage> pedido) =>
        (await Corpo(pedido)).GetProperty("id").GetInt32();

    private static async Task<JsonElement> Corpo(Task<HttpResponseMessage> pedido)
    {
        var resposta = await pedido;
        var texto = await resposta.Content.ReadAsStringAsync();
        Assert.True(resposta.IsSuccessStatusCode, $"{resposta.RequestMessage?.Method} {resposta.RequestMessage?.RequestUri}: {(int)resposta.StatusCode} {texto}");
        return JsonDocument.Parse(texto).RootElement;
    }
}
