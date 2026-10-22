using api.Dtos.Pedido;
using api.Models;

namespace api.Mappers
{
    public static class PedidoMapper
    {
        public static PedidoDto ToPedidoDto(this Pedido pedido)
        {
            return new PedidoDto
            {
                Id = pedido.Id,
                DataPedido = pedido.DataPedido,
                Status = pedido.Status.ToString(),
                ValorTotal = pedido.ValorTotal,

                UsuarioId = pedido.UsuarioId,
                UsuarioNome = pedido.Usuario?.Nome ?? string.Empty,

                Endereco = pedido.EnderecoEntrega?.ToEnderecoFormatado() ?? string.Empty,

                Itens = pedido.Itens.Select(i => i.ToItemPedidoDto()).ToList()
            };
        }
    }
}