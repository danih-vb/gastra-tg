using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastra.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaMencaoHonrosa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mencao_honrosa",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    garcom_id = table.Column<int>(type: "int", nullable: false),
                    titulo = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descricao = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cor = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mes = table.Column<DateOnly>(type: "date", nullable: false),
                    concedida_por_id = table.Column<int>(type: "int", nullable: false),
                    data_hora_concessao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mencao_honrosa", x => x.id);
                    table.CheckConstraint("CK_mencao_honrosa_primeiro_dia_do_mes", "DAY(mes) = 1");
                    table.ForeignKey(
                        name: "FK_mencao_honrosa_usuario_concedida_por_id",
                        column: x => x.concedida_por_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mencao_honrosa_usuario_garcom_id",
                        column: x => x.garcom_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_mencao_honrosa_concedida_por_id",
                table: "mencao_honrosa",
                column: "concedida_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_mencao_honrosa_garcom_id_mes",
                table: "mencao_honrosa",
                columns: new[] { "garcom_id", "mes" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mencao_honrosa");
        }
    }
}
