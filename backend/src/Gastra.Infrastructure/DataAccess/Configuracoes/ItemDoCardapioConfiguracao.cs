using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class ItemDoCardapioConfiguracao : IEntityTypeConfiguration<ItemDoCardapio>
{
    public void Configure(EntityTypeBuilder<ItemDoCardapio> builder)
    {
        builder.ToTable("item_cardapio");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
        builder.Property(i => i.Categoria).HasColumnName("categoria").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(i => i.Preco).HasColumnName("preco").HasPrecision(10, 2).IsRequired();
        builder.Property(i => i.Descricao).HasColumnName("descricao").HasMaxLength(500).IsRequired();
        builder.Property(i => i.Disponivel).HasColumnName("disponivel").IsRequired();
        builder.Property(i => i.Imagem).HasColumnName("imagem").HasMaxLength(255);

        builder.HasIndex(i => i.Nome);

        // Atributo multivalorado em tabela própria (1FN): uma linha por flag de cada item.
        builder.Ignore(i => i.FlagsDieteticas);
        builder.OwnsMany<ItemCardapioFlag>("_flags", flags =>
        {
            flags.ToTable("item_cardapio_flag");
            flags.WithOwner().HasForeignKey("ItemCardapioId");
            flags.Property<int>("ItemCardapioId").HasColumnName("item_cardapio_id");
            flags.Property(f => f.Flag).HasColumnName("flag").HasConversion<string>().HasMaxLength(20);
            flags.HasKey("ItemCardapioId", nameof(ItemCardapioFlag.Flag));
        });
        builder.Navigation("_flags").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
