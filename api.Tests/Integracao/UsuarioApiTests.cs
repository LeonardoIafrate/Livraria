using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using api.Dtos.Auth;
using api.Dtos.Usuario;
using api.Helpers;
using Xunit;

namespace api.Tests.Integracao
{
    [Collection("Api")]
    public class UsuarioApiTests : ApiTestBase
    {
        private const string NovaSenha = "NovaSenha456";

        public UsuarioApiTests(ApiFixture fixture) : base(fixture)
        {
        }

        private static async Task<int> ObterMeuIdAsync(HttpClient cliente)
        {
            var resposta = await cliente.GetAsync("/api/usuarios/me");
            await GarantirSucessoAsync(resposta);

            using var documento = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
            return documento.RootElement.GetProperty("id").GetInt32();
        }

        private Task<HttpResponseMessage> TentarLoginAsync(string email, string senha) =>
            Fixture.CriarCliente().PostAsJsonAsync("/api/auth/login", new LoginDto { Email = email, Senha = senha });

        private static StringContent CorpoVazio() => new("");


        [Fact]
        public async Task RotasDoUsuarioLogado_SemToken_Retornam401()
        {
            var anonimo = Fixture.CriarCliente();
            var json = () => new StringContent("{}", Encoding.UTF8, "application/json");

            await AssertStatusAsync(HttpStatusCode.Unauthorized, await anonimo.GetAsync("/api/usuarios/me"));
            await AssertStatusAsync(HttpStatusCode.Unauthorized, await anonimo.PutAsync("/api/usuarios/me", json()));
            await AssertStatusAsync(HttpStatusCode.Unauthorized, await anonimo.PutAsync("/api/usuarios/me/senha", json()));
            await AssertStatusAsync(HttpStatusCode.Unauthorized, await anonimo.PatchAsync("/api/usuarios/me/desativar", CorpoVazio()));
        }

        [Fact]
        public async Task AlterarSenha_Valida_Retorna204_EANovaSenhaPassaAFuncionar()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = await LogarAsync(email, senha);

            var resposta = await cliente.PutAsJsonAsync("/api/usuarios/me/senha",
                new AlterarSenhaDto { SenhaAtual = senha, NovaSenha = NovaSenha });

            await AssertStatusAsync(HttpStatusCode.NoContent, resposta);
            await AssertStatusAsync(HttpStatusCode.OK, await TentarLoginAsync(email, NovaSenha));
            await AssertStatusAsync(HttpStatusCode.Unauthorized, await TentarLoginAsync(email, senha));
        }

        [Fact]
        public async Task AlterarSenha_SenhaAtualErrada_Retorna400_ESenhaNaoMuda()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = await LogarAsync(email, senha);

            var resposta = await cliente.PutAsJsonAsync("/api/usuarios/me/senha",
                new AlterarSenhaDto { SenhaAtual = "errada123", NovaSenha = NovaSenha });

