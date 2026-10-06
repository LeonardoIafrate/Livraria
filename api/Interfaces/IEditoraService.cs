using api.Dtos.Editora;

namespace api.Interfaces
{
    public interface IEditoraService
    {
        Task<List<EditoraDto>> GetAllAsync(string? nome);
        Task<EditoraDto> GetByIdAsync(int id);
        Task<EditoraDto> CreateAsync(CreateEditoraDto createDto);
        Task<EditoraDto> UpdateAsync(UpdateEditoraDto updateDto, int id);
        Task DeleteAsync(int id);
    }
}