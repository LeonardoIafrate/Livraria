using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [MaxLength(100, ErrorMessage = "O e-mail não pode ultrapassar 100 caracteres.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A senha não pode ultrapassar 100 caracteres.")]
        public string Senha { get; set; } = string.Empty;
    }
}