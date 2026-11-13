using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using api.Dtos.Autor;
using api.Dtos.Categoria;
using api.Dtos.Editora;
using api.Dtos.Estoque;
using api.Dtos.Livro;
using Xunit;

namespace api.Tests.Integracao
{
    [Collection("Api")]
    public class CatalogoApiTests : ApiTestBase
    {
        public CatalogoApiTests(ApiFixture fixture) : base(fixture)
        {
        }

        private static string Unico(string prefixo) => $"{prefixo}-{Guid.NewGuid().ToString("N")[..8]}";

        private static string NovoIsbn() => Random.Shared.NextInt64(1_000_000_000_000, 10_000_000_000_000).ToString();

        private static StringContent CorpoVazio() => new("{}", Encoding.UTF8, "application/json");

        private static async Task<JsonElement> LerJsonAsync(HttpResponseMessage resposta)
        {
            await GarantirSucessoAsync(resposta);
            using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
            return documento.RootElement.Clone();
        }

        private static async Task<int> CriarEditoraAsync(HttpClient admin)
        {
            var resposta = await admin.PostAsJsonAsync("/api/editoras", new CreateEditoraDto { Nome = Unico("Editora") });
            await GarantirSucessoAsync(resposta);
            return (await resposta.Content.ReadFromJsonAsync<EditoraDto>())!.Id;
        }

        private static async Task<int> CriarAutorAsync(HttpClient admin)
        {
            var resposta = await admin.PostAsJsonAsync("/api/autores",
                new CreateAutorDto { Nome = Unico("Autor"), Nacionalidade = "Brasileira", Biografia = "Biografia de teste" });
            await GarantirSucessoAsync(resposta);
            return (await resposta.Content.ReadFromJsonAsync<AutorDto>())!.Id;
        }

        private static async Task<int> CriarCategoriaAsync(HttpClient admin)
        {
            var resposta = await admin.PostAsJsonAsync("/api/categorias", new CreateCategoriaDto { Genero = Unico("Genero") });
            await GarantirSucessoAsync(resposta);
            return (await resposta.Content.ReadFromJsonAsync<CategoriaDto>())!.Id;
        }

        private static CreateLivroDto NovoLivroDto(int editoraId, int autorId, int? categoriaId = null, string? nome = null, string? isbn = null) => new()
        {
            Nome = nome ?? Unico("Livro"),
            Isbn = isbn ?? NovoIsbn(),
            Preco = 30m,
            Idioma = "Português",
            EditoraId = editoraId,
            AutoresIds = new List<int> { autorId },
            CategoriaIds = categoriaId.HasValue ? new List<int> { categoriaId.Value } : new List<int>(),
            QuantidadeInicial = 10
        };

        private static UpdateLivroDto NovoUpdateDto(int editoraId, int autorId, string isbn, decimal preco = 45m) => new()
        {
            Nome = Unico("Livro"),
            Isbn = isbn,
            Preco = preco,
            Idioma = "Português",
            EditoraId = editoraId,
            AutoresIds = new List<int> { autorId },
            CategoriaIds = new List<int>()
        };

        private static async Task<int> CriarLivroAsync(HttpClient admin, CreateLivroDto dto)
        {
            var resposta = await admin.PostAsJsonAsync("/api/livros", dto);
            await GarantirSucessoAsync(resposta);
            return (await resposta.Content.ReadFromJsonAsync<LivroDto>())!.Id;
        }

        [Theory]
        [InlineData("/api/livros")]
        [InlineData("/api/autores")]
        [InlineData("/api/categorias")]
        [InlineData("/api/editoras")]
        public async Task Consultas_SemToken_SaoPublicas(string rota)
        {
            var resposta = await Fixture.CriarCliente().GetAsync(rota);

            await AssertStatusAsync(HttpStatusCode.OK, resposta);
        }

        [Theory]
        [InlineData("/api/livros")]
        [InlineData("/api/autores")]
        [InlineData("/api/categorias")]
        [InlineData("/api/editoras")]
        public async Task Cadastro_SemToken_Retorna401(string rota)
        {
            var resposta = await Fixture.CriarCliente().PostAsync(rota, CorpoVazio());

            await AssertStatusAsync(HttpStatusCode.Unauthorized, resposta);
        }

        [Theory]
        [InlineData("/api/livros")]
        [InlineData("/api/autores")]
        [InlineData("/api/categorias")]
        [InlineData("/api/editoras")]
        public async Task Cadastro_ComCliente_Retorna403(string rota)
        {
            var cliente = await NovoClienteLogadoAsync();

            var resposta = await cliente.PostAsync(rota, CorpoVazio());

            await AssertStatusAsync(HttpStatusCode.Forbidden, resposta);
        }

