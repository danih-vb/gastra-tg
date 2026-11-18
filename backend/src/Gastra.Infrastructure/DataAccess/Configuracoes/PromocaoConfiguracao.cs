using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class PromocaoConfiguracao : IEntityTypeConfiguration<Promocao>
{
    public void Configure(EntityTypeBuilder<Promocao> builder)
    {
        builder.ToTable("promocao", tabela => tabela.HasCheckConstraint(
            "CK_promocao_periodo_e_desconto",
            "data_fim >= data_inicio AND valor_desconto > 0 AND (tipo_desconto <> 'Percentual' OR valor_desconto < 100)"));

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Descricao).HasColumnName("descricao").HasMaxLength(Promocao.TamanhoMaximoDescricao).IsRequired();
        builder.Property(p => p.TipoDesconto).HasColumnName("tipo_desconto").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.ValorDesconto).HasColumnName("valor_desconto").HasPrecision(10, 2).IsRequired();
        builder.Property(p => p.DataInicio).HasColumnName("data_inicio").IsRequired();
        builder.Property(p => p.DataFim).HasColumnName("data_fim").IsRequired();
        builder.Property(p => p.Ativa).HasColumnName("ativa").IsRequired();

        // Consulta mais comum: promoções que valem hoje.
        builder.HasIndex(p => new { p.Ativa, p.DataInicio, p.DataFim });

        // Relação N:N do MER com ItemDoCardapio, em tabela associativa (promocao_item_cardapio).
        builder.Ignore(p => p.ItemCardapioIds);
        builder.OwnsMany<PromocaoItem>("_itens", itens =>
        {
            itens.ToTable("promocao_item_cardapio");
            itens.WithOwner().HasForeignKey("PromocaoId");
            itens.Property<int>("PromocaoId").HasColumnName("promocao_id");
            itens.Property(i => i.ItemCardapioId).HasColumnName("item_cardapio_id");
            itens.HasKey("PromocaoId", nameof(PromocaoItem.ItemCardapioId));

            // Item com promoção (mesmo antiga) não pode ser apagado: o histórico de vendas aponta para ele.
            itens.HasOne<ItemDoCardapio>().WithMany().HasForeignKey(i => i.ItemCardapioId).OnDelete(DeleteBehavior.Restrict);
        });
        builder.Navigation("_itens").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
