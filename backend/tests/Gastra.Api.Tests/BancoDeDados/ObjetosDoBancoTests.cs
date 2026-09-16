using MySqlConnector;

namespace Gastra.Api.Tests.BancoDeDados;

/// <summary>
/// Views analíticas e triggers de auditoria da migration CriaViewsETriggersAnaliticos.
/// Os dados são inseridos direto em SQL para controlar as datas (a aplicação sempre grava "agora").
/// </summary>
public class ObjetosDoBancoTests : IAsyncLifetime
{
    private readonly BancoMySqlDeTeste _banco = new();

    // Ids fixos: cada teste roda num schema novo.
    private const int GarcomA = 1, GarcomB = 2;
    private const int PracaCheia = 1, PracaVazia = 2;
    private const int Mesa1 = 1, Mesa2 = 2;
    private const int Moqueca = 1, Suco = 2;

    public async Task InitializeAsync()
    {
        await _banco.InitializeAsync();

        if (string.IsNullOrWhiteSpace(BancoMySqlDeTeste.ConnectionStringDoServidor))
            return;

        await _banco.Executar($"""
            INSERT INTO usuario (id, nome, email, senha_hash, papel, ativo, chave_sessao) VALUES
                ({GarcomA}, 'Garçom A', 'a@gastra.test', 'hash', 'Garcom', 1, UUID()),
                ({GarcomB}, 'Garçom B', 'b@gastra.test', 'hash', 'Garcom', 1, UUID());
            INSERT INTO praca (id, codigo, quantidade_garcons) VALUES ({PracaCheia}, 'P1', 2), ({PracaVazia}, 'P2', 1);
            INSERT INTO mesa (id, numero, capacidade, praca_id) VALUES ({Mesa1}, '1', 4, {PracaCheia}), ({Mesa2}, '2', 4, {PracaCheia});
            INSERT INTO item_cardapio (id, nome, categoria, preco, descricao, disponivel) VALUES
                ({Moqueca}, 'Moqueca', 'PratoPrincipal', 100.00, '', 1),
                ({Suco}, 'Suco', 'Bebida', 10.00, '', 1);
            """);

        // Comanda 1: almoço de 15/09 (12h em Brasília), mesa 1, garçom A.
        // Moqueca 1x + suco em duas linhas (2x e 1x) + um suco cancelado = 130.
        await InserirComanda(1, Mesa1, GarcomA, "2026-09-15 15:00:00", "Fechada", pessoas: 2,
            (Moqueca, 1, "Entregue"), (Suco, 2, "Entregue"), (Suco, 1, "Entregue"), (Suco, 5, "Cancelado"));

        // Comanda 2: aberta às 23h30 de 14/09 em Brasília, que já é 15/09 em UTC. Mesa 2, garçom A = 200.
        await InserirComanda(2, Mesa2, GarcomA, "2026-09-15 02:30:00", "Fechada", pessoas: 3, (Moqueca, 2, "Entregue"));

        // Comanda 3: jantar de 15/09 (20h em Brasília), mesa 1, garçom B = 300.
        await InserirComanda(3, Mesa1, GarcomB, "2026-09-15 23:00:00", "Fechada", pessoas: 4, (Moqueca, 3, "Entregue"));

        // Comanda 4: ainda aberta, não entra em nenhuma view.
        await InserirComanda(4, Mesa2, GarcomB, "2026-09-15 16:00:00", "Aberta", pessoas: 1, (Moqueca, 9, "Pendente"));
    }

    public Task DisposeAsync() => _banco.DisposeAsync();

    [FactComMySql]
    public async Task Faturamento_da_comanda_ignora_itens_cancelados_e_comandas_abertas()
    {
        var linhas = await _banco.Consultar("SELECT comanda_id, faturamento FROM vw_comanda_faturamento ORDER BY comanda_id");

        Assert.Equal([1L, 2L, 3L], linhas.Select(l => Convert.ToInt64(l["comanda_id"])));
        Assert.Equal([130m, 200m, 300m], linhas.Select(l => (decimal)l["faturamento"]!));
    }