        [Fact]
        public async Task Estoques_SoOAdministradorAcessa()
        {
            var anonimo = Fixture.CriarCliente();
            var cliente = await NovoClienteLogadoAsync();
            var admin = await LogarComoAdminAsync();

            await AssertStatusAsync(HttpStatusCode.Unauthorized, await anonimo.GetAsync("/api/estoques"));
            await AssertStatusAsync(HttpStatusCode.Forbidden, await cliente.GetAsync("/api/estoques"));
            await AssertStatusAsync(HttpStatusCode.OK, await admin.GetAsync("/api/estoques"));
        }

        [Fact]
        public async Task Autor_CicloCompleto_CriaLeAtualizaEExclui()
        {
            var admin = await LogarComoAdminAsync();
            var anonimo = Fixture.CriarCliente();

            var criado = await admin.PostAsJsonAsync("/api/autores",
                new CreateAutorDto { Nome = Unico("Autor"), Nacionalidade = "Brasileira", Biografia = "Bio" });
            await AssertStatusAsync(HttpStatusCode.Created, criado);
            var id = (await criado.Content.ReadFromJsonAsync<AutorDto>())!.Id;

            await AssertStatusAsync(HttpStatusCode.OK, await anonimo.GetAsync($"/api/autores/{id}"));

            var atualizado = await admin.PutAsJsonAsync($"/api/autores/{id}",
                new UpdateAutorDto { Nome = Unico("Autor"), Nacionalidade = "Portuguesa", Biografia = "Nova bio" });
            await AssertStatusAsync(HttpStatusCode.OK, atualizado);

            await AssertStatusAsync(HttpStatusCode.NoContent, await admin.DeleteAsync($"/api/autores/{id}"));
            await AssertStatusAsync(HttpStatusCode.NotFound, await anonimo.GetAsync($"/api/autores/{id}"));
        }

        [Fact]
        public async Task Categoria_GeneroDuplicado_Retorna409_InclusiveEmOutraCapitalizacao()
        {
            var admin = await LogarComoAdminAsync();
            var genero = Unico("Genero");

            await AssertStatusAsync(HttpStatusCode.Created,
                await admin.PostAsJsonAsync("/api/categorias", new CreateCategoriaDto { Genero = genero }));

            await AssertStatusAsync(HttpStatusCode.Conflict,
                await admin.PostAsJsonAsync("/api/categorias", new CreateCategoriaDto { Genero = genero }));
            await AssertStatusAsync(HttpStatusCode.Conflict,
                await admin.PostAsJsonAsync("/api/categorias", new CreateCategoriaDto { Genero = genero.ToUpperInvariant() }));
        }

        [Fact]
        public async Task Categoria_AtualizarComOMesmoGenero_Retorna200()
        {
            var admin = await LogarComoAdminAsync();
            var genero = Unico("Genero");
            var criada = await admin.PostAsJsonAsync("/api/categorias", new CreateCategoriaDto { Genero = genero });
            await AssertStatusAsync(HttpStatusCode.Created, criada);
            var id = (await criada.Content.ReadFromJsonAsync<CategoriaDto>())!.Id;

            var resposta = await admin.PutAsJsonAsync($"/api/categorias/{id}", new UpdateCategoriaDto { Genero = genero });

            await AssertStatusAsync(HttpStatusCode.OK, resposta);
        }

        [Fact]
        public async Task Editora_NomeDuplicado_Retorna409_EAtualizarComOMesmoNomeRetorna200()
        {
            var admin = await LogarComoAdminAsync();
            var nome = Unico("Editora");

            var criada = await admin.PostAsJsonAsync("/api/editoras", new CreateEditoraDto { Nome = nome });
            await AssertStatusAsync(HttpStatusCode.Created, criada);
            var id = (await criada.Content.ReadFromJsonAsync<EditoraDto>())!.Id;

            await AssertStatusAsync(HttpStatusCode.Conflict,
                await admin.PostAsJsonAsync("/api/editoras", new CreateEditoraDto { Nome = nome }));

            await AssertStatusAsync(HttpStatusCode.OK,
                await admin.PutAsJsonAsync($"/api/editoras/{id}", new UpdateEditoraDto { Nome = nome }));
        }

