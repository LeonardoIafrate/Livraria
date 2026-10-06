namespace api.Dtos.Livro
{
    public class LivroDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Sinopse { get; set; }
        public int? AnoLancamento { get; set; }
        public string Isbn { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int? NumeroPaginas { get; set; }
        public string? Idioma { get; set; }
        public DateTime DataCadastro { get; set; }

        public int EditoraId { get; set; }
        public string EditoraNome { get; set; } = string.Empty;

        public List<string> Generos { get; set; } = new();

        public List<string> Autores { get; set; } = new();

        public int QuantidadeEstoque { get; set; }
    }
}