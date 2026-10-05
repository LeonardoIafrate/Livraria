using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class AutorConfiguration : IEntityTypeConfiguration<Autor>
    {
        public void Configure(EntityTypeBuilder<Autor> builder)
        {
            builder.ToTable("Autor");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Nome)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(a => a.Biografia)
                   .HasMaxLength(3000);

            builder.Property(a => a.Nacionalidade)
                   .HasMaxLength(100);
                
        }
    }
}