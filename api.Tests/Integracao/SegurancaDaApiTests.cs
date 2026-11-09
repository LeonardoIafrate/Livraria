using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using api.Dtos.Auth;
using api.Dtos.Endereco;
using api.Helpers;
using Xunit;

namespace api.Tests.Integracao
{

    [Collection("Api")]
    public class SegurancaDaApiTests : ApiTestBase
    {

        public SegurancaDaApiTests(ApiFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task SemToken_RotaProtegida_Retorna401()
        {
            var resposta = await Fixture.CriarCliente().GetAsync("/api/enderecos");

            Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        }

        [Fact]
        public async Task TokenInvalido_Retorna401()
        {
            var cliente = Fixture.CriarCliente();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token.invalido.qualquer");

            var resposta = await cliente.GetAsync("/api/enderecos");

            Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        }

        [Fact]
        public async Task Login_ComCredenciaisIncorretas_RetornaToken()
        {
            var (email, senha) = await CadastrarClienteAsync();

            var resposta = await Fixture.CriarCliente().PostAsJsonAsync("/api/auth/login", new LoginDto { Email = email, Senha = senha});

            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            var token = await resposta.Content.ReadFromJsonAsync<TokenDto>();
            Assert.False(string.IsNullOrWhiteSpace(token?.Token));
        }

        [Fact]
        public async Task Login_ComSenhaErrada_Retorna401()
        {
            var (email, _) = await CadastrarClienteAsync();

            var resposta = await Fixture
                            .CriarCliente().PostAsJsonAsync("/api/auth/login", new LoginDto { Email = email, Senha = "SenhaErrada111"});
            
            Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        }

        [Fact]
        public async Task Login_EmailInexistenteESenhaErrada_DaoAMesmaMensagem()
        {
            var (email, _) = await CadastrarClienteAsync();
            var cliente = Fixture.CriarCliente();

            var senhaErrada = await cliente
                            .PostAsJsonAsync("/api/auth/login", new LoginDto { Email = email, Senha = "SenhaErrada999"});
            var emailInexistente = await cliente
                            .PostAsJsonAsync("/api/auth/login", new LoginDto {Email = "nao.existe@teste.com", Senha = "SenhaErrada111"});

            Assert.Equal(HttpStatusCode.Unauthorized, senhaErrada.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, emailInexistente.StatusCode);
            Assert.Equal(await LerDetalheAsync(senhaErrada), await LerDetalheAsync(emailInexistente));
        }

        [Fact]
        public async Task Cliente_EmRotaDeAdministrador_Retorna403()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = await LogarAsync(email, senha);
    
            var resposta = await cliente.GetAsync("/api/usuarios");
    
            Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        }
    
        [Fact]
        public async Task Administrador_EmRotaDeAdministrador_Retorna200()
        {
            var admin = await LogarComoAdminAsync();
    
            var resposta = await admin.GetAsync("/api/usuarios");
    
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        }
    
        [Fact]
        public async Task Me_DevolveOsDadosDoProprioUsuarioLogado()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = await LogarAsync(email, senha);
    
            var resposta = await cliente.GetAsync("/api/usuarios/me");
    
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            Assert.Contains(email, await resposta.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task UsuarioA_NaoEnxergaNemAlteraOEnderecoDoUsuarioB()
        {
            var (emailA, senhaA) = await CadastrarClienteAsync();
            var (emailB, senhaB) = await CadastrarClienteAsync();
            var clienteA = await LogarAsync(emailA, senhaA);
            var clienteB = await LogarAsync(emailB, senhaB);

            var enderecoDeB = await clienteB.GetFromJsonAsync<List<EnderecoDto>>("/api/enderecos");
            var idDoEnderecoDeB = Assert.Single(enderecoDeB!).Id;

            var alteracao = new UpdateEnderecoDto
            {
                Cep = "01310100", Rua = "Avenida Paulista", Numero = "1000",
                Bairro = "Bela Vista", Cidade = "São Paulo", Uf = "SP"
            };

            Assert.Equal(HttpStatusCode.NotFound, (await clienteA.GetAsync($"/api/enderecos/{idDoEnderecoDeB}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await clienteA.PutAsJsonAsync($"/api/enderecos/{idDoEnderecoDeB}", alteracao)).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await clienteA.PutAsync($"/api/enderecos/{idDoEnderecoDeB}/principal", new StringContent(""))).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await clienteA.DeleteAsync($"/api/enderecos/{idDoEnderecoDeB}")).StatusCode);

            Assert.Equal(HttpStatusCode.OK, (await clienteB.GetAsync($"/api/enderecos/{idDoEnderecoDeB}")).StatusCode);
        }

        [Fact]
        public async Task Login_AposVariasFalhas_BloqueiaMesmoComSenhaCorreta()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = Fixture.CriarCliente();

            for (var i = 0; i < PoliticaDeLogin.MaxTentativas; i++)
            {
                var falha = await cliente.PostAsJsonAsync("/api/auth/login",
                    new LoginDto { Email = email, Senha = "SenhaErrada999" });

                Assert.Equal(HttpStatusCode.Unauthorized, falha.StatusCode);
            }

            var resposta = await cliente.PostAsJsonAsync("/api/auth/login",
                new LoginDto { Email = email, Senha = senha });

            Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        }

        private static async Task<string?> LerDetalheAsync(HttpResponseMessage resposta)
        {
            using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
            return documento.RootElement.GetProperty("detail").GetString();
        }
    }
}