using api.Dtos.Livro;
using api.Exceptions;
using api.Interfaces;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Xunit;
using Xunit.Runner.Common;

namespace api.Tests
{
    public class LivroServiceTests
    {
        private const int LivroId = 1;
        private const int AutorId = 2;
        private const int EditoraId = 3;
        private const int CategoriaId = 4;
        private const string Isbn = "9788532511010";

        private readonly ILivroRepository _livroRepo = Substitute.For<ILivroRepository>();
        private readonly IEditoraRepository _editoraRepo = Substitute.For<IEditoraRepository>();
        private readonly ICategoriaRepository _categoriaRepo = Substitute.For<ICategoriaRepository>();
        private readonly IAutorRepository _autorRepo = Substitute.For<IAutorRepository>();

        private LivroService CriarServico() => new(_livroRepo, _editoraRepo, _categoriaRepo,_autorRepo);

        private static Livro CriarLivro(int livroId = LivroId, string nome = "Harry Potter e a Pedra Filosofal",
        int estoque = 10)
        {
            var editora = CriarEditora();

            return new Livro()
            {
            Id = livroId,
            Nome = nome,
            Sinopse = "Harry Potter descobre, aos 11 anos, que é um bruxo e recebe uma carta para estudar na Escola de Magia e Bruxaria de Hogwarts. Lá, faz amigos, descobre segredos sobre seu passado e enfrenta um mistério envolvendo a Pedra Filosofal.",
            AnoLancamento = 2000,
            Isbn = Isbn,
            Preco = 30m,
            NumeroPaginas = 264,
            EditoraId = editora.Id,
            Editora = editora,
            Estoque = new Estoque { LivroId = livroId, Quantidade = estoque}
            };
        }

        private static CreateLivroDto CriarDto(
            string nome = "Harry Potter e a Pedra Filosofal",
            string isbn = Isbn,
            decimal preco = 30m,
            int editoraId = EditoraId,
            List<int>? autoresIds = null,
            List<int>? categoriaIds = null,
            int quantidadeInicial = 10) => new()
        {
            Nome = nome,
            Sinopse = "Harry Potter descobre, aos 11 anos, que é um bruxo e recebe uma carta para estudar na Escola de Magia e Bruxaria de Hogwarts.",
            AnoLancamento = 2000,
            Isbn = isbn,
            Preco = preco,
            NumeroPaginas = 264,
            Idioma = "Português",
            EditoraId = editoraId,
            AutoresIds = autoresIds ?? new List<int> { AutorId },
            CategoriaIds = categoriaIds ?? new List<int> { CategoriaId },
            QuantidadeInicial = quantidadeInicial
        };

        private static UpdateLivroDto CriarUpdateDto(
            string nome = "Harry Potter e a Câmara Secreta",
            string isbn = "9788532511010",
            decimal preco = 35m,
            int editoraId = EditoraId,
            List<int>? autoresIds = null,
            List<int>? categoriaIds = null) => new()
        {
            Nome = nome,
            Sinopse = "Harry volta a Hogwarts para o segundo ano e descobre que a Câmara Secreta foi aberta.",
            AnoLancamento = 2000,
            Isbn = isbn,
            Preco = preco,
            NumeroPaginas = 288,
            Idioma = "Português",
            EditoraId = editoraId,
            AutoresIds = autoresIds ?? new List<int> { AutorId },
            CategoriaIds = categoriaIds ?? new List<int> { CategoriaId }
        };

        private static Categoria CriarCategoria(int categoriaId = CategoriaId, string genero = "Ficção") => new()
        {
            Id = categoriaId,
            Genero = genero
        };

        private static Editora CriarEditora(int editoraId = EditoraId) => new()
        {
            Id = editoraId,
            Nome = "Rocco"
        };

        private static Autor CriarAutor(int autorId = AutorId) => new()
        {
          Id = autorId,
          Nome = "J.K. Rowling",
          Nacionalidade = "Britanica",
          Biografia = "J.K. Rowling (1965) é uma escritora britânica, mundialmente conhecida por criar a saga de livros Harry Potter. Sua obra conquistou milhões de leitores e foi adaptada para o cinema, tornando-se um fenômeno mundial."  
        };

