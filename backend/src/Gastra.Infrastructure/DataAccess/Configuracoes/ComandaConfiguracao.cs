using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class ComandaConfiguracao : IEntityTypeConfiguration<Comanda>
{
    public void Configure(EntityTypeBuilder<Comanda> builder)
    {
        builder.ToTable("comanda");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.MesaId).HasColumnName("mesa_id").IsRequired();
        builder.Property(c => c.GarcomId).HasColumnName("garcom_id").IsRequired();
        builder.Property(c => c.DataHoraAbertura).HasColumnName("data_hora_abertura").IsRequired();
        builder.Property(c => c.DataHoraFechamento).HasColumnName("data_hora_fechamento");
        builder.Property(c => c.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(c => c.QuantidadePessoas).HasColumnName("quantidade_pessoas").IsRequired();
        builder.Property(c => c.Composicao).HasColumnName("composicao").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.TaxaServicoRemovida).HasColumnName("taxa_servico_removida").IsRequired();
        builder.Property(c => c.CodigoAcessoCliente).HasColumnName("codigo_acesso_cliente").HasMaxLength(32).IsRequired();

        // O cliente consulta a comanda por esse código (UC20): dois iguais dariam acesso à conta errada.
        builder.HasIndex(c => c.CodigoAcessoCliente).IsUnique();

        // Consulta mais comum do salão: o que está aberto em cada mesa.
        builder.HasIndex(c => new { c.MesaId, c.Status });

        builder.HasOne<Mesa>().WithMany().HasForeignKey(c => c.MesaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(c => c.GarcomId).OnDelete(DeleteBehavior.Restrict);

        // Composição: item do pedido e restrição não existem fora da comanda, por isso Cascade.
        builder.HasMany(c => c.Itens)
            .WithOne()
            .HasForeignKey(i => i.ComandaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Restricoes)
            .WithOne()
            .HasForeignKey(r => r.ComandaId)
            .OnDelete(DeleteBehavior.Cascade);

        // As coleções são expostas como somente leitura: o EF acessa os campos por trás delas.
        builder.Metadata.FindNavigation(nameof(Comanda.Itens))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Comanda.Restricoes))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
