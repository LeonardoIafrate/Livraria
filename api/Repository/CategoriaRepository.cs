using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Categoria>> GetAllAsync()
        {
            return await _context.Categoria.AsNoTracking().OrderBy(c => c.Genero).ToListAsync();
        }

        public async Task<Categoria?> GetByIdAsync(int id)
        {
            return await _context.Categoria.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Categoria>> GetByIdsAsync(List<int> ids)
        {
            return await _context.Categoria.Where(c => ids.Contains(c.Id)).ToListAsync();
        }

        public async Task<Categoria?> GetByGeneroAsync(string genero)
        {
            return await _context.Categoria.FirstOrDefaultAsync(c => c.Genero == genero);
        }
        public async Task<Categoria> CreateAsync(Categoria categoria)
        {
            await _context.Categoria.AddAsync(categoria);
            await _context.SaveChangesAsync();
            return categoria;
        }

        public async Task<List<Categoria>> SearchByGeneroAsync(string genero)
        {
            var termo = genero.Trim();

            return await _context.Categoria.AsNoTracking().Where(c => c.Genero.Contains(termo)).OrderBy(c => c.Genero).ToListAsync();
        }

        public async Task<Categoria?> UpdateAsync(int id, Categoria categoria)
        {
            var existente = await _context.Categoria.FirstOrDefaultAsync(c => c.Id == id);

            if(existente == null)
                return null;
            
            existente.Genero = categoria.Genero;
            await _context.SaveChangesAsync();

            return existente;
        }

        public async Task<Categoria?> DeleteAsync(int id)
        {
            var categoriaModel = await _context.Categoria.FirstOrDefaultAsync(c => c.Id == id);

            if (categoriaModel == null)
                return null;

            _context.Categoria.Remove(categoriaModel);
            await _context.SaveChangesAsync();

            return categoriaModel;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categoria.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> PossuiLivroAsync(int id)
        {
            return await _context.Categoria.AnyAsync(c => c.Id == id && c.Livros.Any());
        }

    }
}