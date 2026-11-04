namespace api.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Genero { get; set; } = string.Empty;

        public List<Livro> Livros { get; set; } = new();
    }
}