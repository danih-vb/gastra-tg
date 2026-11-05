using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class RestricaoAlimentarConfiguracao : IEntityTypeConfiguration<RestricaoAlimentar>
{
    public void Configure(EntityTypeBuilder<RestricaoAlimentar> builder)
    {
        builder.ToTable("restricao_alimentar");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.ComandaId).HasColumnName("comanda_id").IsRequired();
        builder.Property(r => r.Categoria).HasColumnName("categoria").HasConversion<string>().HasMaxLength(20).IsRequired();

        // Pode conter dado de saúde (LGPD art. 5º, II): apagada no fechamento da comanda.
        builder.Property(r => r.ObservacaoLivre).HasColumnName("observacao_livre").HasMaxLength(200);
    }
}
