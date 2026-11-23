using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastra.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaFotoDoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "foto",
                table: "usuario",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "foto",
                table: "usuario");
        }
    }
}
