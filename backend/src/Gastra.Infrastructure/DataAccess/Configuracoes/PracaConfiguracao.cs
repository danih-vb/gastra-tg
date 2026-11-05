using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class PracaConfiguracao : IEntityTypeConfiguration<Praca>
{
    public void Configure(EntityTypeBuilder<Praca> builder)
    {
        builder.ToTable("praca");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
        builder.Property(p => p.QuantidadeGarcons).HasColumnName("quantidade_garcons").IsRequired();

        builder.HasIndex(p => p.Codigo).IsUnique();
    }
}