        private void PrepararDependecias()
        {
            _editoraRepo.ExistsAsync(EditoraId).Returns(true);
            _categoriaRepo.GetByIdsAsync(Arg.Any<List<int>>()).Returns(new List<Categoria> {CriarCategoria()});
            _autorRepo.GetByIdsAsync(Arg.Any<List<int>>()).Returns(new List<Autor> {CriarAutor()});
        }

        [Fact]
        public async Task GetById_Inexistente_LancaNotFound()
        {
            _livroRepo.GetByIdAsync(LivroId).Returns((Livro?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().GetByIdAsync(LivroId));
        }

        [Fact]
        public async Task GetById_Existente_DevolveDto()
        {
            _livroRepo.GetByIdAsync(LivroId).Returns(CriarLivro());

            var resultado = await CriarServico().GetByIdAsync(LivroId);

            Assert.NotNull(resultado);
        }

        [Fact]
        public async Task GetByIsbn_Inexistente_LancaNotFound()
        {
            _livroRepo.GetByIsbnAsync(Isbn).Returns((Livro?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().GetByIsbnAsync(Isbn));
        }

        [Fact]
        public async Task GetByIsbn_RemoveHifensAntesDeBuscar()
        {
            _livroRepo.GetByIsbnAsync(Isbn).Returns(CriarLivro());

            var resultado = await CriarServico().GetByIsbnAsync("978-85-325-1101-0");

            Assert.NotNull(resultado);
        }

        [Fact]
        public async Task CreateLivro_IsbnEmUso_LancaConflictENaoSalva()
        {
            PrepararDependecias();
            _livroRepo.GetByIsbnAsync(isbn: Isbn).Returns(CriarLivro(livroId: 99));
            
            await Assert.ThrowsAsync<ConflictException>(() => CriarServico().CreateAsync(CriarDto()));

            await _livroRepo.DidNotReceive().CreateAsync(Arg.Any<Livro>());
        }

        [Fact]
        public async Task CreateLivro_EditoraInexistente_LancaBadRequestENaoSalva()
        {
            _editoraRepo.ExistsAsync(EditoraId).Returns(false);

            await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().CreateAsync(CriarDto()));

            await _livroRepo.DidNotReceive().CreateAsync(Arg.Any<Livro>());
        }

        [Fact]
        public async Task CreateLivro_CategoriaInexistente_LancaBadRequestENaoSalva()
        {
            PrepararDependecias();
            _categoriaRepo.GetByIdsAsync(Arg.Any<List<int>>()).Returns(new List<Categoria>());

            var erro = await Assert.ThrowsAsync<BadRequestException>(() => CriarServico().CreateAsync(CriarDto()));
            Assert.Contains(CategoriaId.ToString(), erro.Message);
            await _livroRepo.DidNotReceive().CreateAsync(Arg.Any<Livro>());
        }

        [Fact]
        public async Task CreateLivro_AutorInexsitente_LancaBadRequestENaoSalva()
        {
            PrepararDependecias();
            
            var erro = await Assert.ThrowsAsync<BadRequestException>(() => CriarServico().CreateAsync(CriarDto(autoresIds: new List<int> {AutorId, 99})));

            Assert.Contains("99", erro.Message);
        }

        [Fact]
        public async Task CreateLivro_Valido_SalvaComEstoqueAutoresECategorias()
        {
            PrepararDependecias();
            Livro? salvo = null;
            _livroRepo.CreateAsync(Arg.Do<Livro>(l => salvo = l)).Returns(CriarLivro());

            var resultado = await CriarServico().CreateAsync(CriarDto(quantidadeInicial: 7));

            Assert.NotNull(resultado);
            Assert.NotNull(salvo);
            Assert.Equal(EditoraId, salvo.EditoraId);
            Assert.Equal(Isbn, salvo.Isbn);
            Assert.Equal(7, salvo.Estoque!.Quantidade);
            Assert.Equal(AutorId, Assert.Single(salvo.Autores).Id);
            Assert.Equal(CategoriaId, Assert.Single(salvo.Categorias).Id);
        }

        [Fact]
        public async Task UpdateLivro_Inexistente_LancaNotFoundENaoAtualiza()
        {
            _livroRepo.GetByIdAsync(LivroId).Returns((Livro?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().UpdateAsync(LivroId, CriarUpdateDto()));

            await _livroRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Livro>());
        }

        [Fact]
        public async Task UpdateLivro_EditoraInexistente_LancaBadRequestENaoAtualiza()
        {
            _livroRepo.GetByIdAsync(LivroId).Returns(CriarLivro());
            _editoraRepo.ExistsAsync(EditoraId).Returns(false);

            await Assert.ThrowsAsync<BadRequestException>(() => CriarServico().UpdateAsync(LivroId, CriarUpdateDto()));

            await _livroRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Livro>());
        }

