using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Pedido
{
    public class ItemPedidoDto
    {
        public int LivroDto { get; set; }
        public string LivroNome { get; set; } = string.Empty;
        public int Quantidade { get; set; }

        public decimal PrecoUnitario { get; set; }

        public decimal Subtotal => Quantidade * PrecoUnitario;
    }
}