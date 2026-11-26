using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class MencaoHonrosaConfiguracao : IEntityTypeConfiguration<MencaoHonrosa>
{
    public void Configure(EntityTypeBuilder<MencaoHonrosa> builder)
    {
        // O mês é sempre o primeiro dia, para a menção cair num mês só.
        builder.ToTable("mencao_honrosa", tabela => tabela.HasCheckConstraint(
            "CK_mencao_honrosa_primeiro_dia_do_mes", "DAY(mes) = 1"));

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.GarcomId).HasColumnName("garcom_id").IsRequired();
        builder.Property(m => m.Titulo).HasColumnName("titulo").HasMaxLength(MencaoHonrosa.TamanhoMaximoDoTitulo).IsRequired();
        builder.Property(m => m.Descricao).HasColumnName("descricao").HasMaxLength(MencaoHonrosa.TamanhoMaximoDaDescricao);
        builder.Property(m => m.Icone).HasColumnName("icone").HasMaxLength(20).IsRequired();
        builder.Property(m => m.Cor).HasColumnName("cor").HasMaxLength(20).IsRequired();
        builder.Property(m => m.Mes).HasColumnName("mes").IsRequired();
        builder.Property(m => m.ConcedidaPorId).HasColumnName("concedida_por_id").IsRequired();
        builder.Property(m => m.DataHoraConcessao).HasColumnName("data_hora_concessao").IsRequired();

        builder.HasIndex(m => new { m.GarcomId, m.Mes });

        builder.HasOne<Usuario>().WithMany().HasForeignKey(m => m.GarcomId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(m => m.ConcedidaPorId).OnDelete(DeleteBehavior.Restrict);
    }
}
