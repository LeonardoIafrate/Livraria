using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Pedido
{
    public class CreatePedidoDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Informe um endereço de entrega válido.")]
        public int EnderecoId { get; set; }

        [Required(ErrorMessage = "O pedido deve ter ao menos um item.")]
        [MinLength(1, ErrorMessage = "O pedido deve ter ao menos um item.")]
        [MaxLength(50, ErrorMessage = "O pedido não pode ter mais de 50 itens diferentes.")]
        public List<CreateItemPedidoDto> Itens { get; set; } = new();
    }
}