    [FactComMySql]
    public async Task Data_turno_e_hora_seguem_o_horario_de_brasilia()
    {
        var linhas = await _banco.Consultar(
            "SELECT comanda_id, data, periodo, hora, dia_semana FROM vw_comanda_faturamento ORDER BY comanda_id");

        Assert.Equal((new DateTime(2026, 9, 15), "Almoco", 12), Turno(linhas[0]));
        // 02:30 UTC de 15/09 é 23:30 de 14/09 em Brasília: jantar do dia anterior.
        Assert.Equal((new DateTime(2026, 9, 14), "Jantar", 23), Turno(linhas[1]));
        Assert.Equal((new DateTime(2026, 9, 15), "Jantar", 20), Turno(linhas[2]));
        // DAYOFWEEK: 1 = domingo. 15/09/2026 é terça-feira.
        Assert.Equal(3, Convert.ToInt32(linhas[0]["dia_semana"]));
    }

    [FactComMySql]
    public async Task Faturamento_medio_da_praca_e_calculado_por_turno_e_inclui_praca_sem_movimento()
    {
        var linhas = await _banco.Consultar(
            "SELECT praca_id, turnos_com_movimento, faturamento_total, faturamento_medio_por_turno FROM vw_faturamento_medio_praca ORDER BY praca_id");

        // Praça cheia: três turnos (almoço 15/09 = 130, jantar 14/09 = 200, jantar 15/09 = 300).
        Assert.Equal(3L, Convert.ToInt64(linhas[0]["turnos_com_movimento"]));
        Assert.Equal(630m, (decimal)linhas[0]["faturamento_total"]!);
        Assert.Equal(210m, (decimal)linhas[0]["faturamento_medio_por_turno"]!);

        Assert.Equal(PracaVazia, Convert.ToInt32(linhas[1]["praca_id"]));
        Assert.Equal(0L, Convert.ToInt64(linhas[1]["turnos_com_movimento"]));
        Assert.Equal(0m, (decimal)linhas[1]["faturamento_medio_por_turno"]!);
    }

    [FactComMySql]
    public async Task Faturamento_da_praca_por_turno_soma_as_comandas_do_turno()
    {
        await InserirComanda(5, Mesa2, GarcomB, "2026-09-15 16:30:00", "Fechada", pessoas: 2, (Suco, 7, "Entregue"));

        var almoco = (await _banco.Consultar(
            $"SELECT comandas, pessoas_atendidas, faturamento FROM vw_faturamento_praca_turno WHERE praca_id = {PracaCheia} AND data = '2026-09-15' AND periodo = 'Almoco'")).Single();

        Assert.Equal(2L, Convert.ToInt64(almoco["comandas"]));
        Assert.Equal(4m, Convert.ToDecimal(almoco["pessoas_atendidas"]));
        Assert.Equal(200m, (decimal)almoco["faturamento"]!);
    }

    [FactComMySql]
    public async Task Desempenho_do_garcom_conta_mesas_distintas_por_turno()
    {
        await InserirComanda(6, Mesa1, GarcomB, "2026-09-15 23:40:00", "Fechada", pessoas: 2, (Suco, 1, "Entregue"));

        var jantarB = (await _banco.Consultar(
            $"SELECT comandas_atendidas, mesas_atendidas, faturamento FROM vw_desempenho_garcom_turno WHERE garcom_id = {GarcomB} AND periodo = 'Jantar'")).Single();

        Assert.Equal(2L, Convert.ToInt64(jantarB["comandas_atendidas"]));
        Assert.Equal(1L, Convert.ToInt64(jantarB["mesas_atendidas"]));
        Assert.Equal(310m, (decimal)jantarB["faturamento"]!);
    }

