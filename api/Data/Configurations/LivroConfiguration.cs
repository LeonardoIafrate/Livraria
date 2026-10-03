using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class LivroConfiguration : IEntityTypeConfiguration<Livro>
    {
        public void Configure(EntityTypeBuilder<Livro> builder)
        {
            builder.ToTable("Livro", t =>
            {
                t.HasCheckConstraint("CK_Livro_NumeroPaginas", "[NumeroPaginas] IS NULL OR [NumeroPaginas] > 0");
                t.HasCheckConstraint("CK_Livro_AnoLancamento", "[AnoLancamento] IS NULL OR [AnoLancamento] BETWEEN 1000 AND 2100");
                t.HasCheckConstraint("CK_Livro_Preco", "[Preco] > 0");
            });

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Nome)
                   .IsRequired()
                   .HasMaxLength(250);

            builder.Property(l => l.Isbn)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.HasIndex(l => l.Isbn).IsUnique();
            
            builder.Property(l => l.Preco)
                   .HasPrecision(10,2);

            builder.Property(l => l.Idioma)
                   .HasMaxLength(50);
            
            builder.HasOne(l => l.Editora)
                   .WithMany(e => e.Livros)
                   .HasForeignKey(l => l.EditoraId)
                   .OnDelete(DeleteBehavior.Restrict);
        
        }
    }
}