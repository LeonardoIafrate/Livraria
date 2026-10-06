using api.Models;

namespace api.Interfaces
{
    public interface IEstoqueRepository
    {
        Task<List<Estoque>> GetAllAsync();
        Task<Estoque?> GetByIdAsync(int id);
        Task<List<Estoque>> SearchByNomeLivroAsync(string nomeLivro);
        Task<Estoque?> GetByLivroIdAsync(int livroId);
        Task<Estoque?> UpdateAsync(int id, Estoque estoqueModel);
    }
}