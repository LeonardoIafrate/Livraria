namespace api.Models
{
    public class Livro
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Sinopse { get; set; }
        public int? AnoLancamento { get; set; }
        public string Isbn { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int? NumeroPaginas { get; set; }
        public string? Idioma { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        public int EditoraId { get; set; }
        public Editora Editora { get; set; } = null!;

        public List<Categoria> Categorias { get; set; } = new();

        public List<Autor> Autores { get; set; } = new();

        public Estoque? Estoque { get; set; }
    }
}