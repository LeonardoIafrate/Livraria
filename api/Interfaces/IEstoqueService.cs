using api.Dtos.Estoque;

namespace api.Interfaces
{
    public interface IEstoqueService
    {
        Task<List<EstoqueDto>> GetAllAsync(string? nomeLivro);
        Task<EstoqueDto> GetByIdAsync(int id);
        Task<EstoqueDto> GetByLivroIdAsync(int livroId);
        Task<EstoqueDto> UpdateAsync(int id, UpdateEstoqueDto dto);
    }
}