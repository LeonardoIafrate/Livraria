using api.Dtos.Autor;

namespace api.Interfaces
{
    public interface IAutorService
    {
        Task<List<AutorDto>> GetAllAsync(string? nome);
        Task<AutorDto> GetByIdAsync(int id);
        Task<AutorDto> CreateAsync(CreateAutorDto dto);
        Task<AutorDto> UpdateAsync(int id, UpdateAutorDto dto);
        Task DeleteAsync(int id);
    }
}