using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Estoque
{
    public class UpdateEstoqueDto
    {
        [Required]
        [Range(1, 1000000, ErrorMessage = "A quantidade de livros em estoque deve estar entre 1 e 1.000.000")]
        public int Quantidade { get; set; }
    }
}