            await AssertStatusAsync(HttpStatusCode.BadRequest, resposta);
            await AssertStatusAsync(HttpStatusCode.OK, await TentarLoginAsync(email, senha));
        }

        [Fact]
        public async Task AlterarSenha_NovaIgualAAtual_Retorna400()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = await LogarAsync(email, senha);

            var resposta = await cliente.PutAsJsonAsync("/api/usuarios/me/senha",
                new AlterarSenhaDto { SenhaAtual = senha, NovaSenha = senha });

            await AssertStatusAsync(HttpStatusCode.BadRequest, resposta);
        }

        [Fact]
        public async Task AlterarSenha_NovaSenhaFraca_Retorna400()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = await LogarAsync(email, senha);

            var resposta = await cliente.PutAsJsonAsync("/api/usuarios/me/senha",
                new AlterarSenhaDto { SenhaAtual = senha, NovaSenha = "12345678" });

            await AssertStatusAsync(HttpStatusCode.BadRequest, resposta);
        }

        [Fact]
        public async Task AlterarSenha_SenhaAtualErradaVariasVezes_BloqueiaTrocaELogin()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = await LogarAsync(email, senha);

            for (var i = 0; i < PoliticaDeLogin.MaxTentativas; i++)
            {
                var falha = await cliente.PutAsJsonAsync("/api/usuarios/me/senha",
                    new AlterarSenhaDto { SenhaAtual = "errada123", NovaSenha = NovaSenha });

                await AssertStatusAsync(HttpStatusCode.BadRequest, falha);
            }

            var comSenhaCerta = await cliente.PutAsJsonAsync("/api/usuarios/me/senha",
                new AlterarSenhaDto { SenhaAtual = senha, NovaSenha = NovaSenha });

            await AssertStatusAsync(HttpStatusCode.BadRequest, comSenhaCerta);
            await AssertStatusAsync(HttpStatusCode.Unauthorized, await TentarLoginAsync(email, senha));
        }

        [Fact]
        public async Task Atualizar_MeusDados_Retorna200ENovoEmailPassaAValer()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = await LogarAsync(email, senha);
            var novoEmail = $"novo.{Guid.NewGuid():N}@teste.com";

            var resposta = await cliente.PutAsJsonAsync("/api/usuarios/me", new UpdateUsuarioDto
            {
                Nome = "Nome Atualizado",
                Email = novoEmail,
                DataNascimento = new DateTime(1990, 5, 20)
            });

            await AssertStatusAsync(HttpStatusCode.OK, resposta);
            await AssertStatusAsync(HttpStatusCode.OK, await TentarLoginAsync(novoEmail, senha));
        }

        [Fact]
        public async Task Atualizar_MantendoOProprioEmail_Retorna200()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = await LogarAsync(email, senha);

            var resposta = await cliente.PutAsJsonAsync("/api/usuarios/me", new UpdateUsuarioDto
            {
                Nome = "Outro Nome",
                Email = email,
                DataNascimento = new DateTime(1990, 5, 20)
            });

            await AssertStatusAsync(HttpStatusCode.OK, resposta);
        }

        [Fact]
        public async Task Atualizar_ComEmailDeOutroUsuario_Retorna409()
        {
            var (emailDoOutro, _) = await CadastrarClienteAsync();
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = await LogarAsync(email, senha);

            var resposta = await cliente.PutAsJsonAsync("/api/usuarios/me", new UpdateUsuarioDto
            {
                Nome = "Qualquer",
                Email = emailDoOutro,
                DataNascimento = new DateTime(1990, 5, 20)
            });

            await AssertStatusAsync(HttpStatusCode.Conflict, resposta);
        }

        [Fact]
        public async Task Desativar_Me_Retorna204_EOLoginPassaAFalhar()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = await LogarAsync(email, senha);

            await AssertStatusAsync(HttpStatusCode.NoContent, await cliente.PatchAsync("/api/usuarios/me/desativar", CorpoVazio()));

            await AssertStatusAsync(HttpStatusCode.Unauthorized, await TentarLoginAsync(email, senha));
        }

        [Fact]
        public async Task Admin_DesativaOutroUsuario_ESoOAdminPodeFazerIsso()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var alvo = await LogarAsync(email, senha);
            var alvoId = await ObterMeuIdAsync(alvo);
            var outro = await NovoClienteLogadoAsync();
            var admin = await LogarComoAdminAsync();

            await AssertStatusAsync(HttpStatusCode.Forbidden, await outro.PatchAsync($"/api/usuarios/{alvoId}/desativar", CorpoVazio()));
            await AssertStatusAsync(HttpStatusCode.NoContent, await admin.PatchAsync($"/api/usuarios/{alvoId}/desativar", CorpoVazio()));

            await AssertStatusAsync(HttpStatusCode.Unauthorized, await TentarLoginAsync(email, senha));
        }


        [Fact]
        public async Task CriarAdmin_ClienteRecebe403_AdminCria201_ENovoAdminAcessaRotaDeAdmin()
        {
            var cliente = await NovoClienteLogadoAsync();
            var dto = NovoCadastro();

            await AssertStatusAsync(HttpStatusCode.Forbidden, await cliente.PostAsJsonAsync("/api/usuarios/admin", dto));

            var admin = await LogarComoAdminAsync();
            await AssertStatusAsync(HttpStatusCode.Created, await admin.PostAsJsonAsync("/api/usuarios/admin", dto));

            var novoAdmin = await LogarAsync(dto.Email, dto.Senha);
            await AssertStatusAsync(HttpStatusCode.OK, await novoAdmin.GetAsync("/api/usuarios"));
        }

        [Fact]
        public async Task PromoverParaAdmin_SoOAdminFaz_EValeNoProximoLogin()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var alvo = await LogarAsync(email, senha);
            var alvoId = await ObterMeuIdAsync(alvo);
            var outro = await NovoClienteLogadoAsync();
            var admin = await LogarComoAdminAsync();

            await AssertStatusAsync(HttpStatusCode.Forbidden, await outro.PatchAsync($"/api/usuarios/{alvoId}", CorpoVazio()));
            await AssertStatusAsync(HttpStatusCode.OK, await admin.PatchAsync($"/api/usuarios/{alvoId}", CorpoVazio()));

            var alvoRelogado = await LogarAsync(email, senha);
            await AssertStatusAsync(HttpStatusCode.OK, await alvoRelogado.GetAsync("/api/usuarios"));
        }

        [Fact]
        public async Task Admin_BuscaUsuarioPorIdEPorEmail_EInexistenteDa404()
        {
            var (email, senha) = await CadastrarClienteAsync();
            var cliente = await LogarAsync(email, senha);
            var id = await ObterMeuIdAsync(cliente);
            var admin = await LogarComoAdminAsync();

            await AssertStatusAsync(HttpStatusCode.OK, await admin.GetAsync($"/api/usuarios/{id}"));
            await AssertStatusAsync(HttpStatusCode.OK, await admin.GetAsync($"/api/usuarios/email/{email}"));
            await AssertStatusAsync(HttpStatusCode.NotFound, await admin.GetAsync("/api/usuarios/999999"));
            await AssertStatusAsync(HttpStatusCode.NotFound, await admin.GetAsync("/api/usuarios/email/nao.existe@teste.com"));
        }

        [Fact]
        public async Task Cliente_NaoAcessaAsRotasDeAdministrador_Retorna403()
        {
            var cliente = await NovoClienteLogadoAsync();
            var id = await ObterMeuIdAsync(cliente);

            await AssertStatusAsync(HttpStatusCode.Forbidden, await cliente.GetAsync($"/api/usuarios/{id}"));
            await AssertStatusAsync(HttpStatusCode.Forbidden, await cliente.GetAsync("/api/usuarios/email/a@b.com"));
            await AssertStatusAsync(HttpStatusCode.Forbidden, await cliente.DeleteAsync($"/api/usuarios/{id}"));
        }

        [Fact]
        public async Task Admin_ExcluiUsuarioSemPedidos_ERecusaQuemTemPedidos()
        {
            var admin = await LogarComoAdminAsync();
            var livroId = await Fixture.CriarLivroAsync(estoque: 10);

            var comPedido = await NovoClienteLogadoAsync();
            var idComPedido = await ObterMeuIdAsync(comPedido);
            await ComprarAsync(comPedido, livroId, 1);

            var semPedido = await NovoClienteLogadoAsync();
            var idSemPedido = await ObterMeuIdAsync(semPedido);

            await AssertStatusAsync(HttpStatusCode.Conflict, await admin.DeleteAsync($"/api/usuarios/{idComPedido}"));
            await AssertStatusAsync(HttpStatusCode.NoContent, await admin.DeleteAsync($"/api/usuarios/{idSemPedido}"));
            await AssertStatusAsync(HttpStatusCode.NotFound, await admin.GetAsync($"/api/usuarios/{idSemPedido}"));
        }
    }
}