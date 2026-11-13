using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using api.Dtos.Endereco;
using Xunit;

namespace api.Tests.Integracao
{
    [Collection("Api")]
    public class EnderecoApiTests : ApiTestBase
    {
        public EnderecoApiTests(ApiFixture fixture) : base(fixture)
        {
        }

        private static CreateEnderecoDto NovoEndereco(string rua = "Avenida Paulista") => new()
        {
            Cep = "01310100",
            Rua = rua,
            Numero = "1000",
            Bairro = "Bela Vista",
            Cidade = "São Paulo",
            Uf = "SP"
        };

        private static async Task<int> CriarAsync(HttpClient cliente, string rua = "Avenida Paulista")
        {
            var resposta = await cliente.PostAsJsonAsync("/api/enderecos", NovoEndereco(rua));
            await AssertStatusAsync(HttpStatusCode.Created, resposta);
            return (await resposta.Content.ReadFromJsonAsync<EnderecoDto>())!.Id;
        }

        private static async Task<List<JsonElement>> ListarAsync(HttpClient cliente)
        {
            var resposta = await cliente.GetAsync("/api/enderecos");
            await GarantirSucessoAsync(resposta);
            using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
            return documento.RootElement.EnumerateArray().Select(e => e.Clone()).ToList();
        }

        [Fact]
        public async Task Criar_Retorna201_EAparecePraListaDoUsuario()
        {
            var cliente = await NovoClienteLogadoAsync();

            var id = await CriarAsync(cliente);

            var enderecos = await ListarAsync(cliente);
            Assert.Equal(2, enderecos.Count);
            Assert.Contains(enderecos, e => e.GetProperty("id").GetInt32() == id);
        }

        [Fact]
        public async Task Criar_SemRua_Retorna400()
        {
            var cliente = await NovoClienteLogadoAsync();
            var dto = NovoEndereco();
            dto.Rua = "";

            var resposta = await cliente.PostAsJsonAsync("/api/enderecos", dto);

            await AssertStatusAsync(HttpStatusCode.BadRequest, resposta);
        }

        [Fact]
        public async Task DefinirPrincipal_TrocaOPrincipal_MantendoUmSo()
        {
            var cliente = await NovoClienteLogadoAsync();
            var novoId = await CriarAsync(cliente, "Rua B");

            var resposta = await cliente.PutAsync($"/api/enderecos/{novoId}/principal", new StringContent(""));
            await AssertStatusAsync(HttpStatusCode.OK, resposta);

            var principais = (await ListarAsync(cliente))
                .Where(e => e.GetProperty("principal").GetBoolean())
                .ToList();

            Assert.Equal(novoId, Assert.Single(principais).GetProperty("id").GetInt32());
        }

        [Fact]
        public async Task Atualizar_MudaOsDadosDoEndereco()
        {
            var cliente = await NovoClienteLogadoAsync();
            var id = await CriarAsync(cliente);

            var resposta = await cliente.PutAsJsonAsync($"/api/enderecos/{id}", new UpdateEnderecoDto
            {
                Cep = "01310100", Rua = "Rua Nova", Numero = "50",
                Bairro = "Jardim", Cidade = "São Paulo", Uf = "SP"
            });
            await AssertStatusAsync(HttpStatusCode.OK, resposta);

            var gravado = await cliente.GetFromJsonAsync<JsonElement>($"/api/enderecos/{id}");
            Assert.Equal("Rua Nova", gravado.GetProperty("rua").GetString());
        }

        [Fact]
        public async Task Atualizar_Inexistente_Retorna404()
        {
            var cliente = await NovoClienteLogadoAsync();

            var resposta = await cliente.PutAsJsonAsync("/api/enderecos/999999", new UpdateEnderecoDto
            {
                Cep = "01310100", Rua = "Rua", Numero = "1",
                Bairro = "Centro", Cidade = "São Paulo", Uf = "SP"
            });

            await AssertStatusAsync(HttpStatusCode.NotFound, resposta);
        }

        [Fact]
        public async Task Excluir_Retorna204_EOEnderecoSaiDaLista()
        {
            var cliente = await NovoClienteLogadoAsync();
            var id = await CriarAsync(cliente);

            await AssertStatusAsync(HttpStatusCode.NoContent, await cliente.DeleteAsync($"/api/enderecos/{id}"));

            var enderecos = await ListarAsync(cliente);
            Assert.DoesNotContain(enderecos, e => e.GetProperty("id").GetInt32() == id);
        }
    }
}