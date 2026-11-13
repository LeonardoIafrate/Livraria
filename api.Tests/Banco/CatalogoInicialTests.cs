using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace api.Tests.Banco
{
    [Collection("SqlServer")]
    public class CatalogoInicialTests
    {
        private readonly SqlServerFixture _fixture;

        public CatalogoInicialTests(SqlServerFixture fixture) => _fixture = fixture;

        private static Task SemearAsync(AppDbContext contexto) => CatalogoInicial.SemearAsync(contexto, NullLogger.Instance);

        private static int SomaPonderada(string isbn) => isbn.Select((c, i) => (c - '0') * (i % 2 == 0 ? 1: 3)).Sum();

        [Fact]
        public async Task Semear_BancoVazio_CriaOCatalogoCompleto()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();

            await SemearAsync(contexto);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            Assert.Equal(13, await conferencia.Set<Livro>().CountAsync());
            Assert.Equal(10, await conferencia.Set<Autor>().CountAsync());
            Assert.Equal(8, await conferencia.Set<Categoria>().CountAsync());
            Assert.Equal(3, await conferencia.Set<Editora>().CountAsync());
        }

        [Fact]
        public async Task Semear_GravaOEstoqueDeCadaLivro_InclusiveZero()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();

            await SemearAsync(contexto);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            var estoques = await conferencia.Set<Estoque>().Include(e => e.Livro).ToListAsync();

            Assert.Equal(13, estoques.Count);
            Assert.Equal(25, estoques.Single(e => e.Livro.Nome == "Dom Casmurro").Quantidade);
            Assert.Equal(0, estoques.Single(e => e.Livro.Nome == "Vidas Secas").Quantidade);
        }

        [Fact]
        public async Task Semear_GeraIsbnDeUsoUnicoComDigitoVerificadorValido()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();

            await SemearAsync(contexto);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            var isbns = await conferencia.Set<Livro>().Select(l => l.Isbn).ToListAsync();

            Assert.Equal(isbns.Count, isbns.Distinct().Count());
            Assert.All(isbns, isbn =>
            {
                Assert.Equal(13, isbn.Length);
                Assert.Equal(0, SomaPonderada(isbn) % 10);
            });
        }

        [Fact]
        public async Task Semear_ChamaDuasVezes_NaoDuplicaNada()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            await SemearAsync(contexto);

            await using var segundo = _fixture.NovoContextoDoMesmoBanco(contexto);
            await SemearAsync(segundo);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            Assert.Equal(13, await conferencia.Set<Livro>().CountAsync());
            Assert.Equal(10, await conferencia.Set<Autor>().CountAsync());
        }

        [Fact]
        public async Task Semear_ComLivrosJaCadastrados_NaoFazNada()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            contexto.Set<Livro>().Add(DadosDeTeste.NovoLivro(DadosDeTeste.NovaEditora("Outra Editora")));
            await contexto.SaveChangesAsync();

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            await SemearAsync(outro);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            Assert.Equal(1, await conferencia.Set<Livro>().CountAsync());
            Assert.Equal(1, await conferencia.Set<Editora>().CountAsync());
        }

        [Fact]
        public async Task Semear_ReaproveitaUmaEditoraQueJaExiste()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            contexto.Set<Editora>().Add(new Editora { Nome = "Editora Aurora" });
            await contexto.SaveChangesAsync();

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            await SemearAsync(outro);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            Assert.Equal(3, await conferencia.Set<Editora>().CountAsync());

            var aurora = await conferencia.Set<Editora>().Include(e => e.Livros)
                .SingleAsync(e => e.Nome == "Editora Aurora");
            Assert.Equal(5, aurora.Livros.Count);
        }
    }
}