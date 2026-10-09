using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Endereco
{
    public class EnderecoBaseDto
    {
        [Required]
        [MaxLength(8, ErrorMessage = "CEP inválido")]
        public string Cep { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100, ErrorMessage = "A rua deve conter no máximo 10 caracteres.")]
        public string Rua { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(10, ErrorMessage = "O número deve conter no máximo 10 caracteres.")]
        public string Numero { get; set; } = string.Empty;
        
        [MaxLength(100, ErrorMessage = "O complemento deve conter no máximo 100 caracteres.")]
        public string Complemento { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100, ErrorMessage = "O bairro deve conter no máximo 10 caracteres.")]
        public string Bairro { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100, ErrorMessage = "A cidade deve conter no máximo 100 caracteres.")]
        public string Cidade { get; set; } = string.Empty;
        
        [Required]
        [RegularExpression(@"^(?i)(AC|AL|AP|AM|BA|CE|DF|ES|GO|MA|MT|MS|MG|PA|PB|PR|PE|PI|RJ|RN|RS|RO|RR|SC|SP|SE|TO)$", 
        ErrorMessage = "UF inválida. Informe uma sigla válida de estado (ex: SP, RJ, MG).")]
        public string Uf { get; set; } = string.Empty;
    }
}