using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class EnderecoConfiguration : IEntityTypeConfiguration<Endereco>
    {
        public void Configure(EntityTypeBuilder<Endereco> builder)
        {
            builder.ToTable("Endereco");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Cep)
                   .IsRequired()
                   .HasMaxLength(8);

            builder.Property(e => e.Rua)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(e => e.Numero)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(e => e.Complemento)
                   .HasMaxLength(100);

            builder.Property(e => e.Bairro)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Cidade)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Uf)
                   .IsRequired()
                   .HasMaxLength(2);

            builder.HasOne(e => e.Usuario)
                   .WithMany(u => u.Enderecos)
                   .HasForeignKey(e => e.UsuarioId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.UsuarioId)
                   .HasFilter("[Principal] = 1")
                   .IsUnique();
        }
    }
}