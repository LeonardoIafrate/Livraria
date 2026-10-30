using System.Net;
using System.Net.Http.Json;
using System.Text;
using api.Dtos.Auth;
using Xunit;

namespace api.Tests.Integracao;

[Collection("Api")]
public class CadastroDeUsuarioApiTests : ApiTestBase
{
    public CadastroDeUsuarioApiTests(ApiFixture fixture) : base(fixture)
    {
    }

    private Task<HttpResponseMessage> CadastrarAsync(object dto)
        => Fixture.CriarCliente().PostAsJsonAsync("/api/usuarios", dto);

    [Fact]
    public async Task Cadastro_SemToken_Retorna201()
    {
        var resposta = await CadastrarAsync(NovoCadastro());

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact]
    public async Task Cadastro_ComCpfJaCadastrado_Retorna409()
    {
        var cpf = CpfDeTeste.Novo();
        await CadastrarAsync(NovoCadastro(cpf: cpf));

        var resposta = await CadastrarAsync(NovoCadastro(cpf: cpf));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Cadastro_ComEmailJaCadastradoEmOutraCapitalizacao_Retorna409()
    {
        var email = $"Duplicado.{Guid.NewGuid():N}@Teste.com";
        await CadastrarAsync(NovoCadastro(email: email));

        var resposta = await CadastrarAsync(NovoCadastro(email: email.ToLowerInvariant()));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task Cadastro_ComCpfInvalido_Retorna400()
    {
        var resposta = await CadastrarAsync(NovoCadastro(cpf: "111.111.111-11"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Cadastro_ComSenhaFraca_Retorna400()
    {
        var dto = NovoCadastro();
        dto.Senha = "12345678";

        var resposta = await CadastrarAsync(dto);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Cadastro_SemEndereco_Retorna400()
    {
        var dto = NovoCadastro();
        dto.Endereco = null!;

        var resposta = await CadastrarAsync(dto);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Cadastro_ComDataEmFormatoInvalido_Retorna400()
    {
        var json = $$"""
        {
          "nome": "Ana",
          "email": "ana.{{Guid.NewGuid():N}}@teste.com",
          "cpf": "{{CpfDeTeste.Novo()}}",
          "senha": "Senha12345",
          "dataNascimento": "2005-26-26",
          "endereco": { "cep": "09010100", "rua": "Rua A", "numero": "1", "bairro": "Centro", "cidade": "Santo André", "uf": "SP" }
        }
        """;

        var resposta = await Fixture.CriarCliente()
            .PostAsync("/api/usuarios", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Cadastro_ComEmailEmMaiusculas_PermiteLoginComOEmailEmMinusculas()
    {
        var email = $"Maiusculas.{Guid.NewGuid():N}@Teste.COM";
        await CadastrarAsync(NovoCadastro(email: email));

        var resposta = await Fixture.CriarCliente().PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = email.ToLowerInvariant(), Senha = "Senha12345" });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }
}