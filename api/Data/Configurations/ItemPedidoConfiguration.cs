using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
    {
        public void Configure(EntityTypeBuilder<ItemPedido> builder)
        {
            builder.ToTable("ItemPedido", t =>
            {
                t.HasCheckConstraint("C_ItemPedido_Quantidade", "[Quantidade] > 0");
                t.HasCheckConstraint("C_ItemPedido_PrecoUnitario", "[PrecoUnitario] >= 0");
            });

            builder.HasKey(i => i.Id);

            builder.HasIndex(i => new { i.PedidoId, i.LivroId}).IsUnique();

            builder.Property(i => i.PrecoUnitario)
                   .HasPrecision(10,2);

            builder.HasOne(i => i.Pedido)
                   .WithMany(p => p.Itens)
                   .HasForeignKey(i => i.PedidoId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Livro)
                   .WithMany()
                   .HasForeignKey(i => i.LivroId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}