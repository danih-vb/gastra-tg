using System.Net;
using System.Net.Http.Json;
using Gastra.Api.Tests.Infraestrutura;
using Gastra.Communication.Responses;
using Gastra.Domain.Auditoria;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using static Gastra.Api.Tests.Infraestrutura.GastraApiFactory;

namespace Gastra.Api.Tests.Controllers;

/// <summary>
/// Eventos da política de log (GASTRA_Politica_Log_Auditoria.md, seção 4): o que é registrado e, principalmente,
/// o que nunca pode aparecer na auditoria.
/// </summary>
public class AuditoriaTests(GastraApiFactory factory) : IClassFixture<GastraApiFactory>
{
    private async Task<HttpClient> ClienteGerente()
    {
        var cliente = factory.CreateClient();
        Autenticar(cliente, await factory.TokenGerente(factory.CreateClient()));
        return cliente;
    }

    private async Task<(HttpClient Cliente, int Id)> ClienteGarcom()
    {
        var email = $"garcom-{Guid.NewGuid():N}@gastra.test";
        var garcom = await factory.CriarUsuario(email, PapelUsuario.Garcom);
        var cliente = factory.CreateClient();
        Autenticar(cliente, await factory.Login(factory.CreateClient(), email));
        return (cliente, garcom.Id);
    }

    private static void SemDadoProibido(RegistroAuditoria registro, params string[] proibidos)
    {
        var texto = $"{registro.Detalhes} {registro.Entidade}";
        foreach (var proibido in proibidos)
            Assert.DoesNotContain(proibido, texto, StringComparison.OrdinalIgnoreCase);
    }

    // --- 4.1 Autenticação e contas ---

    [Fact]
    public async Task Login_ComSucesso_RegistraAtorPapelEIp()
    {
        var email = $"metre-{Guid.NewGuid():N}@gastra.test";
        var metre = await factory.CriarUsuario(email, PapelUsuario.Metre);

        await factory.Login(factory.CreateClient(), email);

        var registro = (await factory.Auditoria(EventoAuditoria.LoginSucesso)).Last(r => r.UsuarioId == metre.Id);
        Assert.Equal(ResultadoAuditoria.Sucesso, registro.Resultado);
        Assert.Equal(PapelUsuario.Metre, registro.Papel);
        Assert.Equal(IpDeTeste, registro.Ip);
        Assert.False(string.IsNullOrWhiteSpace(registro.IdCorrelacao));
        SemDadoProibido(registro, email, SenhaPadrao);
    }

    [Fact]
    public async Task Login_ComXForwardedForDeQuemNaoEProxy_IgnoraOCabecalho()
    {
        var email = $"metre-{Guid.NewGuid():N}@gastra.test";
        var metre = await factory.CriarUsuario(email, PapelUsuario.Metre);
        var cliente = factory.CreateClient();
        cliente.DefaultRequestHeaders.Add("X-Forwarded-For", "198.51.100.99");

        await factory.Login(cliente, email);

        // O IP de teste não está numa rede confiável (#230): quem chama direto não escolhe o IP que vai para o log.
        var registro = (await factory.Auditoria(EventoAuditoria.LoginSucesso)).Last(r => r.UsuarioId == metre.Id);
        Assert.Equal(IpDeTeste, registro.Ip);
    }

    [Fact]
    public async Task Login_ComSenhaErrada_RegistraFalhaComAContaEOMotivo_SemASenha()
    {
        var email = $"garcom-{Guid.NewGuid():N}@gastra.test";
        var garcom = await factory.CriarUsuario(email, PapelUsuario.Garcom);

        var resposta = await factory.CreateClient().PostAsJsonAsync("/api/autenticacao/login",
            new { email, senha = "senha-errada-123" }, Json);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        var registro = (await factory.Auditoria(EventoAuditoria.LoginFalha)).Last(r => r.UsuarioId == garcom.Id);
        Assert.Equal(ResultadoAuditoria.Falha, registro.Resultado);
        Assert.Contains("senha_incorreta", registro.Detalhes);
        Assert.Equal(IpDeTeste, registro.Ip);
        SemDadoProibido(registro, "senha-errada-123", email);
    }

