using api.Dtos.Endereco;

namespace api.Interfaces
{
    public interface IEnderecoService
    {
        Task<List<EnderecoDto>> GetByUsuarioIdAsync(int usuarioId);
        Task<EnderecoDto> GetByIdAsync(int id, int usuarioId);
        Task<EnderecoDto> CreateAsync(int usuarioId, CreateEnderecoDto dto);
        Task<EnderecoDto> UpdateAsync(int id, int usuarioId,UpdateEnderecoDto dto);
        Task<EnderecoDto> DefinirPrincipalAsync(int id, int usuarioId);
        Task ExcluirAsync(int id, int usuarioId);
    }
}