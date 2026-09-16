using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastra.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CriaNucleoComandas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "praca",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    codigo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantidade_garcons = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_praca", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "registro_auditoria",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    data_hora_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    evento = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    resultado = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    usuario_id = table.Column<int>(type: "int", nullable: true),
                    papel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entidade = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_entidade = table.Column<int>(type: "int", nullable: true),
                    detalhes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_correlacao = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registro_auditoria", x => x.id);
                    table.ForeignKey(
                        name: "FK_registro_auditoria_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "alocacao",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    data = table.Column<DateOnly>(type: "date", nullable: false),
                    periodo = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    garcom_id = table.Column<int>(type: "int", nullable: false),
                    praca_id = table.Column<int>(type: "int", nullable: false),
                    confirmada = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alocacao", x => x.id);
                    table.ForeignKey(
                        name: "FK_alocacao_praca_praca_id",
                        column: x => x.praca_id,
                        principalTable: "praca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_alocacao_usuario_garcom_id",
                        column: x => x.garcom_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "mesa",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    numero = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    capacidade = table.Column<int>(type: "int", nullable: false),
                    praca_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mesa", x => x.id);
                    table.ForeignKey(
                        name: "FK_mesa_praca_praca_id",
                        column: x => x.praca_id,
                        principalTable: "praca",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "comanda",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    mesa_id = table.Column<int>(type: "int", nullable: false),
                    garcom_id = table.Column<int>(type: "int", nullable: false),
                    data_hora_abertura = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    data_hora_fechamento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantidade_pessoas = table.Column<int>(type: "int", nullable: false),
                    composicao = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    taxa_servico_removida = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    codigo_acesso_cliente = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comanda", x => x.id);
                    table.ForeignKey(
                        name: "FK_comanda_mesa_mesa_id",
                        column: x => x.mesa_id,
                        principalTable: "mesa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comanda_usuario_garcom_id",
                        column: x => x.garcom_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "item_pedido",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    comanda_id = table.Column<int>(type: "int", nullable: false),
                    item_cardapio_id = table.Column<int>(type: "int", nullable: false),
                    quantidade = table.Column<int>(type: "int", nullable: false),
                    preco_unitario_no_momento = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    data_hora_registro = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    motivo_cancelamento = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_pedido", x => x.id);
                    table.ForeignKey(
                        name: "FK_item_pedido_comanda_comanda_id",
                        column: x => x.comanda_id,
                        principalTable: "comanda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_item_pedido_item_cardapio_item_cardapio_id",
                        column: x => x.item_cardapio_id,
                        principalTable: "item_cardapio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "restricao_alimentar",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    comanda_id = table.Column<int>(type: "int", nullable: false),
                    categoria = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    observacao_livre = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restricao_alimentar", x => x.id);
                    table.ForeignKey(
                        name: "FK_restricao_alimentar_comanda_comanda_id",
                        column: x => x.comanda_id,
                        principalTable: "comanda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_alocacao_data_periodo_garcom_id",
                table: "alocacao",
                columns: new[] { "data", "periodo", "garcom_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_alocacao_garcom_id",
                table: "alocacao",
                column: "garcom_id");

            migrationBuilder.CreateIndex(
                name: "IX_alocacao_praca_id",
                table: "alocacao",
                column: "praca_id");

            migrationBuilder.CreateIndex(
                name: "IX_comanda_codigo_acesso_cliente",
                table: "comanda",
                column: "codigo_acesso_cliente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comanda_garcom_id",
                table: "comanda",
                column: "garcom_id");

            migrationBuilder.CreateIndex(
                name: "IX_comanda_mesa_id_status",
                table: "comanda",
                columns: new[] { "mesa_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_item_pedido_comanda_id",
                table: "item_pedido",
                column: "comanda_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_pedido_item_cardapio_id",
                table: "item_pedido",
                column: "item_cardapio_id");

            migrationBuilder.CreateIndex(
                name: "IX_mesa_numero",
                table: "mesa",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mesa_praca_id",
                table: "mesa",
                column: "praca_id");

            migrationBuilder.CreateIndex(
                name: "IX_praca_codigo",
                table: "praca",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registro_auditoria_data_hora_utc",
                table: "registro_auditoria",
                column: "data_hora_utc");

            migrationBuilder.CreateIndex(
                name: "IX_registro_auditoria_usuario_id_data_hora_utc",
                table: "registro_auditoria",
                columns: new[] { "usuario_id", "data_hora_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_restricao_alimentar_comanda_id",
                table: "restricao_alimentar",
                column: "comanda_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alocacao");

            migrationBuilder.DropTable(
                name: "item_pedido");

            migrationBuilder.DropTable(
                name: "registro_auditoria");

            migrationBuilder.DropTable(
                name: "restricao_alimentar");

            migrationBuilder.DropTable(
                name: "comanda");

            migrationBuilder.DropTable(
                name: "mesa");

            migrationBuilder.DropTable(
                name: "praca");
        }
    }
}
