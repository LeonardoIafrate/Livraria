using api.Models;

namespace api.Interfaces
{
    public interface IPedidoRepository
    {
        Task<List<Pedido>> GetAllAsync();
        Task<Pedido?> GetByIdAsync(int id);
        Task<List<Pedido>> GetByIdUsuarioAsync(int usuarioId);
        Task<List<Pedido>> GetByNomeUsuarioAsync(string nomeUsuario);
        Task<Pedido> CreatePedidoAsync(Pedido pedidoModel);
        Task<Pedido?> UpdateEnderecoPedidoAsync(int id, EnderecoEntrega enderecoEntrega);
        Task<Pedido?> AtualizarStatusAsync(int id, StatusPedido status);
        Task<Pedido?> CancelarPedidoAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}