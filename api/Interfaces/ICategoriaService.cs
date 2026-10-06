using api.Dtos.Categoria;

namespace api.Interfaces
{
    public interface ICategoriaService
    {
        Task<List<CategoriaDto>> GetAllAsync(string? genero);
        Task<CategoriaDto> GetByIdAsync(int id);
        Task<CategoriaDto> CreateAsync(CreateCategoriaDto createDto); 
        Task<CategoriaDto> UpdateAsync(int id, UpdateCategoriaDto updateDto);
        Task DeleteAsync(int id);
    }
}