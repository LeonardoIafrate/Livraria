namespace api.Models
{
    public class Autor
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Biografia { get; set; }
        public string? Nacionalidade { get; set; }

        public List<Livro> Livros { get; set; } = new();
    }
}