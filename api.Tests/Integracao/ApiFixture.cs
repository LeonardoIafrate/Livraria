using api.Data;
using api.Models;
using api.Tests.Integracao;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace api.Tests.Integracao
{
    public class ApiFixture : IAsyncLifetime
    {
        public const string EmailAdmin = "admin.teste@livraria.com";
        public const string SenhaAdmin = "Admin12345";

        private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

        private WebApplicationFactory<Program>? _fabrica;
        private string _conexao = string.Empty;

        public HttpClient CriarCliente() => _fabrica!.CreateClient();

        private AppDbContext CriarContextoDeApoio()
        {
            var opcoes = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(_conexao).Options;
            return new AppDbContext(opcoes);
        }

        public async Task<int> CriarLivroAsync(decimal preco = 30m, int estoque = 10)
        {
            await using var contexto = CriarContextoDeApoio();

            var livro = new Livro
            {
                Nome = $"Livro {Guid.NewGuid():N}",
                Isbn = Random.Shared.NextInt64(1_000_000_000_000, 10_000_000_000_000).ToString(),
                Preco = preco,
                Editora = new Editora { Nome = $"Editora {Guid.NewGuid():N}" },
                Estoque = new Estoque { Quantidade = estoque }
            };

            contexto.Set<Livro>().Add(livro);
            await contexto.SaveChangesAsync();

            return livro.Id;
        }

        public async Task<int> ObterEstoqueAsync(int livroId)
        {
            await using var contexto = CriarContextoDeApoio();

            return await contexto.Set<Estoque>().Where(e => e.LivroId == livroId).Select(e => e.Quantidade).SingleAsync();
        }

        public async ValueTask InitializeAsync()
        {
            await _container.StartAsync();

            var conexao = new SqlConnectionStringBuilder(_container.GetConnectionString())
            {
              InitialCatalog = "LivrariaApiTestes",
              TrustServerCertificate = true 
            }.ConnectionString;

            _conexao = conexao;

            Environment.SetEnvironmentVariable("ConnectionString__DefaulConnection", conexao);
            Environment.SetEnvironmentVariable("Jwt__Chave", "chave-somente-para-testes-com-mais-de-32-caracteres");
            Environment.SetEnvironmentVariable("Loggin__LogLevel__Default", "Warning");

            var opcoes = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(conexao).Options;

            await using (var contexto = new AppDbContext(opcoes))
            {
                await contexto.Database.MigrateAsync();

                var admin = new Usuario
                {
                    Nome = "Administrador de Teste",
                    Email = EmailAdmin,
                    Cpf = CpfDeTeste.Novo(),
                    DataNascimento = new DateTime(1985, 1, 1),
                    Perfil = PerfilUsuario.Admin
                };
                admin.SenhaHash = new PasswordHasher<Usuario>().HashPassword(admin, SenhaAdmin);

                contexto.Set<Usuario>().Add(admin);
                await contexto.SaveChangesAsync();
            }

            _fabrica = new WebApplicationFactory<Program>().WithWebHostBuilder(construtor =>
            {
                construtor.UseEnvironment("Testing");

                construtor.ConfigureLogging(logging =>
                {
                    logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                    logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
                    logging.AddFilter("Microsoft.Hosting", LogLevel.Warning);
                });

                construtor.ConfigureServices(servicos =>
                {
                   servicos.RemoveAll<DbContextOptions<AppDbContext>>(); 
                   servicos.AddDbContext<AppDbContext>(o => o.UseSqlServer(conexao));
                });
            });
        }

        private async Task VerificarQueApiUsaOBancoDoContainer(string conexaoEsperada)
        {
            using var escopo = _fabrica!.Services.CreateScope();

            var banco = escopo.ServiceProvider.GetRequiredService<AppDbContext>().Database.GetDbConnection();

            var esperado = new SqlConnectionStringBuilder(conexaoEsperada);

            if (banco.Database != esperado.InitialCatalog || banco.DataSource != esperado.DataSource)
            {
                throw new InvalidOperationException(
                    $"A API de teste está usando o '{banco.DataSource}' / '{banco.Database}', não o container" +
                    $"('{esperado.DataSource}' / '{esperado.InitialCatalog}'). Os testes forma interrompidos para não alterar o banco errado."
                    );
            }
        }

        public async ValueTask DisposeAsync()
        {
            if(_fabrica != null)
                await _fabrica.DisposeAsync();

            await _container.DisposeAsync();
        }
    }
}

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
}