    [Fact]
    public async Task Login_QuintaSenhaErrada_RegistraQueBloqueouEDepoisOMotivoContaBloqueada()
    {
        var email = $"garcom-{Guid.NewGuid():N}@gastra.test";
        var garcom = await factory.CriarUsuario(email, PapelUsuario.Garcom);
        var cliente = factory.CreateClient();

        for (var i = 0; i < 5; i++)
            await cliente.PostAsJsonAsync("/api/autenticacao/login", new { email, senha = "senha-errada-123" }, Json);
        await cliente.PostAsJsonAsync("/api/autenticacao/login", new { email, senha = SenhaPadrao }, Json);

        var falhas = (await factory.Auditoria(EventoAuditoria.LoginFalha)).Where(r => r.UsuarioId == garcom.Id).ToList();
        Assert.Equal(6, falhas.Count);
        Assert.Contains("conta_bloqueada\":true", falhas[4].Detalhes!.Replace(" ", ""));
        Assert.Contains("\"conta_bloqueada\"", falhas[5].Detalhes!.Replace(" ", ""));
        Assert.All(falhas, r => SemDadoProibido(r, "senha-errada-123", email));
    }

    [Fact]
    public async Task Login_ComEmailInexistente_NaoGuardaOEmailDigitado()
    {
        var emailDigitado = $"alguem-{Guid.NewGuid():N}@exemplo.test";

        await factory.CreateClient().PostAsJsonAsync("/api/autenticacao/login",
            new { email = emailDigitado, senha = "qualquer-coisa-1" }, Json);

        var registros = await factory.Auditoria(EventoAuditoria.LoginFalha);
        var semConta = registros.Where(r => r.UsuarioId is null).ToList();
        Assert.NotEmpty(semConta);
        Assert.All(semConta, r => Assert.Contains("conta_nao_encontrada", r.Detalhes));
        Assert.All(registros, r => SemDadoProibido(r, emailDigitado, "qualquer-coisa-1"));
    }

    [Fact]
    public async Task Login_DeContaInativa_RegistraOMotivo()
    {
        var email = $"inativo-{Guid.NewGuid():N}@gastra.test";
        var inativo = await factory.CriarUsuario(email, PapelUsuario.Garcom, ativo: false);

        await factory.CreateClient().PostAsJsonAsync("/api/autenticacao/login", new { email, senha = SenhaPadrao }, Json);

        var registro = (await factory.Auditoria(EventoAuditoria.LoginFalha)).Last(r => r.UsuarioId == inativo.Id);
        Assert.Contains("conta_inativa", registro.Detalhes);
    }

