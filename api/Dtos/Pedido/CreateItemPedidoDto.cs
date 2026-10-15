using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Pedido
{
    public class CreateItemPedidoDto
    {

        [Range(1, int.MaxValue, ErrorMessage = "Informe um livro válido.")]
        public int LivroId { get; set; }
    
        [Range(1, 100, ErrorMessage = "A quantidade deve estar entre 1 e 100.")]
        public int Quantidade { get; set; }
    }
}