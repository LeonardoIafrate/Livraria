using api.Models;

namespace api.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<List<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetByCpfAsync(string cpf);
        Task<List<Usuario>> SearchByNomeAsync(string nome);
        Task<Usuario> CreateAsync(Usuario usuarioModel);
        Task<Usuario?> UpdateAsync(int id, Usuario usuarioModel);
        Task<bool> ExistsAsync(int id);
        Task<Usuario?> DeleteAsync(int id);
        Task<Usuario?> DesativarAsync(int id);
    }
}