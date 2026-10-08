using api.Models;

namespace api.Interfaces
{
    public interface IEnderecoRepository
    {
        Task<List<Endereco>> GetByUsuarioIdAsync(int usuarioId);
        Task<Endereco?> GetByIdAsync(int id);
        Task<bool > PossuiEnderecoAtivo(int usuarioId);
        Task<Endereco> CreateAsync(Endereco enderecoModel);
        Task<Endereco?> UpdateAsync(int id, Endereco enderecoModel);
        Task<Endereco?> DefinirPrincipalAsync(int id);
        Task<Endereco?> DesativaAsync(int id); 
    }
}