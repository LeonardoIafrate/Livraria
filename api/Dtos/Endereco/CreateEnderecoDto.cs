using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Endereco
{
    public class CreateEnderecoDto
    {
        [Required]
        public EnderecoBaseDto Endereco { get; set; } = null!;
    
        public bool Principal { get; set; } = false;
    }
}