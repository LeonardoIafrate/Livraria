using api.Models;

namespace api.Interfaces
{
    public interface IEnderecoRepository
    {
        Task<List<Endereco>> GetAllAsync();
        Task<Endereco?> GetByIdAsync(int id);
        Task<Endereco> CreateAsync(Endereco enderecoModel);
        Task<Endereco?> UpdateAsync(int id, Endereco enderecoModel);
        Task<Endereco?> DeleteAsync(int id); 
    }
}