    [Fact]
    public async Task SegundoFator_RegistraVinculacaoConfirmacaoERecusa_SemCodigoNemSegredo()
    {
        var email = $"coordenador-{Guid.NewGuid():N}@gastra.test";
        var coordenador = await factory.CriarUsuario(email, PapelUsuario.Coordenador);
        var cliente = factory.CreateClient();

        var login = await (await cliente.PostAsJsonAsync("/api/autenticacao/login", new { email, senha = SenhaPadrao }, Json))
            .Content.ReadFromJsonAsync<LoginResponse>(Json);
        var configuracao = await (await cliente.PostAsJsonAsync("/api/autenticacao/segundo-fator/configurar",
                new { tokenSegundoFator = login!.TokenSegundoFator }, Json))
            .Content.ReadFromJsonAsync<ConfiguracaoSegundoFatorResponse>(Json);

        var codigoCorreto = CodigoTotp(configuracao!.ChaveManual);
        var codigoErrado = codigoCorreto == "000000" ? "111111" : "000000";
        var recusa = await cliente.PostAsJsonAsync("/api/autenticacao/segundo-fator/confirmar",
            new { tokenSegundoFator = login.TokenSegundoFator, codigo = codigoErrado }, Json);
        var confirmacao = await cliente.PostAsJsonAsync("/api/autenticacao/segundo-fator/confirmar",
            new { tokenSegundoFator = login.TokenSegundoFator, codigo = codigoCorreto }, Json);

        Assert.Equal(HttpStatusCode.Unauthorized, recusa.StatusCode);
        Assert.Equal(HttpStatusCode.OK, confirmacao.StatusCode);

        var vinculado = Assert.Single(await factory.Auditoria(EventoAuditoria.AutenticadorVinculado), r => r.UsuarioId == coordenador.Id);
        var recusado = Assert.Single(await factory.Auditoria(EventoAuditoria.SegundoFatorRecusado), r => r.UsuarioId == coordenador.Id);
        var confirmado = Assert.Single(await factory.Auditoria(EventoAuditoria.SegundoFatorConfirmado), r => r.UsuarioId == coordenador.Id);
        var loginComPendencia = (await factory.Auditoria(EventoAuditoria.LoginSucesso)).Last(r => r.UsuarioId == coordenador.Id);

        Assert.Equal(ResultadoAuditoria.Falha, recusado.Resultado);
        Assert.Contains("segundo_fator_pendente", loginComPendencia.Detalhes);
        foreach (var registro in new[] { vinculado, recusado, confirmado })
        {
            Assert.Equal(IpDeTeste, registro.Ip);
            SemDadoProibido(registro, codigoErrado, codigoCorreto, configuracao.ChaveManual);
        }
    }

    [Fact]
    public async Task Logoff_RegistraOAtor()
    {
        var (garcom, id) = await ClienteGarcom();

        await garcom.PostAsync("/api/autenticacao/logoff", null);

        Assert.Single(await factory.Auditoria(EventoAuditoria.Logoff), r => r.UsuarioId == id);
    }

    [Fact]
    public async Task Contas_RegistramCriacaoEdicaoComPapelAnteriorENovo_SemNomeNemEmail()
    {
        var gerente = await ClienteGerente();
        var email = $"carlos-{Guid.NewGuid():N}@gastra.test";

        var criacao = await gerente.PostAsJsonAsync("/api/usuarios",
            new { nome = "Carlos Lima", email, senha = SenhaPadrao, papel = "Garcom" }, Json);
        var conta = await criacao.Content.ReadFromJsonAsync<UsuarioResponse>(Json);
        var novoEmail = $"carlos.lima-{Guid.NewGuid():N}@gastra.test";
        await gerente.PutAsJsonAsync($"/api/usuarios/{conta!.Id}", new { nome = "Carlos A. Lima", email = novoEmail, papel = "Metre" }, Json);
        await gerente.PatchAsJsonAsync($"/api/usuarios/{conta.Id}/situacao", new { ativo = false }, Json);
        await gerente.PatchAsJsonAsync($"/api/usuarios/{conta.Id}/situacao", new { ativo = true }, Json);

        var criada = Assert.Single(await factory.Auditoria(EventoAuditoria.ContaCriada), r => r.IdEntidade == conta.Id);
        var editada = Assert.Single(await factory.Auditoria(EventoAuditoria.ContaEditada), r => r.IdEntidade == conta.Id);
        Assert.Single(await factory.Auditoria(EventoAuditoria.ContaInativada), r => r.IdEntidade == conta.Id);
        Assert.Single(await factory.Auditoria(EventoAuditoria.ContaReativada), r => r.IdEntidade == conta.Id);

        Assert.Equal(factory.IdGerente, criada.UsuarioId);
        Assert.Equal(nameof(Usuario), criada.Entidade);
        Assert.Equal("""{"campos_alterados":["nome","email","papel"],"papel_anterior":"Garcom","papel_novo":"Metre"}""", editada.Detalhes);
        Assert.Null(editada.Ip);
        foreach (var registro in new[] { criada, editada })
            SemDadoProibido(registro, "Carlos", email, novoEmail, SenhaPadrao);
    }

