using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Pedido
{
    public class AlterarEnderecoPedidoDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Informe um endereço de entrega válido")]
        public int EnderecoId { get; set; }
    }
}