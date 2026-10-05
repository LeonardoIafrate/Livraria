using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Categoria
{
    public class UpdateCategoriaDto
    {
        [Required]
        [MaxLength(30, ErrorMessage = "O Gênero não pode exceder 30 caracteres.")]
        public string Genero { get; set; } = string.Empty;
    }
}