    // --- 4.2 Cardápio e salão ---

    [Fact]
    public async Task Cardapio_RegistraCadastroPrecoAnteriorENovoEDisponibilidade()
    {
        var gerente = await ClienteGerente();
        var cadastro = await gerente.PostAsJsonAsync("/api/cardapio",
            new { nome = "Moqueca", categoria = "PratoPrincipal", preco = 89.90m, descricao = "", flagsDieteticas = Array.Empty<string>() }, Json);
        var item = await cadastro.Content.ReadFromJsonAsync<ItemCardapioResponse>(Json);

        await gerente.PatchAsJsonAsync($"/api/cardapio/{item!.Id}/preco", new { preco = 95.50m }, Json);
        await gerente.PatchAsJsonAsync($"/api/cardapio/{item.Id}/disponibilidade", new { disponivel = false }, Json);

        Assert.Single(await factory.Auditoria(EventoAuditoria.ItemCardapioCadastrado), r => r.IdEntidade == item.Id);
        var preco = Assert.Single(await factory.Auditoria(EventoAuditoria.PrecoAlterado), r => r.IdEntidade == item.Id);
        var disponibilidade = Assert.Single(await factory.Auditoria(EventoAuditoria.DisponibilidadeAlterada), r => r.IdEntidade == item.Id);

        Assert.Equal("""{"preco_anterior":89.90,"preco_novo":95.50}""", preco.Detalhes);
        Assert.Equal("""{"disponivel":false}""", disponibilidade.Detalhes);
        Assert.Equal(PapelUsuario.Gerente, preco.Papel);
    }

    [Fact]
    public async Task Salao_RegistraCadastroEEdicaoDePracaEMesa()
    {
        var gerente = await ClienteGerente();
        var codigo = $"P{Guid.NewGuid():N}"[..8];
        var praca = await (await gerente.PostAsJsonAsync("/api/pracas", new { codigo, quantidadeGarcons = 2 }, Json))
            .Content.ReadFromJsonAsync<PracaResponse>(Json);
        await gerente.PutAsJsonAsync($"/api/pracas/{praca!.Id}", new { codigo, quantidadeGarcons = 3 }, Json);
        var numero = $"M{Guid.NewGuid():N}"[..6];
        var mesa = await (await gerente.PostAsJsonAsync("/api/mesas", new { numero, capacidade = 4, pracaId = praca.Id }, Json))
            .Content.ReadFromJsonAsync<MesaResponse>(Json);
        await gerente.PutAsJsonAsync($"/api/mesas/{mesa!.Id}", new { numero, capacidade = 6 }, Json);

        Assert.Single(await factory.Auditoria(EventoAuditoria.PracaCadastrada), r => r.IdEntidade == praca.Id);
        var pracaEditada = Assert.Single(await factory.Auditoria(EventoAuditoria.PracaEditada), r => r.IdEntidade == praca.Id);
        Assert.Single(await factory.Auditoria(EventoAuditoria.MesaCadastrada), r => r.IdEntidade == mesa.Id);
        var mesaEditada = Assert.Single(await factory.Auditoria(EventoAuditoria.MesaEditada), r => r.IdEntidade == mesa.Id);

        Assert.Contains("\"quantidade_garcons\":3", pracaEditada.Detalhes);
        Assert.Contains("\"capacidade\":6", mesaEditada.Detalhes);
    }

    // --- 4.3 Comandas ---

