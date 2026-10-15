namespace api.Models
{
    public class ItemPedido
    {
        public int Id { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }

        public Livro Livro { get; set; } = null!;
        public int LivroId { get; set; }

        public Pedido Pedido { get; set; } = null!;
        public int PedidoId { get; set; }
    }
}