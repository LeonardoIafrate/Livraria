namespace api.Models
{
    public class Endereco
    {
        public int Id { get; set; }
        public string Cep { get; set; } = string.Empty;
        public string Rua { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string UF { get; set; } = string.Empty;
        public bool Principal { get; set; }
        public bool Ativo { get; set; } = true;

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        
    }
}