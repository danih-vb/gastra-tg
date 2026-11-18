using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class AlocacaoConfiguracao : IEntityTypeConfiguration<Alocacao>
{
    public void Configure(EntityTypeBuilder<Alocacao> builder)
    {
        builder.ToTable("alocacao");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.Data).HasColumnName("data").IsRequired();
        builder.Property(a => a.Periodo).HasColumnName("periodo").HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(a => a.GarcomId).HasColumnName("garcom_id").IsRequired();
        builder.Property(a => a.PracaId).HasColumnName("praca_id").IsRequired();
        builder.Property(a => a.Confirmada).HasColumnName("confirmada").IsRequired();

        // Um garçom só pode estar em uma praça no mesmo turno.
        builder.HasIndex(a => new { a.Data, a.Periodo, a.GarcomId }).IsUnique();

        builder.HasOne<Usuario>().WithMany().HasForeignKey(a => a.GarcomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Praca>().WithMany().HasForeignKey(a => a.PracaId).OnDelete(DeleteBehavior.Restrict);
    }
}
