using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastra.Infrastructure.DataAccess.Migrations
{
    /// <summary>
    /// O trigger de exclusão da auditoria bloqueava qualquer DELETE, o que impediria a eliminação ao fim do
    /// prazo de retenção exigida pela política de log (seção 7) e pela LGPD (art. 16). Agora ele só bloqueia
    /// registros com menos de 6 meses. A alteração continua proibida sempre.
    /// </summary>
    public partial class PermiteEliminarAuditoriaAposRetencao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_registro_auditoria_impede_delete;");

            migrationBuilder.Sql("""
                CREATE TRIGGER trg_registro_auditoria_impede_delete
                BEFORE DELETE ON registro_auditoria
                FOR EACH ROW
                BEGIN
                    IF OLD.data_hora_utc > UTC_TIMESTAMP(6) - INTERVAL 6 MONTH THEN
                        SIGNAL SQLSTATE '45000'
                            SET MESSAGE_TEXT = 'O registro de auditoria só pode ser apagado depois do prazo de retenção de 6 meses.';
                    END IF;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_registro_auditoria_impede_delete;");

            migrationBuilder.Sql("""
                CREATE TRIGGER trg_registro_auditoria_impede_delete
                BEFORE DELETE ON registro_auditoria
                FOR EACH ROW
                SIGNAL SQLSTATE '45000'
                    SET MESSAGE_TEXT = 'O registro de auditoria não pode ser apagado.';
                """);
        }
    }
}
