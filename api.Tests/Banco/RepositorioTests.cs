using api.Data;
using api.Helpers;
using api.Models;
using api.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests.Banco
{
    [Collection("SqlServer")]
    public class RepositorioTests
    {
        private readonly SqlServerFixture _fixture;

        public RepositorioTests(SqlServerFixture fixture) => _fixture = fixture;
        
        [Fact]
        public async Task CancelarPedido_MudaOStatusEDevolveEstoque()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();

            var livro = DadosDeTeste.NovoLivro(DadosDeTeste.NovaEditora(), estoque: 8);
            var pedido = DadosDeTeste.NovoPedido(DadosDeTeste.NovoUsuario(), livro, quantidade: 2);
            contexto.Set<Pedido>().Add(pedido);
            await contexto.SaveChangesAsync();

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            var cancelado = await new PedidoRepository(outro).CancelarPedidoAsync(pedido.Id);

            Assert.NotNull(cancelado);
            Assert.Equal(StatusPedido.Cancelado, cancelado.Status);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            var estoque = await conferencia.Set<Estoque>().SingleAsync(e => e.LivroId == livro.Id);
            Assert.Equal(10, estoque.Quantidade);
        }

        
        [Fact]
        public async Task CreatePedido_GravaOPedidoEODescontoDeEstoqueNaMesmaTransacao()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var usuario = DadosDeTeste.NovoUsuario();
            var livro = DadosDeTeste.NovoLivro(DadosDeTeste.NovaEditora(), estoque: 10);
            contexto.Set<Usuario>().Add(usuario);
            contexto.Set<Livro>().Add(livro);
            await contexto.SaveChangesAsync();

            livro.Estoque!.Quantidade -= 2;

            var pedido = new Pedido
            {
              UsuarioId = usuario.Id,
              EnderecoEntrega = DadosDeTeste.NovaEntrega(),
              ValorTotal = 60m,
              Itens = { new ItemPedido {LivroId = livro.Id, Quantidade = 2, PrecoUnitario = 30m} }  
            };

            var criado = await new PedidoRepository(contexto).CreatePedidoAsync(pedido);

            Assert.True(criado.Id > 0);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            Assert.Equal(1, await conferencia.Set<Pedido>().CountAsync());
            var estoque = await conferencia.Set<Estoque>().SingleAsync(e => e.LivroId == livro.Id);
            Assert.Equal(8, estoque.Quantidade);
        }

        [Fact]
        public async Task CreatePedido_QuandoOEstoqueFicariaNegativo_NaoGravaNada()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var usuario = DadosDeTeste.NovoUsuario();
            var livro = DadosDeTeste.NovoLivro(DadosDeTeste.NovaEditora(), estoque: 1);
            contexto.Set<Usuario>().Add(usuario);
            contexto.Set<Livro>().Add(livro);
            await contexto.SaveChangesAsync();

            livro.Estoque!.Quantidade -= 5;
            var pedido = new Pedido
            {
              UsuarioId = usuario.Id,
              EnderecoEntrega = DadosDeTeste.NovaEntrega(),
              ValorTotal = 150m,
              Itens = { new ItemPedido { LivroId = livro.Id, Quantidade = 5, PrecoUnitario = 30m} }  
            };

            await Assert.ThrowsAsync<DbUpdateException>(() => new PedidoRepository(contexto).CreatePedidoAsync(pedido));

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            Assert.Equal(0, await conferencia.Set<Pedido>().CountAsync());
            var estoque = await conferencia.Set<Estoque>().SingleAsync(e => e.LivroId == livro.Id);
            Assert.Equal(1, estoque.Quantidade);
        }

        [Fact]
        public async Task DefinirPrincipal_MantemUmUnicoPrincipalPorUsuario()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var usuario = DadosDeTeste.NovoUsuario();
            var primeiro = DadosDeTeste.NovoEndereco(principal: true, rua: "Rua A");
            var segundo = DadosDeTeste.NovoEndereco(principal: false, rua: "Rua B");
            usuario.Enderecos.Add(primeiro);
            usuario.Enderecos.Add(segundo);
            contexto.Set<Usuario>().Add(usuario);
            await contexto.SaveChangesAsync();

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            await new EnderecoRepository(outro).DefinirPrincipalAsync(segundo.Id);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            var enderecos = await conferencia.Set<Endereco>().Where(e => e.UsuarioId == usuario.Id).ToListAsync();

            var principal = Assert.Single(enderecos, e => e.Principal);
            Assert.Equal(segundo.Id, principal.Id);
        }

        private static async Task CriarLivrosAsync(AppDbContext contexto)
        {
            var editora = DadosDeTeste.NovaEditora();

            contexto.Set<Livro>().AddRange(
                DadosDeTeste.NovoLivro(editora, isbn: "9788535902778", nome: "Dom Casmurro", preco: 30m),
                DadosDeTeste.NovoLivro(editora, isbn: "9788535914849", nome: "Memórias Póstumas", preco: 45m),
                DadosDeTeste.NovoLivro(editora, isbn: "9780140449136", nome: "Dom Quixote", preco: 80m)
            );

            await contexto.SaveChangesAsync();
        }

        [Fact]
        public async Task Livros_FiltrarPorNomeEFaixaDePreco()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            await CriarLivrosAsync(contexto);

            var resultado = await new LivroRepository(contexto).GetAllAsync(new LivroQueryObject { Nome = "Dom", PrecoMax = 50m});

            Assert.Equal(1, resultado.Total);
            Assert.Equal("Dom Casmurro", Assert.Single(resultado.Itens).Nome);
        }

        [Fact]
        public async Task Livros_PaginaEOrdenaPeloNome()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            await CriarLivrosAsync(contexto);

            var resultado = await new LivroRepository(contexto).GetAllAsync(new LivroQueryObject { Pagina = 2, TamanhoPagina = 2});

            Assert.Equal(3, resultado.Total);
            Assert.Equal(2, resultado.TotalPaginas);
            Assert.Equal("Memórias Póstumas", Assert.Single(resultado.Itens).Nome);
        }
    }
}