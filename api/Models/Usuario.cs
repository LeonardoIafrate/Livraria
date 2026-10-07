namespace api.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Cliente;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        public List<Endereco> Enderecos { get; set; } = new();
    }
}