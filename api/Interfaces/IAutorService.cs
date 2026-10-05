using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IAutorService
    {
        Task<List<Autor>> GetAllAsync(string? nome);
        Task<Autor?> GetByIdAsync(int id);
        Task<List<Autor>> GetByIdsAsync(List<int> ids);
        Task<Autor> CreateAutorAsync(Autor autor);
        Task<Autor?> UpdateAutorAsync(int id, Autor autor);
        Task<Autor?> DeleteAutorAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> PossuiLivrosAsync(int id);
    }
}