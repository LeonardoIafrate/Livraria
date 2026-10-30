using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using api.Dtos.Auth;
using api.Dtos.Endereco;
using api.Dtos.Pedido;
using api.Dtos.Usuario;
using api.Models;

namespace api.Tests.Integracao
{
    public class ApiTestBase
    {
        protected ApiFixture Fixture { get; }

        protected ApiTestBase(ApiFixture fixture) => Fixture = fixture;

        protected static CreateUsuarioDto NovoCadastro(string? email = null, string? cpf = null) => new()
        {
          Nome = "Cliente de Teste",
          Email = email ?? $"cliente.{Guid.NewGuid():N}@teste.com",
          Cpf = cpf ?? CpfDeTeste.Novo(),
          Senha = "Senha12345",
          DataNascimento = new DateTime(1900,1,1),
          Endereco = new EnderecoBaseDto
          {
              Cep = "09010100",
              Rua = "Rua das Flores",
              Numero = "123",
              Bairro = "Centro",
              Cidade = "Santo André",
              Uf = "SP"
          }  
        };

        protected async Task<(string Email, string Senha)> CadastrarClienteAsync()
        {
            var dto = NovoCadastro();

            var resposta = await Fixture.CriarCliente().PostAsJsonAsync("/api/usuarios", dto);
            resposta.EnsureSuccessStatusCode();

            return (dto.Email, dto.Senha);
        }

        protected async Task<HttpClient> LogarAsync(string email, string senha)
        {
            var cliente = Fixture.CriarCliente();

            var resposta = await cliente.PostAsJsonAsync("/api/auth/login", new LoginDto { Email = email, Senha = senha });

            if (!resposta.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Login de '{email}' falhou: {(int)resposta.StatusCode} - {await resposta.Content.ReadAsStringAsync()}");
            }
            
            resposta.EnsureSuccessStatusCode();

            var token = await resposta.Content.ReadFromJsonAsync<TokenDto>();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.Token);

            return cliente;
        }

        protected Task<HttpClient> LogarComoAdminAsync() => LogarAsync(ApiFixture.EmailAdmin, ApiFixture.SenhaAdmin);
    
        protected async Task<HttpClient> NovoClienteLogadoAsync()
        {
            var (email, senha) = await CadastrarClienteAsync();
            return await LogarAsync(email, senha);
        }

        protected static CreatePedidoDto NovoPedido(int enderecoId, params (int LivroId, int Quantidade)[] itens)
        {
            var dto = new CreatePedidoDto { EnderecoId = enderecoId };
            
            foreach (var (livroId, quantidade) in itens)
                dto.Itens.Add(new CreateItemPedidoDto { LivroId = livroId, Quantidade = quantidade });
            
            return dto;
        }

        protected static async Task<int> ObterIdDoPrimeiroEnderecoAsync(HttpClient cliente)
        {
            var enderecos = await cliente.GetFromJsonAsync<List<EnderecoDto>>("/api/enderecos");
            return enderecos![0].Id;
        }

        protected static async Task<PedidoDto> ComprarAsync(HttpClient cliente, int livroId, int quantidade)
        {
            var enderecoId = await ObterIdDoPrimeiroEnderecoAsync(cliente);

            var resposta = await cliente.PostAsJsonAsync("/api/pedidos", NovoPedido(enderecoId, (livroId, quantidade)));
            resposta.EnsureSuccessStatusCode();

            return (await resposta.Content.ReadFromJsonAsync<PedidoDto>())!;
        }

        protected static async Task GarantirSucessoAsync(HttpResponseMessage resposta)
        {
            if(!resposta.IsSuccessStatusCode)
                Assert.Fail(await DescreverFalhaAsync(resposta));
        }

        protected static async Task AssertStatusAsync(HttpStatusCode esperado, HttpResponseMessage resposta)
        {
            if(resposta.StatusCode !=esperado)
                Assert.Fail($"Esperado {(int)esperado} {esperado}." + await DescreverFalhaAsync(resposta));
        }

        private static async Task<string> DescreverFalhaAsync(HttpResponseMessage resposta)
        {
            var requisicao = resposta.RequestMessage;
            var corpo = await resposta.Content.ReadAsStringAsync();

            return $"{requisicao?.Method} {requisicao?.RequestUri?.PathAndQuery} devolveu" +
                   $"{(int)resposta.StatusCode} {resposta.StatusCode}. Corpo: {(string.IsNullOrWhiteSpace(corpo) ? "(vazio)" : corpo)}";
        }
    }
}