namespace api.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public DateTime DataPedido { get; set; } = DateTime.UtcNow;
        public StatusPedido Status { get; set; } = StatusPedido.Pendente;
        public decimal ValorTotal { get; set; }

        public Usuario Usuario { get; set; } = null!; 
        public int UsuarioId { get; set; }

        public Endereco Endereco { get; set; } = null!;
        public int EnderecoId { get; set; }        

        public List<ItemPedido> Itens { get; set; } = new();
    }
}