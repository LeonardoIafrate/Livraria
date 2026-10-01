using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class EditoraConfiguration : IEntityTypeConfiguration<Editora>
    {
        public void Configure(EntityTypeBuilder<Editora> builder)
        {
            builder.ToTable("Editora");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nome)
                   .IsRequired()
                   .HasMaxLength(100);
            
            builder.HasIndex(e => e.Nome).IsUnique();
        }

    }
}