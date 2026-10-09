using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using api.Helpers;

namespace api.Dtos.Usuario
{
    public class UpdateUsuarioDto
    {
        [Required]
        [MaxLength(100, ErrorMessage = "O nome não pode passar de 150 caractéres.")]
        public string Nome { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [MaxLength(100, ErrorMessage = "O E-mail deve ter no máximo 150 caractéres.")]
        public string Email { get; set; } = string.Empty;

        [DataNascimento]
        public DateTime DataNascimento { get; set; }
    }
}