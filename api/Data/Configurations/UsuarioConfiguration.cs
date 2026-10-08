using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuario");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Nome)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.Cpf)
                   .IsRequired()
                   .HasMaxLength(11);

            builder.Property(u => u.SenhaHash)
                   .IsRequired()
                   .HasMaxLength(500);
            
            builder.Property(u => u.Perfil)
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(u => u.DataNascimento)
                   .IsRequired()
                   .HasColumnType("date");

            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.Cpf).IsUnique();
        }
    }
}