using System.ComponentModel.DataAnnotations;
using api.Dtos.Endereco;
using api.Helpers;

namespace api.Dtos.Usuario
{
    public class CreateUsuarioDto
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O nome não pode passar de 100 caractéres.")]
        public string Nome { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [MaxLength(100, ErrorMessage = "O E-mail deve ter no máximo 100 caractéres.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [MaxLength(14, ErrorMessage = "CPF inválido.")]
        [Cpf]
        public string Cpf { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "A senha deve conter no mínimo 8 caracteres.")]
        [MaxLength(100, ErrorMessage = "A senha deve conter no máximo 8 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "A senha deve conter letras e números.")]
        public string Senha { get; set; } = string.Empty;

        [DataNascimento]
        public DateTime DataNascimento { get; set; }

        
        [Required(ErrorMessage = "Informe um endereço.")]
        public EnderecoBaseDto Endereco { get; set; } = null!;
    }
}