    [FactComMySql]
    public async Task Itens_por_comanda_juntam_linhas_do_mesmo_item_e_descartam_cancelados()
    {
        var linhas = await _banco.Consultar(
            "SELECT item_cardapio_id, quantidade FROM vw_itens_por_comanda WHERE comanda_id = 1 ORDER BY item_cardapio_id");

        Assert.Equal([Moqueca, Suco], linhas.Select(l => Convert.ToInt32(l["item_cardapio_id"])));
        Assert.Equal([1m, 3m], linhas.Select(l => Convert.ToDecimal(l["quantidade"])));
    }

    [FactComMySql]
    public async Task Faturamento_por_item_traz_a_categoria_do_cardapio()
    {
        var sucoNoAlmoco = (await _banco.Consultar(
            $"SELECT categoria, quantidade, faturamento FROM vw_faturamento_item_cardapio WHERE item_cardapio_id = {Suco} AND periodo = 'Almoco'")).Single();

        Assert.Equal("Bebida", sucoNoAlmoco["categoria"]);
        Assert.Equal(3m, Convert.ToDecimal(sucoNoAlmoco["quantidade"]));
        Assert.Equal(30m, (decimal)sucoNoAlmoco["faturamento"]!);
    }

    [FactComMySql]
    public async Task Views_nao_expoem_dado_pessoal()
    {
        var colunas = await _banco.Consultar("""
            SELECT DISTINCT column_name AS coluna FROM information_schema.columns
            WHERE table_schema = DATABASE() AND table_name LIKE 'vw\_%'
            """);

        var nomes = colunas.Select(c => c["coluna"]!.ToString()).ToList();
        Assert.Contains("faturamento", nomes);
        Assert.DoesNotContain(nomes, n => n is "nome" or "email" or "observacao_livre" or "codigo_acesso_cliente");
    }

    [FactComMySql]
    public async Task Registro_de_auditoria_aceita_insercao_mas_nao_alteracao_nem_exclusao()
    {
        await _banco.Executar("""
            INSERT INTO registro_auditoria (data_hora_utc, evento, resultado) VALUES (UTC_TIMESTAMP(6), 'Login', 'Sucesso');
            """);

        var alteracao = await Assert.ThrowsAsync<MySqlException>(() =>
            _banco.Executar("UPDATE registro_auditoria SET resultado = 'Falha'"));
        var exclusao = await Assert.ThrowsAsync<MySqlException>(() =>
            _banco.Executar("DELETE FROM registro_auditoria"));

        Assert.Equal("45000", alteracao.SqlState);
        Assert.Contains("não pode ser alterado", alteracao.Message);
        Assert.Contains("não pode ser apagado", exclusao.Message);
        Assert.Single(await _banco.Consultar("SELECT id FROM registro_auditoria WHERE resultado = 'Sucesso'"));
    }

    private Task InserirComanda(int id, int mesaId, int garcomId, string aberturaUtc, string status, int pessoas,
        params (int ItemId, int Quantidade, string Status)[] itens)
    {
        var fechamento = status == "Fechada" ? $"DATE_ADD('{aberturaUtc}', INTERVAL 1 HOUR)" : "NULL";
        var valores = string.Join(",\n", itens.Select(i =>
            $"({id}, {i.ItemId}, {i.Quantidade}, (SELECT preco FROM item_cardapio WHERE id = {i.ItemId}), '{aberturaUtc}', '{i.Status}')"));

        return _banco.Executar($"""
            INSERT INTO comanda (id, mesa_id, garcom_id, data_hora_abertura, data_hora_fechamento, status, quantidade_pessoas,
                                 composicao, taxa_servico_removida, composicao_ajustada_manualmente, codigo_acesso_cliente)
            VALUES ({id}, {mesaId}, {garcomId}, '{aberturaUtc}', {fechamento}, '{status}', {pessoas}, 'Casal', 0, 0, 'codigo-{id}');
            INSERT INTO item_pedido (comanda_id, item_cardapio_id, quantidade, preco_unitario_no_momento, data_hora_registro, status)
            VALUES {valores};
            """);
    }

    private static (DateTime, string, int) Turno(Dictionary<string, object?> linha) =>
        ((DateTime)linha["data"]!, (string)linha["periodo"]!, Convert.ToInt32(linha["hora"]));
}
