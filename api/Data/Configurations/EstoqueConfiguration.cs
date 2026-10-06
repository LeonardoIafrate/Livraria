using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class EstoqueConfiguration : IEntityTypeConfiguration<Estoque>
    {
        public void Configure(EntityTypeBuilder<Estoque> builder)
        {
            builder.ToTable("Estoque", t =>
            {
                t.HasCheckConstraint("CK_EstoqueQuantidade", "[Quantidade] >= 0");
            });

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.LivroId).IsUnique();

            builder.HasOne(e => e.Livro)
                   .WithOne(l => l.Estoque)
                   .HasForeignKey<Estoque>(e => e.LivroId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
        }

    }
}