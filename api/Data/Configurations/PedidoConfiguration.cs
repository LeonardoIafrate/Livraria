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

            builder.HasOne(p => p.Endereco)
                   .WithMany(e => e.Pedidos)
                   .HasForeignKey(p => p.EnderecoId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}