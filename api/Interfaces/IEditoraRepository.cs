using api.Models;

namespace api.Interfaces
{
    public interface IEditoraRepository
    {
        Task<List<Editora>> GetAllAsync();
        Task<Editora?> GetByIdAsync(int id);
        Task<List<Editora>> SearchByNameAsync(string nome);
        Task<Editora?> GetByNameAsync(string nome);
        Task<Editora> CreateAsync(Editora editoraModel);
        Task<Editora?> UpdateAsync(int id, Editora editora);
        Task<Editora?> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> PossuiLivrosAsync(int id);

    }
}