using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace api.Interfaces
{
    public interface IAutorRepository
    {
        Task<List<Autor>> GetAllAsync();
        Task<Autor?> GetByIdAsync(int id);
        Task<List<Autor>> GetByIdsAsync(List<int> ids);
        Task<List<Autor>> SearchByNomeAsync(string nome);
        Task<Autor> CreateAsync(Autor autor);
        Task<Autor?> UpdateAsync(int id, Autor autor);
        Task<Autor?> DeleteAsync(int id);
        Task<bool> Exists(int id);
        Task<bool> PossuiLivros(int id);
    }
}