using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Editora
{
    public class UpdateEditoraDto
    {
        [Required]
        [MaxLength(100, ErrorMessage = "O nome da editora não pode conter mais de 100 caractéres.")]
        public string Nome { get; set; } = string.Empty;
    }
}