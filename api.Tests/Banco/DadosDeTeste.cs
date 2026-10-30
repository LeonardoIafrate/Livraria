using api.Models;

namespace api.Tests.Banco
{
    internal static class DadosDeTeste
    {
        public static Usuario NovoUsuario(string email = "ana@gmail.com", string cpf = "52998224725") => new()
        {
          Nome = "Ana Souza",
          Email = email,
          Cpf = cpf,
          SenhaHash = "hash-de-teste",
          DataNascimento = new DateTime(1990, 5, 20),
          Perfil = PerfilUsuario.Cliente 
        };

        public static Endereco NovoEndereco(bool principal = false, string rua = "Rua das Flores") => new()
        {
          Cep = "09010100",
          Rua = rua,
          Numero = "123",
          Bairro = "Centro",
          Cidade = "Santo André",
          Uf = "SP",
          Principal = principal,
          Ativo = true
        };

        public static EnderecoEntrega NovaEntrega() => new()
        {
          Cep = "09010100",
          Rua = "Rua das Flores",
          Numero = "123",
          Bairro = "Centro",
          Cidade = "Santo André",
          Uf = "SP"
        };

        public static Editora NovaEditora(string nome = "Comapanhia das Letras") => new() {Nome = nome};

        public static Livro NovoLivro(
            Editora editora, 
            string isbn = "9788535902778", 
            string nome = "Dom Casmurro", 
            decimal preco = 30,
            int estoque = 10) => new()
        {
            Nome = nome,
            Isbn = isbn,
            Preco = preco,
            Editora = editora,
            Estoque = new Estoque { Quantidade = estoque }
        };

        public static Pedido NovoPedido(Usuario usuario, Livro livro, int quantidade = 2) => new()
        {
          Usuario = usuario,
          EnderecoEntrega = NovaEntrega(),
          Status = StatusPedido.Pendente,
          ValorTotal = livro.Preco * quantidade,
          Itens =
            {
                new ItemPedido { Livro = livro, Quantidade = quantidade, PrecoUnitario = livro.Preco}
            }  
        };
    }
}