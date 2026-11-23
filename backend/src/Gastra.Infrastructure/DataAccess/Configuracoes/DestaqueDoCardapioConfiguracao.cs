using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class DestaqueDoCardapioConfiguracao : IEntityTypeConfiguration<DestaqueDoCardapio>
{
    public void Configure(EntityTypeBuilder<DestaqueDoCardapio> builder)
    {
        // A meta existe só na meta do mês, e o período nunca termina antes de começar.
        builder.ToTable("destaque_cardapio", tabela => tabela.HasCheckConstraint(
            "CK_destaque_cardapio_periodo_e_meta",
            "data_fim >= data_inicio AND ((tipo = 'MetaDoMes' AND meta > 0) OR (tipo = 'DestaqueDoDia' AND meta IS NULL))"));

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.ItemDoCardapioId).HasColumnName("item_cardapio_id").IsRequired();
        builder.Property(d => d.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(d => d.DataInicio).HasColumnName("data_inicio").IsRequired();
        builder.Property(d => d.DataFim).HasColumnName("data_fim").IsRequired();
        builder.Property(d => d.Meta).HasColumnName("meta");

        // Consulta mais comum: o que vale de hoje em diante.
        builder.HasIndex(d => d.DataFim);

        builder.HasOne<ItemDoCardapio>()
            .WithMany()
            .HasForeignKey(d => d.ItemDoCardapioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
