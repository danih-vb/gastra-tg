using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastra.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaDestaquesEMetas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "destaque_cardapio",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    item_cardapio_id = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: false),
                    meta = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_destaque_cardapio", x => x.id);
                    table.CheckConstraint("CK_destaque_cardapio_periodo_e_meta", "data_fim >= data_inicio AND ((tipo = 'MetaDoMes' AND meta > 0) OR (tipo = 'DestaqueDoDia' AND meta IS NULL))");
                    table.ForeignKey(
                        name: "FK_destaque_cardapio_item_cardapio_item_cardapio_id",
                        column: x => x.item_cardapio_id,
                        principalTable: "item_cardapio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_destaque_cardapio_data_fim",
                table: "destaque_cardapio",
                column: "data_fim");

            migrationBuilder.CreateIndex(
                name: "IX_destaque_cardapio_item_cardapio_id",
                table: "destaque_cardapio",
                column: "item_cardapio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "destaque_cardapio");
        }
    }
}