        [Fact]
        public async Task UpdateLivro_IsbnEmUso_LancaConflictENaoAtualiza()
        {
            PrepararDependecias();
            _livroRepo.GetByIdAsync(LivroId).Returns(CriarLivro());
            _livroRepo.GetByIsbnAsync(Isbn).Returns(CriarLivro(livroId: 99));

            await Assert.ThrowsAsync<ConflictException>(() => CriarServico().UpdateAsync(LivroId, CriarUpdateDto()));

            await _livroRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Livro>());
        }

        [Fact]
        public async Task UpdateLivro_CategoriaInexistente_LancaBadRequestENaoAtualiza()
        {
            PrepararDependecias();
            _livroRepo.GetByIdAsync(LivroId).Returns(CriarLivro());
            _categoriaRepo.GetByIdsAsync(Arg.Any<List<int>>()).Returns(new List<Categoria>());

            await Assert.ThrowsAsync<BadRequestException>(() => CriarServico().UpdateAsync(LivroId, CriarUpdateDto()));
            await _livroRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Livro>());
        }

        [Fact]
        public async Task UpdateLivro_AutorInexistente_LancaBadRequestENaoAtualiza()
        {
            PrepararDependecias();
            _livroRepo.GetByIdAsync(LivroId).Returns(CriarLivro());
            _autorRepo.GetByIdsAsync(Arg.Any<List<int>>()).Returns(new List<Autor>());

            await Assert.ThrowsAsync<BadRequestException>(() => CriarServico().UpdateAsync(LivroId, CriarUpdateDto(autoresIds: new List<int> {AutorId, 99})));

            await _livroRepo.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<Livro>());
        }

        [Fact]
        public async Task UpdateLivro_Valido_EnviaAutoresECategoriasAoRepositorio()
        {
            PrepararDependecias();
            _livroRepo.GetByIdAsync(LivroId).Returns(CriarLivro());

            Livro? enviado= null;
            _livroRepo.UpdateAsync(LivroId, Arg.Do<Livro>(l => enviado = l)).Returns(CriarLivro());

            await CriarServico().UpdateAsync(LivroId, CriarUpdateDto(preco:42m));

            Assert.NotNull(enviado);
            Assert.Equal(42m, enviado.Preco);
            Assert.Equal(AutorId, Assert.Single(enviado.Autores).Id);
            Assert.Equal(CategoriaId, Assert.Single(enviado.Categorias).Id);
        }

        [Fact]
        public async Task Delete_Inexistente_LancaNotFoundENaoExclui()
        {
            _livroRepo.GetByIdAsync(LivroId).Returns((Livro?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => CriarServico().DeleteAsync(LivroId));

            await _livroRepo.DidNotReceive().DeleteAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task Delete_LivroEmPedidos_LancaConflictENaoExclui()
        {
            _livroRepo.GetByIdAsync(LivroId).Returns(CriarLivro());
            _livroRepo.PossuiPedidosAsync(LivroId).Returns(true);

            await Assert.ThrowsAsync<ConflictException>(() => CriarServico().DeleteAsync(LivroId));

            await _livroRepo.DidNotReceive().DeleteAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task Delete_Existente_ChamaORepositorio()
        {
            _livroRepo.GetByIdAsync(LivroId).Returns(CriarLivro());
            _livroRepo.PossuiPedidosAsync(LivroId).Returns(false);

            await CriarServico().DeleteAsync(LivroId);

            await _livroRepo.Received(1).DeleteAsync(LivroId);
        }
    }
}