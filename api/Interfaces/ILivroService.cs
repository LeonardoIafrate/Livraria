using api.Dtos.Livro;
using api.Helpers;

namespace api.Interfaces
{
    public interface ILivroService
    {
        Task<PagedResult<LivroDto>> GetAllAsync(LivroQueryObject query);
        Task<LivroDto> GetByIdAsync(int id);
        Task<LivroDto> GetByIsbnAsync(string isbn);
        Task<LivroDto> CreateAsync(CreateLivroDto createDto);
        Task<LivroDto> UpdateAsync(int id, UpdateLivroDto updateDto);
        Task DeleteAsync(int id);
    }
}