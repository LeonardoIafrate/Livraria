using api.Models;

namespace api.Interfaces
{
    public interface IEditoraRepository
    {
        Task<List<Editora>> GetAllAsync();
        Task<Editora?> GetByIdAsync(int id);
        Task<Editora?> GetByNomeAsync(string nome);
        Task<Editora> CreateAsync(Editora editoraModel);
        Task<Editora?> UpdateAsync(int id, Editora editora);
        Task<Editora?> DeleteAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task<bool> PossuiLivrosAsync(int id);

    }
}