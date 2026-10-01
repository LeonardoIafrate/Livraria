using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Editora
{
    public class CreateEditoraDto
    {
        [Required]
        [MaxLength(100, ErrorMessage = "O nome da editora não pode conter mais de 100 caractéres.")]
        public string Nome { get; set; } = string.Empty;
    }
}