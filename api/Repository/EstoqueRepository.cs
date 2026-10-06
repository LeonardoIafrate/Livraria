using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                                   .OrderBy(e => e.Id)
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
                    .FirstOrDefaultAsync(e => e.LivroId == livroId);
        }
        
        public async Task<Estoque> CreateAsync(Estoque estoqueModel)
        {
            await _dbContext.Estoque.AddAsync(estoqueModel);
            await _dbContext.SaveChangesAsync();

            return estoqueModel;
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

        public async Task<Estoque?> DeleteAsync(int id)
        {
            var estoqueModel = await _dbContext.Estoque.FirstOrDefaultAsync(e => e.Id == id);
            if(estoqueModel == null)
                return null;

            _dbContext.Estoque.Remove(estoqueModel);
            await _dbContext.SaveChangesAsync();

            return estoqueModel;
        }

    }
}