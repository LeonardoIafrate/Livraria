using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class EstoqueRepository : IEstoqueRepository
    {
        private readonly AppDbContext _dbContext;

        public EstoqueRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<Estoque>> GetAllAsync()
        {
            return await _dbContext.Estoque
                                   .AsNoTracking()
                                   .Include(e => e.Livro)
                                   .OrderBy(e => e.Livro.Nome)
                                   .ToListAsync();
        }

        public async Task<Estoque?> GetByIdAsync(int id)
        {
            return await _dbContext.Estoque.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Estoque?> GetByLivroIdAsync(int livroId)
        {
            return await _dbContext.Estoque
                    .Include(e => e.Livro)
                    .OrderBy(e => e.Livro.Nome)
                    .FirstOrDefaultAsync(e => e.LivroId == livroId);
        }

        public async Task<List<Estoque>> SearchByNomeLivroAsync(string nomeLivro)
        {
            var termo = nomeLivro.Trim();

            return await _dbContext.Estoque
                    .AsNoTracking()
                    .Where(e => e.Livro.Nome
                    .Contains(termo))
                    .OrderBy(e => e.Livro.Nome)
                    .ToListAsync();
        }

        public async Task<Estoque?> UpdateAsync(int id, Estoque estoqueModel)
        {
            var estoqueExistente = await _dbContext.Estoque.FirstOrDefaultAsync(e => e.Id == id);
            if(estoqueExistente == null)
                return null;

            estoqueExistente.Quantidade = estoqueModel.Quantidade;
            await _dbContext.SaveChangesAsync();

            return estoqueExistente;
        }

    }
}