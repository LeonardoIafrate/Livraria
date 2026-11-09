namespace api.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Cliente;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;

        public List<Endereco> Enderecos { get; set; } = new();
        public List<Pedido> Pedidos { get; set;} = new();

        public int TentativasFalhas { get; set; }
        public DateTime? BloqueadoAte { get; set; }

        public bool EstaBloqueado(DateTime agora) => BloqueadoAte is { } ate && ate > agora;

        public void RegistrarFalhasDeSenha(DateTime agora, int MaxTentativas, TimeSpan duracao)
        {
            TentativasFalhas++;

            if(TentativasFalhas >= MaxTentativas)
            {
                BloqueadoAte = agora.Add(duracao);
                TentativasFalhas = 0;
            }
        }

        public void LimparFalhas()
        {
            TentativasFalhas = 0;
            BloqueadoAte = null;
        }
    }
}