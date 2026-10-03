using api.Helpers;
using api.Models;

namespace api.Interfaces
{
    public interface ICategoriaRepository
    {
            Task<PagedResult<Categoria>> GetAllAsync();
            Task<Categoria?> GetByIdAsync(int id);
            Task<List<Categoria>> SearchByNameAsync(string nome);
            Task<Categoria?> GetByNameAsync(string nome);
            Task<Categoria> CreateAsync(Categoria categoria);
            Task<Categoria?> UpdateAsync(int id, Categoria categoria);
            Task<Categoria?> DeleteAsync(int id);
            Task<bool> ExistsAsync(int id);
            Task<bool> PossuiLivroAsync( int id);
    }
}