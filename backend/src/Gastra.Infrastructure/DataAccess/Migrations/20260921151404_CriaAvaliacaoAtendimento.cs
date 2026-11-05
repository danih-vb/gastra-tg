using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastra.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CriaAvaliacaoAtendimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "avaliacao_atendimento",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    comanda_id = table.Column<int>(type: "int", nullable: false),
                    nota = table.Column<int>(type: "int", nullable: false),
                    comentario = table.Column<string>(type: "varchar(280)", maxLength: 280, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_hora_envio = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avaliacao_atendimento", x => x.id);
                    table.CheckConstraint("CK_avaliacao_nota", "nota BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_avaliacao_atendimento_comanda_comanda_id",
                        column: x => x.comanda_id,
                        principalTable: "comanda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_avaliacao_atendimento_comanda_id",
                table: "avaliacao_atendimento",
                column: "comanda_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avaliacao_atendimento");
        }
    }
}
