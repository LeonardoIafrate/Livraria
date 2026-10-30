using api.Data;
using api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests.Banco
{
    [Collection("SqlServer")]
    public class RestricoesDoBancoTests
    {
        private const int ViolacaoDeConstraint = 547;

        private static readonly int[] ViolacaoDeIndiceUnico = { 2601, 2627 };

        private readonly SqlServerFixture _fixture;

        public RestricoesDoBancoTests(SqlServerFixture fixture) => _fixture = fixture;

        private static async Task<int> NumeroDoErroSqlAsync(Func<Task> acao)
        {
            var excecao = await Assert.ThrowsAsync<DbUpdateException>(acao);
            var erroSql = Assert.IsType<SqlException>(excecao.InnerException);

            return erroSql.Number;
        }

        private static async Task<(int UsuarioId, int EnderecoId, int LivroId, int PedidoId)> CriarPedidoAsync(AppDbContext contexto)
        {
            var usuario = DadosDeTeste.NovoUsuario();
            var endereco = DadosDeTeste.NovoEndereco();
            usuario.Enderecos.Add(endereco);

            var livro = DadosDeTeste.NovoLivro(DadosDeTeste.NovaEditora());
            var pedido = DadosDeTeste.NovoPedido(usuario, livro);

            contexto.Set<Pedido>().Add(pedido);
            await contexto.SaveChangesAsync();

            return (usuario.Id, endereco.Id, livro.Id, pedido.Id);
        }

        [Fact]
        public async Task Migrations_AplicamEmUmBancoVazioESemPendencias()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
    
            Assert.Empty(await contexto.Database.GetPendingMigrationsAsync());
        }

        [Fact]
        public async Task Email_Duplicado_ViolaOIndiceUnico()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            contexto.Set<Usuario>().Add(DadosDeTeste.NovoUsuario(email: "ana@email.com", cpf: "52998224725"));
            await contexto.SaveChangesAsync();
    
            contexto.Set<Usuario>().Add(DadosDeTeste.NovoUsuario(email: "ana@email.com", cpf: "58310472617"));
    
            var numero = await NumeroDoErroSqlAsync(() => contexto.SaveChangesAsync());
    
            Assert.Contains(numero, ViolacaoDeIndiceUnico);
        }
    
        [Fact]
        public async Task Cpf_Duplicado_ViolaOIndiceUnico()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            contexto.Set<Usuario>().Add(DadosDeTeste.NovoUsuario(email: "ana@email.com", cpf: "52998224725"));
            await contexto.SaveChangesAsync();
    
            contexto.Set<Usuario>().Add(DadosDeTeste.NovoUsuario(email: "bia@email.com", cpf: "52998224725"));
    
            var numero = await NumeroDoErroSqlAsync(() => contexto.SaveChangesAsync());
    
            Assert.Contains(numero, ViolacaoDeIndiceUnico);
        }
    
        [Fact]
        public async Task Isbn_Duplicado_ViolaOIndiceUnico()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var editora = DadosDeTeste.NovaEditora();
            contexto.Set<Livro>().Add(DadosDeTeste.NovoLivro(editora, isbn: "9788535902778"));
            await contexto.SaveChangesAsync();
    
            contexto.Set<Livro>().Add(DadosDeTeste.NovoLivro(editora, isbn: "9788535902778", nome: "Outro livro"));
    
            var numero = await NumeroDoErroSqlAsync(() => contexto.SaveChangesAsync());
    
            Assert.Contains(numero, ViolacaoDeIndiceUnico);
        }
    
        [Fact]
        public async Task DoisEnderecosPrincipaisDoMesmoUsuario_ViolamOIndiceFiltrado()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var usuario = DadosDeTeste.NovoUsuario();
            usuario.Enderecos.Add(DadosDeTeste.NovoEndereco(principal: true, rua: "Rua A"));
            usuario.Enderecos.Add(DadosDeTeste.NovoEndereco(principal: true, rua: "Rua B"));
            contexto.Set<Usuario>().Add(usuario);
    
            var numero = await NumeroDoErroSqlAsync(() => contexto.SaveChangesAsync());
    
            Assert.Contains(numero, ViolacaoDeIndiceUnico);
        }
    
        [Fact]
        public async Task EstoqueNegativo_ViolaACheckConstraint()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            contexto.Set<Livro>().Add(DadosDeTeste.NovoLivro(DadosDeTeste.NovaEditora(), estoque: -1));
    
            var numero = await NumeroDoErroSqlAsync(() => contexto.SaveChangesAsync());
    
            Assert.Equal(ViolacaoDeConstraint, numero);
        }
    
        [Fact]
        public async Task PedidoComValorTotalZero_ViolaACheckConstraint()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var livro = DadosDeTeste.NovoLivro(DadosDeTeste.NovaEditora());
            var pedido = DadosDeTeste.NovoPedido(DadosDeTeste.NovoUsuario(), livro);
            pedido.ValorTotal = 0;
            contexto.Set<Pedido>().Add(pedido);
    
            var numero = await NumeroDoErroSqlAsync(() => contexto.SaveChangesAsync());
    
            Assert.Equal(ViolacaoDeConstraint, numero);
        }
    
        [Fact]
        public async Task ItemComQuantidadeZero_ViolaACheckConstraint()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var livro = DadosDeTeste.NovoLivro(DadosDeTeste.NovaEditora());
            var pedido = DadosDeTeste.NovoPedido(DadosDeTeste.NovoUsuario(), livro, quantidade: 2);
            pedido.Itens[0].Quantidade = 0;   // o ValorTotal continua positivo
            contexto.Set<Pedido>().Add(pedido);
    
            var numero = await NumeroDoErroSqlAsync(() => contexto.SaveChangesAsync());
    
            Assert.Equal(ViolacaoDeConstraint, numero);
        }

        [Fact]
        public async Task Usuario_ComPedidos_NaoPodeSerExcluido()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var ids = await CriarPedidoAsync(contexto);
    
            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            var usuario = await outro.Set<Usuario>().SingleAsync(u => u.Id == ids.UsuarioId);
            outro.Set<Usuario>().Remove(usuario);
    
            var numero = await NumeroDoErroSqlAsync(() => outro.SaveChangesAsync());
    
            Assert.Equal(ViolacaoDeConstraint, numero);
        }
    
        [Fact]
        public async Task Livro_JaVendido_NaoPodeSerExcluido()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var ids = await CriarPedidoAsync(contexto);
    
            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            var livro = await outro.Set<Livro>().SingleAsync(l => l.Id == ids.LivroId);
            outro.Set<Livro>().Remove(livro);
    
            var numero = await NumeroDoErroSqlAsync(() => outro.SaveChangesAsync());
    
            Assert.Equal(ViolacaoDeConstraint, numero);
        }
    
        [Fact]
        public async Task Endereco_UsadoEmPedido_PodeSerExcluido_EOPedidoMantemACopia()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var ids = await CriarPedidoAsync(contexto);
    
            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            var endereco = await outro.Set<Endereco>().SingleAsync(e => e.Id == ids.EnderecoId);
            outro.Set<Endereco>().Remove(endereco);
            await outro.SaveChangesAsync();  
    
            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            Assert.False(await conferencia.Set<Endereco>().AnyAsync(e => e.Id == ids.EnderecoId));
    
            var pedido = await conferencia.Set<Pedido>().SingleAsync(p => p.Id == ids.PedidoId);
            Assert.Equal("Rua das Flores", pedido.EnderecoEntrega.Rua);
        }
    
        [Fact]
        public async Task Usuario_SemPedidos_Excluido_ApagaOsEnderecosEmCascata()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var usuario = DadosDeTeste.NovoUsuario();
            usuario.Enderecos.Add(DadosDeTeste.NovoEndereco(principal: true, rua: "Rua A"));
            usuario.Enderecos.Add(DadosDeTeste.NovoEndereco(principal: false, rua: "Rua B"));
            contexto.Set<Usuario>().Add(usuario);
            await contexto.SaveChangesAsync();
    
            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            var paraExcluir = await outro.Set<Usuario>().SingleAsync(u => u.Id == usuario.Id);
            outro.Set<Usuario>().Remove(paraExcluir);
            await outro.SaveChangesAsync();
    
            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            Assert.Equal(0, await conferencia.Set<Endereco>().CountAsync());
        }
    
        [Fact]
        public async Task Pedido_Excluido_ApagaOsItensEmCascata()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var ids = await CriarPedidoAsync(contexto);
    
            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            var pedido = await outro.Set<Pedido>().SingleAsync(p => p.Id == ids.PedidoId);
            outro.Set<Pedido>().Remove(pedido);
            await outro.SaveChangesAsync();
    
            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            Assert.Equal(0, await conferencia.Set<ItemPedido>().CountAsync());
        }
    
        [Fact]
        public async Task Estoque_AlteradoPorDoisContextosAoMesmoTempo_ASegundaGravacaoFalha()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var livro = DadosDeTeste.NovoLivro(DadosDeTeste.NovaEditora(), estoque: 1);
            contexto.Set<Livro>().Add(livro);
            await contexto.SaveChangesAsync();
            var estoqueId = livro.Estoque!.Id;
    
            await using var primeiro = _fixture.NovoContextoDoMesmoBanco(contexto);
            await using var segundo = _fixture.NovoContextoDoMesmoBanco(contexto);
            var estoquePrimeiro = await primeiro.Set<Estoque>().SingleAsync(e => e.Id == estoqueId);
            var estoqueSegundo = await segundo.Set<Estoque>().SingleAsync(e => e.Id == estoqueId);
    
            estoquePrimeiro.Quantidade = 0;
            await primeiro.SaveChangesAsync();
    
            estoqueSegundo.Quantidade = 0;
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => segundo.SaveChangesAsync());
        }
    }
}