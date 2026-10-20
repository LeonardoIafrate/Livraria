using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Usuario
{
    public class AlterarSenhaDto
    {
        [Required(ErrorMessage = "Informe a senha atual.")]
        [MaxLength(100, ErrorMessage = "A senha atual não pode ultrapassar 100 caracteres.")]
        public string SenhaAtual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a nova senha.")]
        [MinLength(8, ErrorMessage = "A senha deve conter no mínimo 8 caracteres.")]
        [MaxLength(100, ErrorMessage = "A senha deve conter no máximo 8 caracteres.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "A senha deve conter letras e números.")]
        public string NovaSenha { get; set; } = string.Empty;
    }
}