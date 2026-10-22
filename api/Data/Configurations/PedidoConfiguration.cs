using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data.Configurations
{
    public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
    {
        public void Configure(EntityTypeBuilder<Pedido> builder)
        {
            builder.ToTable("Pedido", t=>
            {
                t.HasCheckConstraint("CK_Pedido_ValorTotal", "[ValorTotal] > 0");
                t.HasCheckConstraint("CK_Pedido_Status", "[Status] IN ('Pendente', 'Pago', 'Enviado', 'Entregue', 'Cancelado')");
            });

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(p => p.ValorTotal)
                   .HasPrecision(10, 2);

            builder.HasOne(p => p.Usuario)
                   .WithMany(u => u.Pedidos)
                   .HasForeignKey(p => p.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.OwnsOne(p => p.EnderecoEntrega, e =>
            {
                e.Property(x => x.Cep).IsRequired().HasMaxLength(8);
                e.Property(x => x.Rua).IsRequired().HasMaxLength(150);
                e.Property(x => x.Numero).IsRequired().HasMaxLength(10);
                e.Property(x => x.Complemento).HasMaxLength(100);
                e.Property(x => x.Bairro).IsRequired().HasMaxLength(100);
                e.Property(x => x.Cidade).IsRequired().HasMaxLength(100);
                e.Property(x => x.Uf).IsRequired().HasMaxLength(2);
            });

            builder.Navigation(p => p.EnderecoEntrega).IsRequired();
        }
    }
}