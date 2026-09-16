using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class MesaConfiguracao : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> builder)
    {
        builder.ToTable("mesa");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.Numero).HasColumnName("numero").HasMaxLength(10).IsRequired();
        builder.Property(m => m.Capacidade).HasColumnName("capacidade").IsRequired();
        builder.Property(m => m.PracaId).HasColumnName("praca_id").IsRequired();

        builder.HasIndex(m => m.Numero).IsUnique();

        // REL01: toda mesa pertence a uma praça. Restrict impede apagar uma praça que ainda tem mesas.
        builder.HasOne<Praca>()
            .WithMany()
            .HasForeignKey(m => m.PracaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
