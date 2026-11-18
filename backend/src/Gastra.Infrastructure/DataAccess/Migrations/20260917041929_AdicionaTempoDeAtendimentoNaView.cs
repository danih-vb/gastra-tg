using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastra.Infrastructure.DataAccess.Migrations
{
    /// <summary>
    /// KPI "tempo médio de atendimento" (abertura → fechamento da comanda): acrescenta a coluna
    /// <c>minutos_atendimento</c> no fim da view base. As demais views listam as colunas pelo nome, então não mudam.
    /// </summary>
    public partial class AdicionaTempoDeAtendimentoNaView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(DefinicaoDaView(comMinutos: true));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(DefinicaoDaView(comMinutos: false));
        }

        private static string DefinicaoDaView(bool comMinutos) => $"""
            CREATE OR REPLACE VIEW vw_comanda_faturamento AS
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
                  WHERE i.comanda_id = c.id AND i.status <> 'Cancelado') AS faturamento{(comMinutos ? ",\n    TIMESTAMPDIFF(MINUTE, c.data_hora_abertura, c.data_hora_fechamento) AS minutos_atendimento" : "")}
            FROM comanda c
            JOIN mesa m ON m.id = c.mesa_id
            WHERE c.status = 'Fechada';
            """;
    }
}
