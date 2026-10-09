using api.Models;

namespace api.Dtos.Usuario
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public PerfilUsuario Perfil { get; set;}
        public bool Ativo { get; set; }
    }
}