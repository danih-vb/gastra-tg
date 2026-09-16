using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastra.Infrastructure.DataAccess.Migrations
{
    /// <summary>
    /// Views de leitura para o BI e para a camada analítica (decisão D10) e triggers que tornam a
    /// auditoria somente de inserção. Não há stored procedures: as regras ficam no domínio (D6).
    /// A justificativa de cada objeto está em docs/modelagem/GASTRA_Objetos_Banco.md.
    /// </summary>
    public partial class CriaViewsETriggersAnaliticos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Base de todas as outras: uma linha por comanda fechada, já com o faturamento calculado.
            // As datas são gravadas em UTC; o turno e a hora do dia usam o horário de Brasília
            // (UTC-3 fixo, sem horário de verão desde 2019). Abertura antes das 17h conta como almoço.
            migrationBuilder.Sql("""
                CREATE VIEW vw_comanda_faturamento AS
                SELECT
                    c.id AS comanda_id,
                    c.garcom_id,
                    c.mesa_id,
                    m.praca_id,
                    DATE(CONVERT_TZ(c.data_hora_abertura, '+00:00', '-03:00')) AS data,
                    CASE WHEN HOUR(CONVERT_TZ(c.data_hora_abertura, '+00:00', '-03:00')) < 17
                         THEN 'Almoco' ELSE 'Jantar' END AS periodo,
                    DAYOFWEEK(CONVERT_TZ(c.data_hora_abertura, '+00:00', '-03:00')) AS dia_semana,
                    HOUR(CONVERT_TZ(c.data_hora_abertura, '+00:00', '-03:00')) AS hora,
                    c.quantidade_pessoas,
                    c.composicao,
                    (SELECT COALESCE(SUM(i.quantidade * i.preco_unitario_no_momento), 0)
                       FROM item_pedido i
                      WHERE i.comanda_id = c.id AND i.status <> 'Cancelado') AS faturamento
                FROM comanda c
                JOIN mesa m ON m.id = c.mesa_id
                WHERE c.status = 'Fechada';
                """);

            migrationBuilder.Sql("""
                CREATE VIEW vw_faturamento_praca_turno AS
                SELECT
                    praca_id,
                    data,
                    periodo,
                    COUNT(*) AS comandas,
                    SUM(quantidade_pessoas) AS pessoas_atendidas,
                    SUM(faturamento) AS faturamento
                FROM vw_comanda_faturamento
                GROUP BY praca_id, data, periodo;
                """);

            // Atributo derivado do MER: calculado a cada leitura, nunca armazenado.
            migrationBuilder.Sql("""
                CREATE VIEW vw_faturamento_medio_praca AS
                SELECT
                    p.id AS praca_id,
                    COUNT(t.praca_id) AS turnos_com_movimento,
                    COALESCE(SUM(t.faturamento), 0) AS faturamento_total,
                    COALESCE(ROUND(AVG(t.faturamento), 2), 0) AS faturamento_medio_por_turno
                FROM praca p
                LEFT JOIN vw_faturamento_praca_turno t ON t.praca_id = p.id
                GROUP BY p.id;
                """);

            // Bases do índice de desempenho (RF11). A fórmula e os pesos ficam na aplicação.
            migrationBuilder.Sql("""
                CREATE VIEW vw_desempenho_garcom_turno AS
                SELECT
                    garcom_id,
                    data,
                    periodo,
                    COUNT(*) AS comandas_atendidas,
                    COUNT(DISTINCT mesa_id) AS mesas_atendidas,
                    SUM(quantidade_pessoas) AS pessoas_atendidas,
                    SUM(faturamento) AS faturamento
                FROM vw_comanda_faturamento
                GROUP BY garcom_id, data, periodo;
                """);

            migrationBuilder.Sql("""
                CREATE VIEW vw_faturamento_item_cardapio AS
                SELECT
                    i.item_cardapio_id,
                    ic.categoria,
                    cf.data,
                    cf.periodo,
                    SUM(i.quantidade) AS quantidade,
                    SUM(i.quantidade * i.preco_unitario_no_momento) AS faturamento
                FROM item_pedido i
                JOIN vw_comanda_faturamento cf ON cf.comanda_id = i.comanda_id
                JOIN item_cardapio ic ON ic.id = i.item_cardapio_id
                WHERE i.status <> 'Cancelado'
                GROUP BY i.item_cardapio_id, ic.categoria, cf.data, cf.periodo;
                """);

            // Transações para as regras de associação (RF09): só ids, sem dado de cliente (RN05).
            migrationBuilder.Sql("""
                CREATE VIEW vw_itens_por_comanda AS
                SELECT
                    i.comanda_id,
                    i.item_cardapio_id,
                    cf.data,
                    SUM(i.quantidade) AS quantidade
                FROM item_pedido i
                JOIN vw_comanda_faturamento cf ON cf.comanda_id = i.comanda_id
                WHERE i.status <> 'Cancelado'
                GROUP BY i.comanda_id, i.item_cardapio_id, cf.data;
                """);

            // A auditoria é gravada pela aplicação; o banco só garante que ninguém a altere depois.
            migrationBuilder.Sql("""
                CREATE TRIGGER trg_registro_auditoria_impede_update
                BEFORE UPDATE ON registro_auditoria
                FOR EACH ROW
                SIGNAL SQLSTATE '45000'
                    SET MESSAGE_TEXT = 'O registro de auditoria não pode ser alterado.';
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER trg_registro_auditoria_impede_delete
                BEFORE DELETE ON registro_auditoria
                FOR EACH ROW
                SIGNAL SQLSTATE '45000'
                    SET MESSAGE_TEXT = 'O registro de auditoria não pode ser apagado.';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_registro_auditoria_impede_delete;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_registro_auditoria_impede_update;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_itens_por_comanda;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_faturamento_item_cardapio;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_desempenho_garcom_turno;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_faturamento_medio_praca;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_faturamento_praca_turno;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_comanda_faturamento;");
        }
    }
}
