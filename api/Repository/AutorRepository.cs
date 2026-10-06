using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class AutorRepository : IAutorRepository
    {
        private readonly AppDbContext _dbContext;

        public AutorRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<Autor>> GetAllAsync()
        {
            return await _dbContext.Autor.AsNoTracking().OrderBy(a => a.Nome).ToListAsync();
        }

        public async Task<Autor?> GetByIdAsync(int id)
        {
            return await _dbContext.Autor.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Autor>> GetByIdsAsync(List<int> ids)
        {
            return await _dbContext.Autor.Where(a => ids.Contains(a.Id)).ToListAsync();
        }

        public async Task<List<Autor>> SearchByNomeAsync(string nome)
        {
            var termo = nome.Trim();

            return await _dbContext.Autor
                            .AsNoTracking()
                            .Where(a => a.Nome.Contains(termo))
                            .OrderBy(a => a.Nome)
                            .ToListAsync();
        }

        public async Task<Autor> CreateAsync(Autor autor)
        {
            await _dbContext.Autor.AddAsync(autor);
            await _dbContext.SaveChangesAsync();
            return autor;
        }

        public async Task<Autor?> UpdateAsync(int id, Autor autor)
        {
            var existente = await _dbContext.Autor.FirstOrDefaultAsync(a => a.Id == id);

            if(existente == null)
                return null;
            
            existente.Nome = autor.Nome;
            existente.Biografia = autor.Biografia;
            existente.Nacionalidade = autor.Nacionalidade;

            await _dbContext.SaveChangesAsync();

            return existente;
        }

        public async Task<Autor?> DeleteAsync(int id)
        {
            var autorModel = await _dbContext.Autor.FirstOrDefaultAsync(a => a.Id == id);

            if(autorModel == null)
                return null;

            _dbContext.Autor.Remove(autorModel);
            await _dbContext.SaveChangesAsync();

            return autorModel;
        }

        public async Task<bool> Exists(int id)
        {
            return await _dbContext.Autor.AnyAsync(a => a.Id == id);
        }

        public async Task<bool> PossuiLivros(int id)
        {
            return await _dbContext.Autor.AnyAsync(a => a.Id == id && a.Livros.Any());
        }
    }
}