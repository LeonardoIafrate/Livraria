namespace api.Models
{
    public class Estoque
    {
        public int Id { get; set; }
        public int Quantidade { get; set; }
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        public int LivroId { get; set; }
        public Livro Livro { get; set; } = null!;
    }
}