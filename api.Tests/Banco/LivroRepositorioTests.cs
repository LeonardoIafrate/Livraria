using api.Data;
using api.Helpers;
using api.Models;
using api.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests.Banco
{
    [Collection("SqlServer")]
    public class LivroRepositorioTests
    {
        private const string IsbnA = "9788535902778";
        private const string IsbnB = "9788535914849";
        private const string IsbnC = "9780140449136";

        private readonly SqlServerFixture _fixture;

        public LivroRepositorioTests(SqlServerFixture fixture) => _fixture = fixture;

        private static Autor NovoAutor(string nome = "Machado de Assis") => new() { Nome = nome };
        private static Categoria NovaCategoria(string genero = "Romance") => new() { Genero = genero };

        private static async Task SalvarAsync(AppDbContext contexto, params object[] entidades)
        {
            contexto.AddRange(entidades);
            await contexto.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAll_FiltraPorAutor()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var editora = DadosDeTeste.NovaEditora();
            var machado = NovoAutor("Machado de Assis");
            var alencar = NovoAutor("José de Alencar");

            var dom = DadosDeTeste.NovoLivro(editora, isbn: IsbnA, nome: "Dom Casmurro");
            dom.Autores.Add(machado);
            var iracema = DadosDeTeste.NovoLivro(editora, isbn: IsbnB, nome: "Iracema");
            iracema.Autores.Add(alencar);
            await SalvarAsync(contexto, dom, iracema);

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var resultado = await new LivroRepository(consulta).GetAllAsync(new LivroQueryObject { AutorId = machado.Id });

            Assert.Equal("Dom Casmurro", Assert.Single(resultado.Itens).Nome);
        }

        [Fact]
        public async Task GetAll_FiltraPorCategoria()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var editora = DadosDeTeste.NovaEditora();
            var romance = NovaCategoria("Romance");
            var infantil = NovaCategoria("Infantil");

            var dom = DadosDeTeste.NovoLivro(editora, isbn: IsbnA, nome: "Dom Casmurro");
            dom.Categorias.Add(romance);
            var narizinho = DadosDeTeste.NovoLivro(editora, isbn: IsbnB, nome: "Narizinho");
            narizinho.Categorias.Add(infantil);
            await SalvarAsync(contexto, dom, narizinho);

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var resultado = await new LivroRepository(consulta).GetAllAsync(new LivroQueryObject { CategoriaId = infantil.Id });

            Assert.Equal("Narizinho", Assert.Single(resultado.Itens).Nome);
        }

        [Fact]
        public async Task GetAll_FiltraPorEditora()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var aurora = DadosDeTeste.NovaEditora("Editora Aurora");
            var cais = DadosDeTeste.NovaEditora("Editora Cais");

            await SalvarAsync(contexto,
                DadosDeTeste.NovoLivro(aurora, isbn: IsbnA, nome: "Dom Casmurro"),
                DadosDeTeste.NovoLivro(cais, isbn: IsbnB, nome: "Vidas Secas"));

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var resultado = await new LivroRepository(consulta).GetAllAsync(new LivroQueryObject { EditoraId = cais.Id });

            Assert.Equal("Vidas Secas", Assert.Single(resultado.Itens).Nome);
        }

        [Fact]
        public async Task GetAll_PrecoMinimo_IncluiOValorExatoEOrdenaPeloNome()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var editora = DadosDeTeste.NovaEditora();

            await SalvarAsync(contexto,
                DadosDeTeste.NovoLivro(editora, isbn: IsbnA, nome: "Dom Casmurro", preco: 30m),
                DadosDeTeste.NovoLivro(editora, isbn: IsbnB, nome: "Memórias Póstumas", preco: 45m),
                DadosDeTeste.NovoLivro(editora, isbn: IsbnC, nome: "Dom Quixote", preco: 80m));

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var resultado = await new LivroRepository(consulta).GetAllAsync(new LivroQueryObject { PrecoMin = 45m });

            Assert.Equal(new[] { "Dom Quixote", "Memórias Póstumas" }, resultado.Itens.Select(l => l.Nome));
        }

        [Fact]
        public async Task GetByIds_DevolveSoOsLivrosPedidos()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var editora = DadosDeTeste.NovaEditora();
            var a = DadosDeTeste.NovoLivro(editora, isbn: IsbnA, nome: "A");
            var b = DadosDeTeste.NovoLivro(editora, isbn: IsbnB, nome: "B");
            var c = DadosDeTeste.NovoLivro(editora, isbn: IsbnC, nome: "C");
            await SalvarAsync(contexto, a, b, c);

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var resultado = await new LivroRepository(consulta).GetByIdsAsync(new List<int> { a.Id, c.Id });

            Assert.Equal(new[] { a.Id, c.Id }.OrderBy(x => x), resultado.Select(l => l.Id).OrderBy(x => x));
        }

        [Fact]
        public async Task GetByIsbn_EncontraSoOIsbnExato()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            await SalvarAsync(contexto, DadosDeTeste.NovoLivro(DadosDeTeste.NovaEditora(), isbn: IsbnA));

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var repo = new LivroRepository(consulta);

            Assert.NotNull(await repo.GetByIsbnAsync(IsbnA));
            Assert.Null(await repo.GetByIsbnAsync(IsbnB));
        }

        [Fact]
        public async Task SearchByName_BuscaPorParteDoNomeEOrdena()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var editora = DadosDeTeste.NovaEditora();

            await SalvarAsync(contexto,
                DadosDeTeste.NovoLivro(editora, isbn: IsbnA, nome: "Dom Quixote"),
                DadosDeTeste.NovoLivro(editora, isbn: IsbnB, nome: "Vidas Secas"),
                DadosDeTeste.NovoLivro(editora, isbn: IsbnC, nome: "Dom Casmurro"));

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var resultado = await new LivroRepository(consulta).SearchByNameAsync("dom");

            Assert.Equal(new[] { "Dom Casmurro", "Dom Quixote" }, resultado.Select(l => l.Nome));
        }

        [Fact]
        public async Task PossuiPedidos_VerdadeiroSoParaLivroComprado()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var editora = DadosDeTeste.NovaEditora();
            var comprado = DadosDeTeste.NovoLivro(editora, isbn: IsbnA, nome: "Comprado");
            var parado = DadosDeTeste.NovoLivro(editora, isbn: IsbnB, nome: "Parado");
            var pedido = DadosDeTeste.NovoPedido(DadosDeTeste.NovoUsuario(), comprado);
            await SalvarAsync(contexto, pedido, parado);

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var repo = new LivroRepository(consulta);

            Assert.True(await repo.PossuiPedidosAsync(comprado.Id));
            Assert.False(await repo.PossuiPedidosAsync(parado.Id));
        }
        [Fact]
        public async Task Create_GravaEDevolveComAEditoraCarregada()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var editora = DadosDeTeste.NovaEditora("Editora Aurora");
            await SalvarAsync(contexto, editora);

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            var livro = new Livro
            {
                Nome = "Dom Casmurro",
                Isbn = IsbnA,
                Preco = 30m,
                EditoraId = editora.Id,
                Estoque = new Estoque { Quantidade = 3 }
            };

            var criado = await new LivroRepository(outro).CreateAsync(livro);

            Assert.True(criado.Id > 0);
            Assert.Equal("Editora Aurora", criado.Editora.Nome);
        }

        [Fact]
        public async Task Update_SubstituiAutoresECategorias()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var editora = DadosDeTeste.NovaEditora();
            var autorA = NovoAutor("Autor A");
            var autorB = NovoAutor("Autor B");
            var categoriaX = NovaCategoria("Categoria X");
            var categoriaY = NovaCategoria("Categoria Y");

            var livro = DadosDeTeste.NovoLivro(editora, isbn: IsbnA);
            livro.Autores.Add(autorA);
            livro.Categorias.Add(categoriaX);
            await SalvarAsync(contexto, livro, autorB, categoriaY);

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            var autorBNoContexto = await outro.Set<Autor>().SingleAsync(a => a.Id == autorB.Id);
            var categoriaYNoContexto = await outro.Set<Categoria>().SingleAsync(c => c.Id == categoriaY.Id);

            var modelo = new Livro
            {
                Nome = "Novo nome",
                Isbn = IsbnA,
                Preco = 55m,
                EditoraId = editora.Id,
                Autores = { autorBNoContexto },
                Categorias = { categoriaYNoContexto }
            };

            var atualizado = await new LivroRepository(outro).UpdateAsync(livro.Id, modelo);

            Assert.NotNull(atualizado);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            var gravado = await conferencia.Set<Livro>()
                .Include(l => l.Autores).Include(l => l.Categorias)
                .SingleAsync(l => l.Id == livro.Id);

            Assert.Equal("Novo nome", gravado.Nome);
            Assert.Equal(55m, gravado.Preco);
            Assert.Equal(autorB.Id, Assert.Single(gravado.Autores).Id);
            Assert.Equal(categoriaY.Id, Assert.Single(gravado.Categorias).Id);
        }

        [Fact]
        public async Task Update_Inexistente_DevolveNull()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();

            var resultado = await new LivroRepository(contexto).UpdateAsync(9999, new Livro { Nome = "X", Isbn = IsbnA });

            Assert.Null(resultado);
        }

        [Fact]
        public async Task Delete_RemoveOLivro()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var livro = DadosDeTeste.NovoLivro(DadosDeTeste.NovaEditora(), isbn: IsbnA);
            await SalvarAsync(contexto, livro);

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            var removido = await new LivroRepository(outro).DeleteAsync(livro.Id);

            Assert.NotNull(removido);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            Assert.False(await conferencia.Set<Livro>().AnyAsync(l => l.Id == livro.Id));
        }
    }
}