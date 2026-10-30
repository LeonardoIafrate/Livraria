using api.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Xunit;

namespace api.Tests.Banco
{
    public class SqlServerFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

        public async ValueTask DisposeAsync() => await _container.DisposeAsync();

        public async ValueTask InitializeAsync() => await _container.StartAsync();

        public async Task<AppDbContext> CriarBancoLimpoAsync()
        {
            var conexao = new SqlConnectionStringBuilder(_container.GetConnectionString())
            {
              InitialCatalog = $"Livaria_{Guid.NewGuid():N}",
              TrustServerCertificate = true  
            }.ConnectionString;

            var contexto = CriarContexto(conexao);
            await contexto.Database.MigrateAsync();

            return contexto;
        }

        public AppDbContext NovoContextoDoMesmoBanco(AppDbContext contexto) => CriarContexto(contexto.Database.GetConnectionString()!);

        private static AppDbContext CriarContexto(string conexao)
        {
            var opcoes = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(conexao).Options;
            return new AppDbContext(opcoes);
        }
    }

    [CollectionDefinition("SqlServer")]
    public class SqlServerCollection : ICollectionFixture<SqlServerFixture>
    {
    }
}