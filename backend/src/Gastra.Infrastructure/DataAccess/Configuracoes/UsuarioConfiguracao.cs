using Gastra.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gastra.Infrastructure.DataAccess.Configuracoes;

public class UsuarioConfiguracao : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
        builder.Property(u => u.SenhaHash).HasColumnName("senha_hash").HasMaxLength(100).IsRequired();
        builder.Property(u => u.Papel).HasColumnName("papel").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(u => u.Ativo).HasColumnName("ativo").IsRequired();
        builder.Property(u => u.SegredoTotp).HasColumnName("segredo_totp").HasMaxLength(500);
        builder.Property(u => u.ChaveSessao).HasColumnName("chave_sessao").IsRequired();

        // E-mail é chave alternativa (MER, ENT03).
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
