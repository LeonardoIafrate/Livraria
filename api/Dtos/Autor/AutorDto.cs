namespace api.Dtos.Autor
{
    public class AutorDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Biografia { get; set; }
        public string? Nacionalidade { get; set; }
    }
}