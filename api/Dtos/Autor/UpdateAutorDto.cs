using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Autor
{
    public class UpdateAutorDto
    {
        [Required]
        [MaxLength(100, ErrorMessage = "O nome do autor deve conter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(3000, ErrorMessage = "A biografia do autor deve conter no máximo 3000 caracteres")]
        public string? Biografia { get; set; }

        [MaxLength(100, ErrorMessage = "A nacionalidade do autor deve conter no máximo 100 caracteres")]
        public string? Nacionalidade { get; set; }
    }
}