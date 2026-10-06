using api.Dtos.Autor;

namespace api.Interfaces
{
    public interface IAutorService
    {
        Task<List<AutorDto>> GetAllAsync(string? nome);
        Task<AutorDto> GetByIdAsync(int id);
        Task<AutorDto> CreateAutorAsync(CreateAutorDto dto);
        Task<AutorDto> UpdateAutorAsync(int id, UpdateAutorDto dto);
        Task DeleteAutorAsync(int id);
    }
}