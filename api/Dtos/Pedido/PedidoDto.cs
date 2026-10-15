using api.Models;

namespace api.Dtos.Pedido
{
    public class PedidoDto
    {
        public int Id { get; set; }
        public DateTime DataPedido { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }

        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;

        public int EnderecoId { get; set; }
        public string Endereco { get; set; } = string.Empty;
        
        public List<ItemPedido> Itens { get; set; } = new();
    }
}