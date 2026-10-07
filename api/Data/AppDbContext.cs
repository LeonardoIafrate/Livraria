using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Editora> Editora => Set<Editora>();
        public DbSet<Livro> Livro =>Set<Livro>();
        public DbSet<Categoria> Categoria => Set<Categoria>();
        public DbSet<Autor> Autor => Set<Autor>();
        public DbSet<Estoque> Estoque => Set<Estoque>();
        public DbSet<Usuario> Usuario => Set<Usuario>();
        public DbSet<Endereco> Endereco => Set<Endereco>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}