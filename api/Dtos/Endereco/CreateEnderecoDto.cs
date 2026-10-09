using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Endereco
{
    public class CreateEnderecoDto : EnderecoBaseDto
    {
        public bool Principal { get; set; } = false;
    }
}