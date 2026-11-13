using api.Data;
using api.Models;
using api.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace api.Tests.Banco
{
    [Collection("SqlServer")]
    public class UsuarioRepositorioTests
    {
        private static readonly DateTime Bloqueio = new(2026, 11, 5, 12, 15, 0, DateTimeKind.Utc);

        private readonly SqlServerFixture _fixture;

        public UsuarioRepositorioTests(SqlServerFixture fixture) => _fixture = fixture;

        private static async Task<Usuario> SalvarAsync(AppDbContext contexto, Usuario usuario)
        {
            contexto.Set<Usuario>().Add(usuario);
            await contexto.SaveChangesAsync();
            return usuario;
        }

        [Fact]
        public async Task AtualizarBloqueio_GravaSoOsDoisCampos()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var usuario = await SalvarAsync(contexto, DadosDeTeste.NovoUsuario());

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            await new UsuarioRepository(outro).AtualizarBloqueioAsync(usuario.Id, 3, Bloqueio);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            var gravado = await conferencia.Set<Usuario>().SingleAsync(u => u.Id == usuario.Id);

            Assert.Equal(3, gravado.TentativasFalhas);
            Assert.Equal(Bloqueio, gravado.BloqueadoAte);
            Assert.Equal("Ana Souza", gravado.Nome);
            Assert.Equal("hash-de-teste", gravado.SenhaHash);
        }

        [Fact]
        public async Task AtualizarBloqueio_ComNull_LimpaOBloqueio()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var bloqueado = DadosDeTeste.NovoUsuario();
            bloqueado.TentativasFalhas = 4;
            bloqueado.BloqueadoAte = Bloqueio;
            await SalvarAsync(contexto, bloqueado);

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            await new UsuarioRepository(outro).AtualizarBloqueioAsync(bloqueado.Id, 0, null);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            var gravado = await conferencia.Set<Usuario>().SingleAsync(u => u.Id == bloqueado.Id);

            Assert.Equal(0, gravado.TentativasFalhas);
            Assert.Null(gravado.BloqueadoAte);
        }

        [Fact]
        public async Task AtualizarBloqueio_NaoAfetaOutrosUsuarios()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var alvo = await SalvarAsync(contexto, DadosDeTeste.NovoUsuario());
            var outroUsuario = await SalvarAsync(contexto, DadosDeTeste.NovoUsuario("bia@gmail.com", "11144477735"));

            await using var escrita = _fixture.NovoContextoDoMesmoBanco(contexto);
            await new UsuarioRepository(escrita).AtualizarBloqueioAsync(alvo.Id, 5, Bloqueio);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            var intacto = await conferencia.Set<Usuario>().SingleAsync(u => u.Id == outroUsuario.Id);

            Assert.Equal(0, intacto.TentativasFalhas);
            Assert.Null(intacto.BloqueadoAte);
        }

        [Fact]
        public async Task AtualizarBloqueio_IdInexistente_NaoLancaENaoAltera()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var usuario = await SalvarAsync(contexto, DadosDeTeste.NovoUsuario());

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            await new UsuarioRepository(outro).AtualizarBloqueioAsync(9999, 5, Bloqueio);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            var gravado = await conferencia.Set<Usuario>().SingleAsync(u => u.Id == usuario.Id);
            Assert.Equal(0, gravado.TentativasFalhas);
        }

        [Fact]
        public async Task UpdateAsync_NaoMexeNoBloqueio()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var bloqueado = DadosDeTeste.NovoUsuario();
            bloqueado.TentativasFalhas = 3;
            bloqueado.BloqueadoAte = Bloqueio;
            await SalvarAsync(contexto, bloqueado);

            var modelo = new Usuario
            {
                Nome = "Ana Maria",
                Email = "ana.maria@gmail.com",
                DataNascimento = new DateTime(1991, 1, 1)
            };

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            await new UsuarioRepository(outro).UpdateAsync(bloqueado.Id, modelo);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            var gravado = await conferencia.Set<Usuario>().SingleAsync(u => u.Id == bloqueado.Id);

            Assert.Equal("Ana Maria", gravado.Nome);
            Assert.Equal(3, gravado.TentativasFalhas);
            Assert.Equal(Bloqueio, gravado.BloqueadoAte);
        }

        [Fact]
        public async Task GetByEmail_IgnoraMaiusculasEMinusculas()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            await SalvarAsync(contexto, DadosDeTeste.NovoUsuario("ana@gmail.com"));

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var encontrado = await new UsuarioRepository(consulta).GetByEmailAsync("ANA@GMAIL.COM");

            Assert.NotNull(encontrado);
        }

        [Fact]
        public async Task GetByCpf_EncontraSoOCpfExato()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            await SalvarAsync(contexto, DadosDeTeste.NovoUsuario());

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var repo = new UsuarioRepository(consulta);

            Assert.NotNull(await repo.GetByCpfAsync("52998224725"));
            Assert.Null(await repo.GetByCpfAsync("11144477735"));
        }

        [Fact]
        public async Task SearchByNome_BuscaPorParteDoNomeSemDiferenciarMaiusculas()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            await SalvarAsync(contexto, DadosDeTeste.NovoUsuario());
            var outro = DadosDeTeste.NovoUsuario("bia@gmail.com", "11144477735");
            outro.Nome = "Bia Lima";
            await SalvarAsync(contexto, outro);

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var resultado = await new UsuarioRepository(consulta).SearchByNomeAsync("souza");

            Assert.Equal("Ana Souza", Assert.Single(resultado).Nome);
        }

        [Fact]
        public async Task GetAll_OrdenaPeloNome()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var zeca = DadosDeTeste.NovoUsuario("zeca@gmail.com", "11144477735");
            zeca.Nome = "Zeca";
            await SalvarAsync(contexto, zeca);
            await SalvarAsync(contexto, DadosDeTeste.NovoUsuario());

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var resultado = await new UsuarioRepository(consulta).GetAllAsync();

            Assert.Equal(new[] { "Ana Souza", "Zeca" }, resultado.Select(u => u.Nome));
        }

        [Fact]
        public async Task PossuiPedidos_VerdadeiroSoParaQuemComprou()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var livro = DadosDeTeste.NovoLivro(DadosDeTeste.NovaEditora());
            var pedido = DadosDeTeste.NovoPedido(DadosDeTeste.NovoUsuario(), livro);
            contexto.Set<Pedido>().Add(pedido);
            var semPedido = await SalvarAsync(contexto, DadosDeTeste.NovoUsuario("bia@gmail.com", "11144477735"));

            await using var consulta = _fixture.NovoContextoDoMesmoBanco(contexto);
            var repo = new UsuarioRepository(consulta);

            Assert.True(await repo.PossuiPedidosAsync(pedido.UsuarioId));
            Assert.False(await repo.PossuiPedidosAsync(semPedido.Id));
        }

        [Fact]
        public async Task DesativarAsync_DesativaOUsuarioETodosOsEnderecos()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var usuario = DadosDeTeste.NovoUsuario();
            usuario.Enderecos.Add(DadosDeTeste.NovoEndereco(principal: true, rua: "Rua A"));
            usuario.Enderecos.Add(DadosDeTeste.NovoEndereco(principal: false, rua: "Rua B"));
            await SalvarAsync(contexto, usuario);

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            var desativado = await new UsuarioRepository(outro).DesativarAsync(usuario.Id);

            Assert.NotNull(desativado);

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            var gravado = await conferencia.Set<Usuario>().Include(u => u.Enderecos).SingleAsync(u => u.Id == usuario.Id);

            Assert.False(gravado.Ativo);
            Assert.All(gravado.Enderecos, e =>
            {
                Assert.False(e.Ativo);
                Assert.False(e.Principal);
            });
        }

        [Fact]
        public async Task DesativarAsync_Inexistente_DevolveNull()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();

            Assert.Null(await new UsuarioRepository(contexto).DesativarAsync(9999));
        }

        [Fact]
        public async Task UpdateSenha_GravaOHashEDevolveFalseParaInexistente()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var usuario = await SalvarAsync(contexto, DadosDeTeste.NovoUsuario());

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            var repo = new UsuarioRepository(outro);

            Assert.True(await repo.UpdateSenhaAsync(usuario.Id, "novo-hash"));
            Assert.False(await repo.UpdateSenhaAsync(9999, "novo-hash"));

            await using var conferencia = _fixture.NovoContextoDoMesmoBanco(contexto);
            var gravado = await conferencia.Set<Usuario>().SingleAsync(u => u.Id == usuario.Id);
            Assert.Equal("novo-hash", gravado.SenhaHash);
        }

        [Fact]
        public async Task AlterarUsuarioAdmin_MudaOPerfil()
        {
            await using var contexto = await _fixture.CriarBancoLimpoAsync();
            var usuario = await SalvarAsync(contexto, DadosDeTeste.NovoUsuario());

            await using var outro = _fixture.NovoContextoDoMesmoBanco(contexto);
            var alterado = await new UsuarioRepository(outro).AlterarUsuarioAdminAsync(usuario.Id);

            Assert.Equal(PerfilUsuario.Admin, alterado!.Perfil);
        }
    }
}