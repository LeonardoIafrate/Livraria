using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IEstoqueRepository
    {
        Task<List<Estoque>> GetAllAsync();
        Task<Estoque?> GetByIdAsync(int id);
        Task<Estoque?> GetByLivroIdAsync(int livroId);
        Task<Estoque> CreateAsync(Estoque estoqueModel);
        Task<Estoque?> UpdateAsync(int id, Estoque estoqueModel);
        Task<Estoque?> DeleteAsync(int id);
    }
}