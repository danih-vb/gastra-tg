using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastra.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaLancamentoParaConfirmar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "lancado_por",
                table: "item_pedido",
                type: "varchar(12)",
                maxLength: 12,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lancado_por",
                table: "item_pedido");
        }
    }
}