        [Fact]
        public async Task Excluir_ComLivrosVinculados_Retorna409()
        {
            var admin = await LogarComoAdminAsync();
            var editoraId = await CriarEditoraAsync(admin);
            var autorId = await CriarAutorAsync(admin);
            var categoriaId = await CriarCategoriaAsync(admin);
            await CriarLivroAsync(admin, NovoLivroDto(editoraId, autorId, categoriaId));

            await AssertStatusAsync(HttpStatusCode.Conflict, await admin.DeleteAsync($"/api/editoras/{editoraId}"));
            await AssertStatusAsync(HttpStatusCode.Conflict, await admin.DeleteAsync($"/api/autores/{autorId}"));
            await AssertStatusAsync(HttpStatusCode.Conflict, await admin.DeleteAsync($"/api/categorias/{categoriaId}"));
        }

        [Fact]
        public async Task Livro_Valido_Retorna201ETemEstoqueComAQuantidadeInicial()
        {
            var admin = await LogarComoAdminAsync();
            var dto = NovoLivroDto(await CriarEditoraAsync(admin), await CriarAutorAsync(admin));
            dto.QuantidadeInicial = 7;

            var criado = await admin.PostAsJsonAsync("/api/livros", dto);
            await AssertStatusAsync(HttpStatusCode.Created, criado);
            var livroId = (await criado.Content.ReadFromJsonAsync<LivroDto>())!.Id;

            var estoque = await LerJsonAsync(await admin.GetAsync($"/api/estoques/livro/{livroId}"));

            Assert.Equal(7, estoque.GetProperty("quantidade").GetInt32());
        }

        [Fact]
        public async Task Livro_IsbnDuplicado_Retorna409()
        {
            var admin = await LogarComoAdminAsync();
            var editoraId = await CriarEditoraAsync(admin);
            var autorId = await CriarAutorAsync(admin);
            var isbn = NovoIsbn();
            await CriarLivroAsync(admin, NovoLivroDto(editoraId, autorId, isbn: isbn));

            var resposta = await admin.PostAsJsonAsync("/api/livros", NovoLivroDto(editoraId, autorId, isbn: isbn));

            await AssertStatusAsync(HttpStatusCode.Conflict, resposta);
        }

        [Fact]
        public async Task Livro_DadosInvalidos_Retornam400()
        {
            var admin = await LogarComoAdminAsync();
            var editoraId = await CriarEditoraAsync(admin);
            var autorId = await CriarAutorAsync(admin);

            var casos = new Dictionary<string, Action<CreateLivroDto>>
            {
                ["ISBN curto"] = d => d.Isbn = "123",
                ["ISBN com letras"] = d => d.Isbn = "ABCDEFGHIJKLM",
                ["ISBN com hífens"] = d => d.Isbn = "978-85-3251-101-0",
                ["preço zero"] = d => d.Preco = 0m,
                ["preço acima do limite"] = d => d.Preco = 10001m,
                ["sem idioma"] = d => d.Idioma = "",
                ["sem nome"] = d => d.Nome = "",
                ["sem autores"] = d => d.AutoresIds = new List<int>(),
                ["editora com id zero"] = d => d.EditoraId = 0,
                ["estoque inicial negativo"] = d => d.QuantidadeInicial = -1
            };

            foreach (var (descricao, ajustar) in casos)
            {
                var dto = NovoLivroDto(editoraId, autorId);
                ajustar(dto);

                var resposta = await admin.PostAsJsonAsync("/api/livros", dto);

                Assert.True(resposta.StatusCode == HttpStatusCode.BadRequest,
                    $"caso '{descricao}' devolveu {(int)resposta.StatusCode} em vez de 400");
            }
        }

        [Fact]
        public async Task Livro_ComEditoraOuAutorInexistente_Retorna400()
        {
            var admin = await LogarComoAdminAsync();
            var editoraId = await CriarEditoraAsync(admin);
            var autorId = await CriarAutorAsync(admin);

            var editoraInexistente = await admin.PostAsJsonAsync("/api/livros", NovoLivroDto(999999, autorId));
            var autorInexistente = await admin.PostAsJsonAsync("/api/livros", NovoLivroDto(editoraId, 999999));

            await AssertStatusAsync(HttpStatusCode.BadRequest, editoraInexistente); 
            await AssertStatusAsync(HttpStatusCode.BadRequest, autorInexistente);
        }

        [Fact]
        public async Task Livro_Inexistente_Retorna404()
        {
            var resposta = await Fixture.CriarCliente().GetAsync("/api/livros/999999");

            await AssertStatusAsync(HttpStatusCode.NotFound, resposta);
        }

