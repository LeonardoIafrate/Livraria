using System.ComponentModel.DataAnnotations;
using api.Helpers;

namespace api.Dtos.Usuario
{
    public class CreateUsuarioDto
    {
        [Required]
        [MaxLength(100, ErrorMessage = "O nome não pode passar de 150 caractéres.")]
        public string Nome { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [MaxLength(100, ErrorMessage = "O E-mail deve ter no máximo 150 caractéres.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(11, ErrorMessage = "CPF inválido.")]
        [Cpf]
        public string Cpf { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "A senha deve conter no mínimo 8 caracteres.")]
        [MaxLength(30, ErrorMessage = "A senha deve conter no mínimo 8 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "A senha deve conter letras e números.")]
        public string SenhaHash { get; set; } = string.Empty;

        [DataNascimento]
        public DateTime DataNascimento { get; set; }
    }
}