    [Fact]
    public async Task Comanda_RegistraCicloCompleto_SemCodigoDeAcessoNemRestricao()
    {
        var (garcom, idGarcom) = await ClienteGarcom();
        var mesaId = await factory.CriarMesa();
        var itemId = await factory.CriarItemCardapio(preco: 100m);

        var comanda = await (await garcom.PostAsJsonAsync("/api/comandas", new { mesaId, quantidadePessoas = 4 }, Json))
            .Content.ReadFromJsonAsync<ComandaResponse>(Json);
        var rota = $"/api/comandas/{comanda!.Id}";
        await garcom.PatchAsJsonAsync($"{rota}/composicao", new { quantidadePessoas = 4, composicao = "GrupoGrande" }, Json);
        var entregue = await (await garcom.PostAsJsonAsync($"{rota}/itens", new { itemCardapioId = itemId, quantidade = 2 }, Json))
            .Content.ReadFromJsonAsync<ItemPedidoResponse>(Json);
        var cancelado = await (await garcom.PostAsJsonAsync($"{rota}/itens", new { itemCardapioId = itemId, quantidade = 1 }, Json))
            .Content.ReadFromJsonAsync<ItemPedidoResponse>(Json);
        await garcom.PatchAsJsonAsync($"{rota}/itens/{entregue!.Id}/situacao", new { situacao = "Entregue" }, Json);
        await garcom.PatchAsJsonAsync($"{rota}/itens/{cancelado!.Id}/situacao",
            new { situacao = "Cancelado", motivoCancelamento = "ClienteDesistiu" }, Json);
        await garcom.PostAsJsonAsync($"{rota}/restricoes", new { categoria = "SemGluten", observacaoLivre = "doença celíaca" }, Json);
        await garcom.DeleteAsync($"{rota}/taxa-servico");
        var fechamento = await garcom.PostAsync($"{rota}/fechamento", null);
        Assert.Equal(HttpStatusCode.OK, fechamento.StatusCode);

        var aberta = Assert.Single(await factory.Auditoria(EventoAuditoria.ComandaAberta), r => r.IdEntidade == comanda.Id);
        var composicao = Assert.Single(await factory.Auditoria(EventoAuditoria.ComposicaoAjustada), r => r.IdEntidade == comanda.Id);
        var itens = (await factory.Auditoria(EventoAuditoria.ItemRegistrado)).Where(r => r.IdEntidade == comanda.Id).ToList();
        var itemCancelado = Assert.Single(await factory.Auditoria(EventoAuditoria.ItemCancelado), r => r.IdEntidade == cancelado.Id);
        var restricao = Assert.Single(await factory.Auditoria(EventoAuditoria.RestricaoRegistrada), r => r.IdEntidade == comanda.Id);
        Assert.Single(await factory.Auditoria(EventoAuditoria.TaxaServicoRemovida), r => r.IdEntidade == comanda.Id);
        var fechada = Assert.Single(await factory.Auditoria(EventoAuditoria.ComandaFechada), r => r.IdEntidade == comanda.Id);

        Assert.Equal(idGarcom, aberta.UsuarioId);
        Assert.Equal(PapelUsuario.Garcom, aberta.Papel);
        Assert.Contains($"\"mesa_id\":{mesaId}", aberta.Detalhes);
        Assert.Equal("""{"quantidade_pessoas":4,"composicao_sugerida":"GrupoPequeno","composicao_confirmada":"GrupoGrande"}""", composicao.Detalhes);
        Assert.Equal(2, itens.Count);
        Assert.Contains("\"motivo\":\"ClienteDesistiu\"", itemCancelado.Detalhes);
        Assert.Null(restricao.Detalhes);
        Assert.Equal("""{"total":200}""", fechada.Detalhes);

        var todos = new[] { aberta, composicao, itemCancelado, restricao, fechada }.Concat(itens);
        Assert.All(todos, r => SemDadoProibido(r, comanda.CodigoAcessoCliente, "SemGluten", "celíaca"));
        Assert.All(todos, r => Assert.Null(r.Ip));
    }

    [Fact]
    public async Task OperacaoRecusada_NaoGeraRegistroDeSucesso()
    {
        var (garcom, idGarcom) = await ClienteGarcom();

        var resposta = await garcom.PostAsJsonAsync("/api/comandas", new { mesaId = 999999, quantidadePessoas = 2 }, Json);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.DoesNotContain(await factory.Auditoria(EventoAuditoria.ComandaAberta), r => r.UsuarioId == idGarcom);
    }
}
