using api.Helpers;
using api.Models;

namespace api.Interfaces
{
    public interface ILivroRepository
    {
        Task<PagedResult<Livro>> GetAllAsync(LivroQueryObject query);
        Task<Livro?> GetByIdAsync(int id);
        Task<List<Livro>> SearchByNameAsync(string nome);
        Task<Livro?> GetByIsbnAsync(string isbn);
        Task<Livro> CreateAsync(Livro livroModel);
        Task<Livro?> UpdateAsync(int id, Livro livroModel);
        Task<Livro?> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}