using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastra.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class GaranteConsistenciaDeStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_item_pedido_motivo_cancelamento",
                table: "item_pedido",
                sql: "(status = 'Cancelado' AND motivo_cancelamento IS NOT NULL) OR (status <> 'Cancelado' AND motivo_cancelamento IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_comanda_status_fechamento",
                table: "comanda",
                sql: "(status = 'Aberta' AND data_hora_fechamento IS NULL) OR (status = 'Fechada' AND data_hora_fechamento IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_item_pedido_motivo_cancelamento",
                table: "item_pedido");

            migrationBuilder.DropCheckConstraint(
                name: "CK_comanda_status_fechamento",
                table: "comanda");
        }
    }
}
