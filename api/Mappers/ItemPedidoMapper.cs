using api.Dtos.Pedido;
using api.Models;

namespace api.Mappers
{
    public static class ItemPedidoMapper
    {
        public static ItemPedidoDto ToItemPedidoDto(this ItemPedido itemPedido)
        {
            return new ItemPedidoDto
            {
                LivroId = itemPedido.LivroId,
                LivroNome = itemPedido.Livro?.Nome ?? string.Empty,
                Quantidade = itemPedido.Quantidade,
                PrecoUnitario = itemPedido.PrecoUnitario
            };
        }

        public static ItemPedido ToItemPedidoFromCreateDto(this CreateItemPedidoDto dto)
        {
            return new ItemPedido
            {
              LivroId = dto.LivroId,
              Quantidade = dto.Quantidade
            };
        }
    }
}