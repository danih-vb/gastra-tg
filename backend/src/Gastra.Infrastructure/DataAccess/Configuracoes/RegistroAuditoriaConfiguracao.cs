using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class RegistroAuditoriaConfiguracao : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("registro_auditoria");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.DataHoraUtc).HasColumnName("data_hora_utc").IsRequired();
        builder.Property(r => r.Evento).HasColumnName("evento").HasMaxLength(50).IsRequired();
        builder.Property(r => r.Resultado).HasColumnName("resultado").HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(r => r.UsuarioId).HasColumnName("usuario_id");
        builder.Property(r => r.Papel).HasColumnName("papel").HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Entidade).HasColumnName("entidade").HasMaxLength(50);
        builder.Property(r => r.IdEntidade).HasColumnName("id_entidade");
        builder.Property(r => r.Detalhes).HasColumnName("detalhes").HasMaxLength(1000);
        builder.Property(r => r.Ip).HasColumnName("ip").HasMaxLength(45);
        builder.Property(r => r.IdCorrelacao).HasColumnName("id_correlacao").HasMaxLength(50);

        // Consulta típica da auditoria: o que aconteceu num período, e o que um usuário fez.
        builder.HasIndex(r => r.DataHoraUtc);
        builder.HasIndex(r => new { r.UsuarioId, r.DataHoraUtc });

        // Usuário inativado é soft delete, então o registro sempre continua apontando para ele.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
