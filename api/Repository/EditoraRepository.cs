using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class EditoraRepository : IEditoraRepository
    {
        private readonly AppDbContext _dbContext;

        public EditoraRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Editora> CreateAsync(Editora editoraModel)
        {
            await _dbContext.Editora.AddAsync(editoraModel);
            await _dbContext.SaveChangesAsync();
            return editoraModel;
        }

        public async Task<Editora?> DeleteAsync(int id)
        {
            var editoraModel = await _dbContext.Editora.FirstOrDefaultAsync(e => e.Id == id);

            if(editoraModel == null)
                return null;

            _dbContext.Editora.Remove(editoraModel);
            await _dbContext.SaveChangesAsync();

            return editoraModel;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Editora.AnyAsync(e => e.Id == id);
        }

        public async Task<List<Editora>> GetAllAsync()
        {
            return await _dbContext.Editora
                   .AsNoTracking()
                   .OrderBy(e => e.Nome)
                   .ToListAsync();
        }

        public async Task<Editora?> GetByIdAsync(int id)
        {
            return await _dbContext.Editora.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Editora?> GetByNameAsync(string nome)
        {
            return await _dbContext.Editora.FirstOrDefaultAsync(e => e.Nome == nome);
        }

        public async Task<bool> PossuiLivrosAsync(int id)
        {
            return await _dbContext.Livro.AnyAsync(l => l.EditoraId == id);
        }

        public async Task<List<Editora>> SearchByNameAsync(string nome)
        {
            var termo = nome.Trim();

            return await _dbContext.Editora.AsNoTracking().Where(e => e.Nome.Contains(termo)).OrderBy(e =>e.Nome).ToListAsync();
        }

        public async Task<Editora?> UpdateAsync(int id, Editora editora)
        {
            var editoraExistente = await _dbContext.Editora.FirstOrDefaultAsync(e => e.Id == id);

            if(editoraExistente == null)
                return null;
            
            editoraExistente.Nome = editora.Nome;
            await _dbContext.SaveChangesAsync();

            return editoraExistente;
        }
    }
}