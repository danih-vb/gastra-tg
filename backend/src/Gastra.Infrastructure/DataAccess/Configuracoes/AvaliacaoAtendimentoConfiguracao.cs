using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class AvaliacaoAtendimentoConfiguracao : IEntityTypeConfiguration<AvaliacaoAtendimento>
{
    public void Configure(EntityTypeBuilder<AvaliacaoAtendimento> builder)
    {
        // A nota vem do domínio validada; o CHECK impede que qualquer outro caminho até o banco grave
        // uma nota fora da escala, inclusive uma carga manual.
        builder.ToTable("avaliacao_atendimento", tabela => tabela.HasCheckConstraint(
            "CK_avaliacao_nota",
            $"nota BETWEEN {AvaliacaoAtendimento.NotaMinima} AND {AvaliacaoAtendimento.NotaMaxima}"));

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.ComandaId).HasColumnName("comanda_id").IsRequired();
        builder.Property(a => a.Nota).HasColumnName("nota").IsRequired();
        builder.Property(a => a.Comentario)
            .HasColumnName("comentario")
            .HasMaxLength(AvaliacaoAtendimento.TamanhoMaximoDoComentario);
        builder.Property(a => a.DataHoraEnvio).HasColumnName("data_hora_envio").IsRequired();

        // Uma avaliação por comanda (RN08): o índice único é o que garante isso mesmo com dois envios
        // simultâneos do mesmo código de acesso.
        builder.HasIndex(a => a.ComandaId).IsUnique();
    }
}
