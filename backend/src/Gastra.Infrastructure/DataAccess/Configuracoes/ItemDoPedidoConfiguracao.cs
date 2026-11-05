using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class ItemDoPedidoConfiguracao : IEntityTypeConfiguration<ItemDoPedido>
{
    public void Configure(EntityTypeBuilder<ItemDoPedido> builder)
    {
        // RN02: motivo existe se, e somente se, o item foi cancelado.
        builder.ToTable("item_pedido", tabela => tabela.HasCheckConstraint(
            "CK_item_pedido_motivo_cancelamento",
            "(status = 'Cancelado' AND motivo_cancelamento IS NOT NULL) OR (status <> 'Cancelado' AND motivo_cancelamento IS NULL)"));

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.ComandaId).HasColumnName("comanda_id").IsRequired();
        builder.Property(i => i.ItemDoCardapioId).HasColumnName("item_cardapio_id").IsRequired();
        builder.Property(i => i.Quantidade).HasColumnName("quantidade").IsRequired();
        builder.Property(i => i.PrecoUnitarioNoMomento).HasColumnName("preco_unitario_no_momento")
            .HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(i => i.DataHoraRegistro).HasColumnName("data_hora_registro").IsRequired();
        builder.Property(i => i.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(i => i.MotivoCancelamento).HasColumnName("motivo_cancelamento")
            .HasConversion<string>().HasMaxLength(20);

        // Faturamento por item e recomendação de pratos leem sempre por item do cardápio.
        builder.HasIndex(i => i.ItemDoCardapioId);

        builder.HasOne<ItemDoCardapio>()
            .WithMany()
            .HasForeignKey(i => i.ItemDoCardapioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
