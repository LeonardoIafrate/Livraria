using System.ComponentModel.DataAnnotations;


namespace api.Dtos.Livro
{
    public class UpdateLivroDto
    {
        [Required]
        [MaxLength(250, ErrorMessage = "O nome do livro não pode utrapassar 250 caracteres")]
        public string Nome { get; set; } = string.Empty;
        [MaxLength(2000, ErrorMessage = "A sinopse não pode ultrapassar 2000 caracteres.")]
        public string? Sinopse { get; set; }
        [Range(1000, 2100)]
        public int? AnoLancamento { get; set; }
        [Required]
        [RegularExpression(@"^(\d{9}[\dXx]|\d{13})$", ErrorMessage = "O ISBN deve conter 10 ou 13 dígitos, sem hífens.")]
        public string Isbn{ get; set; } = string.Empty;
        [Required]
        [Range(0.1, 10000)]
        public decimal Preco { get; set; }
        [Range(1, 10000, ErrorMessage = "O número de páginas deve estar entre 1 e 10000")]
        public int? NumeroPaginas { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "O Idioma do livro deve conter no máximo 50 caracteres")]
        public string Idioma { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "")]
        public int EditoraId { get; set; }
    }
}