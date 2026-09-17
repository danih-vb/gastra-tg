using Gastra.Infrastructure.DataAccess.Repositorios;

namespace Gastra.Api.Tests.BancoDeDados;

/// <summary>As consultas às views usadas pela alocação (RN03), contra o MySQL real.</summary>
public class RepositorioIndicadoresTests : IAsyncLifetime
{
    private readonly BancoMySqlDeTeste _banco = new();

    public async Task InitializeAsync()
    {
        await _banco.InitializeAsync();
        if (string.IsNullOrWhiteSpace(BancoMySqlDeTeste.ConnectionStringDoServidor))
            return;

        await _banco.Executar("""
            INSERT INTO usuario (id, nome, email, senha_hash, papel, ativo, chave_sessao) VALUES
                (1, 'A', 'a@gastra.test', 'h', 'Garcom', 1, UUID()), (2, 'B', 'b@gastra.test', 'h', 'Garcom', 1, UUID());
            INSERT INTO praca (id, codigo, quantidade_garcons) VALUES (1, 'P1', 2), (2, 'P2', 2);
            INSERT INTO mesa (id, numero, capacidade, praca_id) VALUES (1, '1', 4, 1);
            INSERT INTO item_cardapio (id, nome, categoria, preco, descricao, disponivel) VALUES (1, 'Prato', 'PratoPrincipal', 100.00, '', 1);
            -- Garçom 1: 100 em 10/09 e 300 em 20/09 (almoço em Brasília). Garçom 2: 50 em 20/09.
            INSERT INTO comanda (id, mesa_id, garcom_id, data_hora_abertura, data_hora_fechamento, status, quantidade_pessoas, composicao, taxa_servico_removida, composicao_ajustada_manualmente, codigo_acesso_cliente) VALUES
                (1, 1, 1, '2026-09-10 15:00:00', '2026-09-10 16:00:00', 'Fechada', 2, 'Casal', 0, 0, 'c1'),
                (2, 1, 1, '2026-09-20 15:00:00', '2026-09-20 16:00:00', 'Fechada', 2, 'Casal', 0, 0, 'c2'),
                (3, 1, 2, '2026-09-20 15:30:00', '2026-09-20 16:00:00', 'Fechada', 2, 'Casal', 0, 0, 'c3');
            INSERT INTO item_pedido (comanda_id, item_cardapio_id, quantidade, preco_unitario_no_momento, data_hora_registro, status) VALUES
                (1, 1, 1, 100.00, '2026-09-10 15:00:00', 'Entregue'),
                (2, 1, 3, 100.00, '2026-09-20 15:00:00', 'Entregue'),
                (3, 1, 1, 50.00, '2026-09-20 15:30:00', 'Entregue');
            """);
    }

    public Task DisposeAsync() => _banco.DisposeAsync();

    [FactComMySql]
    public async Task Faturamento_medio_por_praca_inclui_praca_sem_movimento()
    {
        await using var contexto = _banco.CriarContexto();

        var medio = await new RepositorioIndicadores(contexto).ObterFaturamentoMedioPorPraca();

        // Praça 1: um turno em 10/09 (100) e um em 20/09 (300 + 50) = média 225.
        Assert.Equal(225m, medio[1]);
        Assert.Equal(0m, medio[2]);
    }

    [FactComMySql]
    public async Task Faturamento_por_turno_do_garcom_e_a_media_dos_turnos_no_intervalo_com_fim_exclusivo()
    {
        await using var contexto = _banco.CriarContexto();
        var repositorio = new RepositorioIndicadores(contexto);

        var ate20 = await repositorio.ObterFaturamentoMedioPorTurnoDoGarcom(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 20));
        var tudo = await repositorio.ObterFaturamentoMedioPorTurnoDoGarcom(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 21));

        Assert.Equal(100m, ate20[1]);
        Assert.False(ate20.ContainsKey(2));
        Assert.Equal(200m, tudo[1]); // média de 100 e 300
        Assert.Equal(50m, tudo[2]);
    }
}
