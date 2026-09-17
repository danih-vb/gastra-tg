using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastra.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CriaPromocoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "promocao",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    descricao = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipo_desconto = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    valor_desconto = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: false),
                    ativa = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promocao", x => x.id);
                    table.CheckConstraint("CK_promocao_periodo_e_desconto", "data_fim >= data_inicio AND valor_desconto > 0 AND (tipo_desconto <> 'Percentual' OR valor_desconto < 100)");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "promocao_item_cardapio",
                columns: table => new
                {
                    item_cardapio_id = table.Column<int>(type: "int", nullable: false),
                    promocao_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promocao_item_cardapio", x => new { x.promocao_id, x.item_cardapio_id });
                    table.ForeignKey(
                        name: "FK_promocao_item_cardapio_item_cardapio_item_cardapio_id",
                        column: x => x.item_cardapio_id,
                        principalTable: "item_cardapio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_promocao_item_cardapio_promocao_promocao_id",
                        column: x => x.promocao_id,
                        principalTable: "promocao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_promocao_ativa_data_inicio_data_fim",
                table: "promocao",
                columns: new[] { "ativa", "data_inicio", "data_fim" });

            migrationBuilder.CreateIndex(
                name: "IX_promocao_item_cardapio_item_cardapio_id",
                table: "promocao_item_cardapio",
                column: "item_cardapio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "promocao_item_cardapio");

            migrationBuilder.DropTable(
                name: "promocao");
        }
    }
}