        [Fact]
        public async Task Livro_AtualizarMantendoOProprioIsbn_Retorna200ENovoPreco()
        {
            var admin = await LogarComoAdminAsync();
            var editoraId = await CriarEditoraAsync(admin);
            var autorId = await CriarAutorAsync(admin);
            var isbn = NovoIsbn();
            var livroId = await CriarLivroAsync(admin, NovoLivroDto(editoraId, autorId, isbn: isbn));

            var resposta = await admin.PutAsJsonAsync($"/api/livros/{livroId}", NovoUpdateDto(editoraId, autorId, isbn, preco: 45m));
            await AssertStatusAsync(HttpStatusCode.OK, resposta);

            var gravado = await LerJsonAsync(await Fixture.CriarCliente().GetAsync($"/api/livros/{livroId}"));
            Assert.Equal(45m, gravado.GetProperty("preco").GetDecimal());
        }

        [Fact]
        public async Task Livro_AtualizarComIsbnDeOutroLivro_Retorna409()
        {
            var admin = await LogarComoAdminAsync();
            var editoraId = await CriarEditoraAsync(admin);
            var autorId = await CriarAutorAsync(admin);
            var isbnDoOutro = NovoIsbn();
            await CriarLivroAsync(admin, NovoLivroDto(editoraId, autorId, isbn: isbnDoOutro));
            var livroId = await CriarLivroAsync(admin, NovoLivroDto(editoraId, autorId));

            var resposta = await admin.PutAsJsonAsync($"/api/livros/{livroId}", NovoUpdateDto(editoraId, autorId, isbnDoOutro));

            await AssertStatusAsync(HttpStatusCode.Conflict, resposta);
        }

        [Fact]
        public async Task Livro_ComPedido_NaoPodeSerExcluido_SemPedidoPode()
        {
            var admin = await LogarComoAdminAsync();
            var cliente = await NovoClienteLogadoAsync();
            var livroComPedido = await Fixture.CriarLivroAsync(estoque: 10);
            var livroLivre = await Fixture.CriarLivroAsync(estoque: 10);
            await ComprarAsync(cliente, livroComPedido, 1);

            await AssertStatusAsync(HttpStatusCode.Conflict, await admin.DeleteAsync($"/api/livros/{livroComPedido}"));
            await AssertStatusAsync(HttpStatusCode.NoContent, await admin.DeleteAsync($"/api/livros/{livroLivre}"));
        }

        [Fact]
        public async Task Livros_FiltrarPorNome_DevolveSoOsQueCasam()
        {
            var admin = await LogarComoAdminAsync();
            var editoraId = await CriarEditoraAsync(admin);
            var autorId = await CriarAutorAsync(admin);
            var nome = Unico("Filtro");
            await CriarLivroAsync(admin, NovoLivroDto(editoraId, autorId, nome: nome));
            await CriarLivroAsync(admin, NovoLivroDto(editoraId, autorId));

            var pagina = await LerJsonAsync(await Fixture.CriarCliente().GetAsync($"/api/livros?nome={Uri.EscapeDataString(nome)}"));

            Assert.Equal(1, pagina.GetProperty("itens").GetArrayLength());
        }

        [Fact]
        public async Task Estoque_AtualizarQuantidade_PersisteOValor()
        {
            var admin = await LogarComoAdminAsync();
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);
            var estoque = await LerJsonAsync(await admin.GetAsync($"/api/estoques/livro/{livroId}"));
            var estoqueId = estoque.GetProperty("id").GetInt32();

            var resposta = await admin.PutAsJsonAsync($"/api/estoques/{estoqueId}", new UpdateEstoqueDto { Quantidade = 25 });
            await AssertStatusAsync(HttpStatusCode.OK, resposta);

            Assert.Equal(25, await Fixture.ObterEstoqueAsync(livroId));
        }

        [Fact]
        public async Task Estoque_QuantidadeNegativa_Retorna400()
        {
            var admin = await LogarComoAdminAsync();
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);
            var estoque = await LerJsonAsync(await admin.GetAsync($"/api/estoques/livro/{livroId}"));

            var resposta = await admin.PutAsJsonAsync($"/api/estoques/{estoque.GetProperty("id").GetInt32()}",
                new UpdateEstoqueDto { Quantidade = -1 });

            await AssertStatusAsync(HttpStatusCode.BadRequest, resposta);
            Assert.Equal(10, await Fixture.ObterEstoqueAsync(livroId));
        }

        [Fact]
        public async Task Estoque_Inexistente_Retorna404()
        {
            var admin = await LogarComoAdminAsync();

            var resposta = await admin.PutAsJsonAsync("/api/estoques/999999", new UpdateEstoqueDto { Quantidade = 5 });

            await AssertStatusAsync(HttpStatusCode.NotFound, resposta);
        }
    }
}