using System.Net;
using System.Net.Http.Json;
using api.Dtos.Pedido;
using Xunit;

namespace api.Tests.Integracao
{
    [Collection("Api")]
    public class PedidosStatusApiTests : ApiTestBase
    {

        public PedidosStatusApiTests(ApiFixture fixture) : base(fixture)
        {
        }

        private static Task<HttpResponseMessage> MudarStatusAsync(HttpClient cliente, int pedidoId, string status) 
                => cliente.PatchAsync($"/api/pedidos/{pedidoId}/status", JsonContent.Create(status));

        private async Task<(HttpClient Cliente, PedidoDto Pedido, int LivroId)> NovaCompraAsync()
        {
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);
            var cliente = await NovoClienteLogadoAsync();
            var pedido = await ComprarAsync(cliente, livroId, 1);

            return (cliente, pedido, livroId);
        }

        [Fact]
        public async Task Administrador_AvancaOStatusNaOrdemCorreta()
        {
            var (_, pedido, _) = await NovaCompraAsync();
            var admin = await LogarComoAdminAsync();

            foreach(var status in new[] { "Pago", "Enviado", "Entregue" })
            {
                var resposta = await MudarStatusAsync(admin, pedido.Id, status);

                Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
                var atualizado = await resposta.Content.ReadFromJsonAsync<PedidoDto>();
                Assert.Equal(status, atualizado!.Status);
            }
        } 

        [Fact]
        public async Task Administrador_PulaEtapas_Retorna409()
        {
            var (_, pedido, _) = await NovaCompraAsync();
            var admin = await LogarComoAdminAsync();

            var resposta = await MudarStatusAsync(admin, pedido.Id, "Enviado");

            Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        }

        [Fact]
        public async Task Administrador_NaoCancelaPelaBarraDeStatus_Retorna409()
        {
            var (_, pedido, _) = await NovaCompraAsync();
            var admin = await LogarComoAdminAsync();

            var resposta = await MudarStatusAsync(admin, pedido.Id, "Cancelado");
            Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        }

        [Fact]
        public async Task StatusDesconhecido_Retorna400()
        {
            var (_, pedido, _) = await NovaCompraAsync();
            var admin = await LogarComoAdminAsync();

            var resposta = await MudarStatusAsync(admin, pedido.Id, "Voando");

            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        [Fact]
        public async Task Cliente_NaoMudaOsStatus_Retorna403()
        {
            var(cliente, pedido, _) = await NovaCompraAsync();
        
            var resposta = await MudarStatusAsync(cliente, pedido.Id, "Pago");

            Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        }

        [Fact]
        public async Task PedidoEnviado_NaoPodeSerCancelado_NemTrocarEndereco()
        {
            var(cliente, pedido, livroId) = await NovaCompraAsync();
            var admin = await LogarComoAdminAsync();
            await MudarStatusAsync(admin, pedido.Id, "Pago");
            await MudarStatusAsync(admin, pedido.Id, "Enviado");
            var enderecoId = await ObterIdDoPrimeiroEnderecoAsync(cliente);

            var cancelamento = await cliente.PatchAsync($"/api/pedidos/{pedido.Id}/cancelar", new StringContent(""));
            var trocarDeEndereco = await cliente.PatchAsync($"/api/pedidos/{pedido.Id}/endereco",
                JsonContent.Create(new AlterarEnderecoPedidoDto {EnderecoId = enderecoId}));

            Assert.Equal(HttpStatusCode.Conflict, cancelamento.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, trocarDeEndereco.StatusCode);
            Assert.Equal(9, await Fixture.ObterEstoqueAsync(livroId));
        }
    }
}