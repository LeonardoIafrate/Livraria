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
    public class LivroRepository : ILivroRepository
    {
        private readonly AppDbContext _dbContext;

        public LivroRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Livro> CreateAsync(Livro livroModel)
        {
            await _dbContext.Livro.AddAsync(livroModel);
            await _dbContext.SaveChangesAsync();

            await _dbContext.Entry(livroModel).Reference(l => l.Editora).LoadAsync();
            return livroModel;
        }

        public async Task<Livro?> DeleteAsync(int id)
        {
            var livroModel = await _dbContext.Livro.FirstOrDefaultAsync(l => l.Id == id);

            if(livroModel == null)
                return null;

            _dbContext.Livro.Remove(livroModel);
            await _dbContext.SaveChangesAsync();
            return livroModel;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.Livro.AnyAsync(l => l.Id == id);
        }

        public async Task<List<Livro>> GetAllAsync()
        {
            return await _dbContext.Livro.AsNoTracking().Include(l => l.Editora).OrderBy(l => l.Nome).ToListAsync();
        }

        public async Task<Livro?> GetByIdAsync(int id)
        {
            return await _dbContext.Livro.Include(l => l.Editora).FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Livro?> GetByIsbnAsync(string isbn)
        {
            return await _dbContext.Livro.Include(l => l.Editora).FirstOrDefaultAsync(l => l.Isbn == isbn);
        }

        public async Task<List<Livro>> SearchByNameAsync(string nome)
        {
            var termo = nome.Trim();

            return await _dbContext.Livro
                   .AsNoTracking()
                   .Include(l => l.Editora)
                   .Where(l => l.Nome.Contains(termo))
                   .OrderBy(l => l.Nome)
                   .ToListAsync();
        }

        public async Task<Livro?> UpdateAsync(int id, Livro livroModel)
        {
            var livroExistente = await _dbContext.Livro.FirstOrDefaultAsync(l => l.Id == id);

            if(livroExistente == null)
                return null;
            
            livroExistente.Nome = livroModel.Nome;
            livroExistente.Sinopse = livroModel.Sinopse;
            livroExistente.AnoLancamento = livroModel.AnoLancamento;
            livroExistente.Isbn = livroModel.Isbn;
            livroExistente.Preco = livroModel.Preco;
            livroExistente.NumeroPaginas = livroModel.NumeroPaginas;
            livroExistente.Idioma = livroModel.Idioma;
            livroExistente.EditoraId = livroModel.EditoraId;

            await _dbContext.SaveChangesAsync();
            await _dbContext.Entry(livroExistente).Reference(l => l.Editora).LoadAsync();
            return livroExistente;
            
        }
    }
}