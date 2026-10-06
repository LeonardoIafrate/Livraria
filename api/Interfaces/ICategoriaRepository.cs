using api.Models;

namespace api.Interfaces
{
    public interface ICategoriaRepository
    {
            Task<List<Categoria>> GetAllAsync();
            Task<Categoria?> GetByIdAsync(int id);
            Task<List<Categoria>> GetByIdsAsync(List<int> ids);
            Task<List<Categoria>> SearchByGeneroAsync(string genero);
            Task<Categoria?> GetByGeneroAsync(string genero);
            Task<Categoria> CreateAsync(Categoria categoria);
            Task<Categoria?> UpdateAsync(int id, Categoria categoria);
            Task<Categoria?> DeleteAsync(int id);
            Task<bool> ExistsAsync(int id);
            Task<bool> PossuiLivroAsync( int id);
    }
}