namespace api.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Senha_hash { get; set; } = string.Empty;
        public PerfilUsuario perfil { get; set; } = PerfilUsuario.Cliente;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        public List<Endereco> Enderecos { get; set; } = new();
    }
}