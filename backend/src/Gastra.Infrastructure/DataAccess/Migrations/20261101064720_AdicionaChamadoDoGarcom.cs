using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastra.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaChamadoDoGarcom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "atendimento_chamado_em",
                table: "comanda",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "atendimento_chamado_em",
                table: "comanda");
        }
    }
}
