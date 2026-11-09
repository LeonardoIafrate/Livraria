using System.Net;
using System.Net.Http.Json;
using api.Dtos.Endereco;
using api.Dtos.Pedido;
using Xunit;

namespace api.Tests.Integracao
{
    [Collection("Api")]
    public class PedidosApiTests : ApiTestBase
    {
        public PedidosApiTests(ApiFixture fixture): base(fixture)
        {
        }

        [Fact]
        public async Task SemToken_AsRotasRetornam401()
        {
            var cliente = Fixture.CriarCliente();

            Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync("/api/pedidos/meus-pedidos")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.PostAsJsonAsync("/api/pedidos", NovoPedido(1, (1,1)))).StatusCode);
        }

        [Fact]
        public async Task Cliente_NaoListaTodosPedidos_Retorna403()
        {
            var cliente = await NovoClienteLogadoAsync();

            var resposta = await cliente.GetAsync("/api/pedidos");

            Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        }

        [Fact]
        public async Task Administrador_ListaTodosPedidos_Retorna200()
        {
            var admin = await LogarComoAdminAsync();

            var resposta = await admin.GetAsync("/api/pedidos");

            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        }

        [Fact]
        public async Task Comprar_CalculaOTotal_UsaOPrecoDoLivro_EDescontaOEstoque()
        {
            var livroId = await Fixture.CriarLivroAsync(preco: 30m, estoque: 10);
            var antes = await Fixture.ObterEstoqueAsync(livroId);
            var cliente = await NovoClienteLogadoAsync();

            var pedido = await ComprarAsync(cliente, livroId, quantidade: 2);

            Assert.Equal("Pendente", pedido.Status);
            Assert.Equal(60m, pedido.ValorTotal);
            Assert.Contains("Rua das Flores", pedido.Endereco);

            var item = Assert.Single(pedido.Itens);
            Assert.Equal(livroId, item.LivroId);
            Assert.Equal(2, item.Quantidade);
            Assert.Equal(30m, item.PrecoUnitario);

            var depois = await Fixture.ObterEstoqueAsync(livroId);
            Assert.True(depois == 8,
                $"estoque antes={antes}, depois={depois}, quantidade no pedido={pedido.Itens.Sum(i => i.Quantidade)}");

            Assert.Equal(8, await Fixture.ObterEstoqueAsync(livroId));
        }

        [Fact]
        public async Task Comprar_OMesmoLivroDuasVezesNaLista_SomaAsQuantidades()
        {
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);
            var cliente = await NovoClienteLogadoAsync();
            var enderecoId = await ObterIdDoPrimeiroEnderecoAsync(cliente);

            var resposta = await cliente.PostAsJsonAsync("/api/pedidos", NovoPedido(enderecoId, (livroId, 1), (livroId, 2)));

            Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
            var pedido = await resposta.Content.ReadFromJsonAsync<PedidoDto>();
            Assert.Equal(3, Assert.Single(pedido!.Itens).Quantidade);
            Assert.Equal(7, await Fixture.ObterEstoqueAsync(livroId));
        }

        [Fact]
        public async Task Comprar_ComEstoqueInsuficiente_Retorna409ENaoAlteraEstoque()
        {
            var livroId = await Fixture.CriarLivroAsync(estoque: 1);
            var cliente = await NovoClienteLogadoAsync();
            var enderecoId = await ObterIdDoPrimeiroEnderecoAsync(cliente);

            var resposta = await cliente.PostAsJsonAsync("/api/pedidos", NovoPedido(enderecoId, (livroId, 2)));

            Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
            Assert.Equal(1, await Fixture.ObterEstoqueAsync(livroId));
        }

        [Fact]
        public async Task Comprar_ComLivroInexistente_Retorna400()
        {
            var cliente = await NovoClienteLogadoAsync();
            var enderecoId = await ObterIdDoPrimeiroEnderecoAsync(cliente);

            var resposta = await cliente.PostAsJsonAsync("/api/pedidos", NovoPedido(enderecoId, (9999, 1)));

            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        [Fact]
        public async Task Comprar_SemItens_Retorna400()
        {
            var cliente = await NovoClienteLogadoAsync();
            var enderecoId = await ObterIdDoPrimeiroEnderecoAsync(cliente);

            var resposta = await cliente.PostAsJsonAsync("/api/pedidos", NovoPedido(enderecoId));

            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        [Fact]
        public async Task Comprar_ComEnderecoDeOutroUsuario_Retorna400ENaoAlteraEstoque()
        {
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);
            var clienteA = await NovoClienteLogadoAsync();
            var clienteB = await NovoClienteLogadoAsync();
            var enderecoDeB = await ObterIdDoPrimeiroEnderecoAsync(clienteB);

            var resposta = await clienteA.PostAsJsonAsync("/api/pedidos", NovoPedido(enderecoDeB, (livroId, 1)));

            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
            Assert.Equal(10, await Fixture.ObterEstoqueAsync(livroId));
        }

        [Fact]
        public async Task MeusPedidos_ListaTodosOsPedidosDoProprioUsuario()
        {
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);
            var clienteA = await NovoClienteLogadoAsync();
            var clienteB = await NovoClienteLogadoAsync();
            var pedidoA = await ComprarAsync(clienteA, livroId, 1);
            await ComprarAsync(clienteB, livroId, 1);

            var pedidosDeA = await clienteA.GetFromJsonAsync<List<PedidoDto>>("/api/pedidos/meus-pedidos");

            Assert.Equal(pedidoA.Id, Assert.Single(pedidosDeA!).Id);
        }

        [Fact]
        public async Task UsuarioB_NaoEnxergaNemAlteraPedidosDeA()
        {
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);
            var clienteA = await NovoClienteLogadoAsync();
            var clienteB = await NovoClienteLogadoAsync();
            var pedido = await ComprarAsync(clienteA, livroId, 1);
            var enderecoDeB = await ObterIdDoPrimeiroEnderecoAsync(clienteB);

            Assert.Equal(HttpStatusCode.NotFound, (await clienteB.GetAsync($"/api/pedidos/{pedido.Id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await clienteB.PatchAsync($"/api/pedidos/{pedido.Id}/cancelar", new StringContent(""))).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await clienteB.PatchAsync($"/api/pedidos/{pedido.Id}/endereco",
                                                             JsonContent.Create(new AlterarEnderecoPedidoDto {EnderecoId = enderecoDeB}))).StatusCode);
            

            var doBanco = await clienteA.GetFromJsonAsync<PedidoDto>($"/api/pedidos/{pedido.Id}");
            Assert.Equal("Pendente", doBanco!.Status);
            Assert.Equal(9, await Fixture.ObterEstoqueAsync(livroId));
        }

        [Fact]
        public async Task Administrador_EnxergaOPedidoDeQualquerUsuario()
        {
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);
            var admin = await LogarComoAdminAsync();
            var cliente = await NovoClienteLogadoAsync();
            var pedido = await ComprarAsync(cliente ,livroId, 1);
            
            var resposta = await admin.GetAsync($"/api/pedidos/{pedido.Id}");

            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        }

        [Fact]
        public async Task Cancelar_DevolveAoEstoque_ENaoDevolveDuasVezes()
        {
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);
            var cliente = await NovoClienteLogadoAsync();
            var pedido = await ComprarAsync(cliente, livroId, quantidade: 2);
            Assert.Equal(8, await Fixture.ObterEstoqueAsync(livroId));

            var primeira = await cliente.PatchAsync($"/api/pedidos/{pedido.Id}/cancelar", new StringContent(""));
            Assert.Equal(HttpStatusCode.OK, primeira.StatusCode);

            var cancelado = await primeira.Content.ReadFromJsonAsync<PedidoDto>();
            Assert.Equal("Cancelado", cancelado!.Status);
            Assert.Equal(10, await Fixture.ObterEstoqueAsync(livroId));

            var segunda = await cliente.PatchAsync($"/api/pedidos/{pedido.Id}/cancelar", new StringContent(""));
            Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
            Assert.Equal(10, await Fixture.ObterEstoqueAsync(livroId));
        }

        [Fact]
        public async Task TrocarEnderecoDoPedido_AntesDoEnvio_Funciona()
        {
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);
            var cliente = await NovoClienteLogadoAsync();
            var pedido = await ComprarAsync(cliente, livroId, quantidade: 1);

            var criado = await cliente.PostAsJsonAsync("/api/enderecos", new CreateEnderecoDto
            {
                Cep = "01310100", Rua = "Avenida Paulista", Numero = "1000",
                Bairro = "Bela Vista", Cidade = "São Paulo", Uf = "SP"
            });

            await GarantirSucessoAsync(criado);

            var novoEndereco = await criado.Content.ReadFromJsonAsync<EnderecoDto>();
            if (!criado.IsSuccessStatusCode)
                Assert.Fail($"POST devolveu {(int)criado.StatusCode}: {await criado.Content.ReadAsStringAsync()}");

            var resposta = await cliente.PatchAsync($"/api/pedidos/{pedido.Id}/endereco", JsonContent.Create(new AlterarEnderecoPedidoDto {EnderecoId = novoEndereco!.Id}));

            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            var atualizado = await resposta.Content.ReadFromJsonAsync<PedidoDto>();
            Assert.Contains("Avenida Paulista", atualizado!.Endereco);
        }

        [Fact]
        public async Task EditarOEnderecoDoUsuario_NaoAlteraOEnderecoDoPedidoJaFeito()
        {
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);
            var cliente = await NovoClienteLogadoAsync();
            var enderecoId = await ObterIdDoPrimeiroEnderecoAsync(cliente);
            var pedido = await ComprarAsync(cliente, livroId, quantidade: 1);
            
            var edicao = await cliente.PutAsJsonAsync($"/api/enderecos/{enderecoId}", new UpdateEnderecoDto
            {
                Cep = "01310100", Rua = "Rua Nova", Numero = "50",
                Bairro = "Jardim", Cidade = "São Paulo", Uf = "SP"
            });
            await GarantirSucessoAsync(edicao);

            var doBanco = await cliente.GetFromJsonAsync<PedidoDto>($"/api/pedidos/{pedido.Id}");
            Assert.Contains("Rua das Flores", doBanco!.Endereco);
            Assert.DoesNotContain("Rua Nova", doBanco!.Endereco);
        }

        [Fact]
        public async Task ExcluirOEnderecoDoUsuario_NaoApagaNemAlteraOPedido()
        {
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);
            var cliente = await NovoClienteLogadoAsync();
            var enderecoId = await ObterIdDoPrimeiroEnderecoAsync(cliente);
            var pedido = await ComprarAsync(cliente, livroId, 1);

            var exclusao = await cliente.DeleteAsync($"/api/enderecos/{enderecoId}");

            await AssertStatusAsync(HttpStatusCode.NoContent, exclusao);

            var doBanco = await cliente.GetFromJsonAsync<PedidoDto>($"/api/pedidos/{pedido.Id}");
            Assert.Contains("Rua das Flores", doBanco!.Endereco);
        }
    }
}