namespace api.Dtos.Estoque
{
    public class EstoqueDto
    {
        public int Id { get; set; }
        public int Quantidade { get; set; }
        public DateTime DataAtualizacao { get; set; }
        
        public int LivroId { get; set; }
        public string NomeLivro { get; set; } = string.Empty;
    }
}